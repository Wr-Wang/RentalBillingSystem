using Dapper;
using Microsoft.Extensions.DependencyInjection;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Contract;
using RBS.Core.Common;
using RBS.Core.Entities.Approval;
using RBS.Core.Entities.Billing;
using RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;
using System.Data;

namespace RBS.Application.Services.Contract;

/// <summary>
/// 续签应用服务实现
/// </summary>
public class RenewalService : IRenewalService
{
    private readonly IUnitOfWork _uow;
    private readonly IDbConnectionFactory _db;
    private readonly IApprovalService _approvalService;
    private readonly ISqlLoader _sql;
    private readonly IServiceProvider _serviceProvider;

    public RenewalService(IUnitOfWork uow, IDbConnectionFactory db, IApprovalService approvalService, ISqlLoader sql, IServiceProvider serviceProvider)
    {
        _uow = uow;
        _db = db;
        _approvalService = approvalService;
        _sql = sql;
        _serviceProvider = serviceProvider;
    }

    public async Task<RenewalPreviewDto> PreviewAsync(Guid contractId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        conn.Open();

        var contract = await conn.QuerySingleOrDefaultAsync<dynamic>(@"
            SELECT c.Id, c.ContractNo, c.RentAmount, c.DepositAmount,
                   c.StartDate, c.EndDate, c.PaymentCycle, c.Status, c.RenewalCount,
                   r.FullCode AS RoomFullCode
            FROM Contracts c
            LEFT JOIN HousingUnits r ON r.Id = c.RoomId
            WHERE c.Id = @Id", new { Id = contractId });

        if (contract == null)
            throw new KeyNotFoundException("合同不存在");

        var dto = new RenewalPreviewDto
        {
            OldContract = new RenewalOldContractDto
            {
                Id = contract.Id, ContractNo = contract.ContractNo,
                RentAmount = contract.RentAmount, DepositAmount = contract.DepositAmount,
                StartDate = FormatDate(contract.StartDate), EndDate = FormatDate(contract.EndDate),
                PaymentCycle = contract.PaymentCycle, Status = contract.Status,
                RoomFullCode = contract.RoomFullCode, RenewalCount = contract.RenewalCount ?? 0
            }
        };

        // 查租客
        var tenants = await conn.QueryAsync<RenewalInheritedTenantDto>(@"
            SELECT ct.TenantId, t.Name AS TenantName, ct.IsPrimary
            FROM ContractTenants ct
            INNER JOIN Tenants t ON t.Id = ct.TenantId
            WHERE ct.ContractId = @Id", new { Id = contractId });
        dto.Tenants = tenants.ToList();

        // 查费用配置
        var fees = await conn.QueryAsync<RenewalInheritedFeeDto>(@"
            SELECT cf.FeeCodeId, fc.Name AS FeeName, cf.Amount, cf.BillingMode, cf.Unit, cf.UnitPrice
            FROM ContractFeeConfigs cf
            LEFT JOIN FeeCodes fc ON fc.Id = cf.FeeCodeId
            WHERE cf.ContractId = @Id AND cf.IsActive = 1", new { Id = contractId });
        dto.FeeConfigs = fees.ToList();

        // 欠费检查
        var outstanding = await conn.QuerySingleAsync<decimal>(@"
            SELECT ISNULL(SUM(rp.Amount - rp.Received), 0)
            FROM ReceivablePlans rp
            WHERE rp.ContractId = @Id AND rp.Status IN ('Pending', 'Partial', 'Overdue')",
            new { Id = contractId });
        dto.Checks.PaymentStatus = new PaymentStatusDto
        {
            Passed = outstanding <= 0,
            OutstandingAmount = outstanding
        };

        // 并发检查
        var hasPending = await conn.QuerySingleAsync<int>(@"
            SELECT COUNT(1) FROM ApprovalRequests
            WHERE ContractId = @Id AND Status = 'Pending'",
            new { Id = contractId });
        dto.Checks.ConcurrentApprovals = new ConcurrentApprovalsDto
        {
            HasPending = hasPending > 0,
            BlockedMessage = hasPending > 0 ? "该合同存在待审批的申请，请处理完成后再提交续签" : null
        };

        // 重复续签检查：是否已有合同指向本合同（PreviousContractId = 本合同Id）
        var renewedInfo = await conn.QuerySingleOrDefaultAsync<dynamic>(@"
            SELECT c.Id, c.ContractNo FROM Contracts c
            WHERE c.PreviousContractId = @Id",
            new { Id = contractId });
        if (renewedInfo != null)
        {
            dto.Checks.ConcurrentApprovals.AlreadyRenewed = true;
            dto.Checks.ConcurrentApprovals.RenewedContractId = renewedInfo.Id;
            dto.Checks.ConcurrentApprovals.RenewedContractNo = renewedInfo.ContractNo;
            dto.Checks.ConcurrentApprovals.BlockedMessage = $"该合同已被续签，新合同号为 {renewedInfo.ContractNo}，不可再次续签";
        }

        // 市场参考价（mock数据，后续对接真实数据源）
        dto.Checks.MarketPrice = new MarketPriceInfoDto
        {
            MinPrice = contract.RentAmount * 0.9m,
            MaxPrice = contract.RentAmount * 1.2m,
            AveragePrice = contract.RentAmount * 1.05m,
            SourceDescription = "同小区近3个月成交均价（模拟数据）"
        };

        // 默认续签信息
        var endDate = DateOnly.FromDateTime(DateTime.Today);
        if (contract.EndDate is DateOnly ed) endDate = ed;
        var suggestedStart = endDate.AddDays(1);
        var suggestedEnd = new DateOnly(suggestedStart.Year + 1, suggestedStart.Month, suggestedStart.Day).AddDays(-1);

        dto.DefaultRenewalInfo = new RenewalDefaultsDto
        {
            SuggestedStartDate = suggestedStart.ToString("yyyy-MM-dd"),
            SuggestedEndDate = suggestedEnd.ToString("yyyy-MM-dd"),
            CurrentRentAmount = contract.RentAmount
        };

        return dto;
    }

    /// <summary>统一格式化日期（处理 Dapper dynamic 返回 DateTime 的场景）</summary>
    private static string FormatDate(object? dateValue)
    {
        if (dateValue == null) return "";
        if (dateValue is DateTime dt) return dt.ToString("yyyy-MM-dd");
        if (dateValue is DateOnly d) return d.ToString("yyyy-MM-dd");
        return dateValue.ToString() ?? "";
    }

    public async Task<RenewalSubmitResultDto> SubmitAsync(SubmitRenewalRequest request, Guid userId, CancellationToken ct = default)
    {
        // 1. 统一并发检查
        await EnsureNoPendingApprovalAsync(request.ContractId, ct);

        // 2. 加载旧合同
        var oldContract = await _uow.Contracts.GetByIdAsync(request.ContractId, ct)
            ?? throw new KeyNotFoundException("合同不存在");

        if (oldContract.Status != "Active" && oldContract.Status != "Expired")
            throw new InvalidOperationException("只有生效中或已到期的合同可以续签");

        // 3. 重复续签检查：是否已有续签合同指向本合同
        using (var checkConn = _db.CreateConnection())
        {
            checkConn.Open();
            var existing = await checkConn.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT Id, ContractNo FROM Contracts WHERE PreviousContractId = @Id",
                new { Id = request.ContractId });
            if (existing != null)
                throw new InvalidOperationException($"该合同已被续签（新合同号：{existing.ContractNo}），不可再次续签");
        }

        // 4. 欠费检查
        using (var conn = _db.CreateConnection())
        {
            conn.Open();
            var outstanding = await conn.QuerySingleAsync<decimal>(@"
                SELECT ISNULL(SUM(rp.Amount - rp.Received), 0)
                FROM ReceivablePlans rp
                WHERE rp.ContractId = @Id AND rp.Status IN ('Pending', 'Partial', 'Overdue')",
                new { Id = request.ContractId });

            if (outstanding > 0)
                throw new InvalidOperationException($"该合同有未结清欠费 ¥{outstanding:N2}，请先处理后再续签");
        }

        // 4. 生成新合同号：剥离已有 -R{n} 后缀，基于原始号 + 续签次数
        var baseNo = oldContract.ContractNo.Split("-R").First();
        var newContractNo = $"{baseNo}-R{oldContract.RenewalCount + 1}";

        // 5. 创建 RenewalRequest
        var renewal = new RenewalRequest(
            oldContract.Id, newContractNo, oldContract.RentAmount,
            request.NewRentAmount, DateOnly.FromDateTime(DateTime.Parse(request.NewEndDate, System.Globalization.CultureInfo.InvariantCulture)));

        renewal.SetDepositInfo(
            request.DepositHandling, oldContract.DepositAmount, request.NewDepositAmount);
        renewal.SetMarketPrice(request.MarketReferencePrice);
        renewal.SetPaymentStatusCheck(true);
        renewal.SetRemark(request.Remark);
        // 注意：AddAsync 内部会调用 SetCreated 覆盖 CreatedBy/CreatedAt
        // 所以这里不手动设置，由仓储统一处理
        await _uow.RenewalRequests.AddAsync(renewal, ct);

        // 6. 提交审批
        var approvalType = await _uow.FindApprovalTypeByCodeAsync("CONTRACT_RENEW", ct);
        if (approvalType == null)
            throw new InvalidOperationException("未配置续签审批类型，请联系管理员");

        // 找审批级别配置
        var levels = await _uow.ApprovalLevelConfigs.GetAllAsync(ct);
        var typeLevels = levels.Where(l => l.ApprovalTypeId == approvalType.Id).ToList();
        var maxLevel = typeLevels.Count > 0 ? typeLevels.Max(l => l.Level) : 0;

        var approvalRequest = new ApprovalRequest(
            approvalType.Id,
            $"合同续签 - {oldContract.ContractNo}",
            renewal.Id,
            "ContractRenewal",
            oldContract.CompanyId,
            maxLevel);  // 直接传入 maxLevel，不再用反射

        // 设置创建人和合同ID
        approvalRequest.SetCreated(userId, ChinaTime.Now, null, null);
        approvalRequest.SetContractId(oldContract.Id);
        approvalRequest.AddRecord(userId, "Submitted",
            $"续签：月租 ¥{oldContract.RentAmount.Amount:N2} → ¥{request.NewRentAmount:N2}，" +
            $"到期日：{request.NewEndDate}，押金处理：{(request.DepositHandling == "TRANSFER" ? "原押金延续" : "重新收取")}");

        approvalRequest.Submit();
        await _uow.ApprovalRequests.AddAsync(approvalRequest, ct);

        // [事件] 提交后通知第一级审批人
        if (approvalRequest.Status == "Pending")
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider
                .GetRequiredService<IEventHandler<ApprovalSubmittedEvent>>();
            await handler.HandleAsync(
                new ApprovalSubmittedEvent(approvalRequest.Id, approvalRequest.ApprovalTypeId,
                    approvalRequest.TargetEntityId, approvalRequest.TargetEntityType, approvalRequest.Title),
                ct);
        }

        // 7. 提交审批（写入审批记录）
        var firstRecord = approvalRequest.Records.First();
        using (var updateConn = _db.CreateConnection())
        {
            updateConn.Open();
            await updateConn.ExecuteAsync(@"
                INSERT INTO ApprovalRecords (Id, ApprovalRequestId, Level, ApproverId, Action, Comment, CreatedBy, CreatedAt)
                VALUES (@Id, @ApprovalRequestId, @Level, @ApproverId, @Action, @Comment, @CreatedBy, @CreatedAt)",
                new
                {
                    firstRecord.Id, ApprovalRequestId = approvalRequest.Id,
                    firstRecord.Level, firstRecord.ApproverId, firstRecord.Action,
                    Comment = firstRecord.Comment ?? "", firstRecord.CreatedBy, firstRecord.CreatedAt
                });

            // 更新状态
            await updateConn.ExecuteAsync(
                "UPDATE ApprovalRequests SET Status = @Status, ContractId = @ContractId WHERE Id = @Id",
                new { approvalRequest.Status, ContractId = oldContract.Id, approvalRequest.Id });

            // 更新 RenewalRequest 状态
            await updateConn.ExecuteAsync(
                "UPDATE RenewalRequests SET Status = 'PendingApproval' WHERE Id = @Id",
                new { renewal.Id });
        }

        return new RenewalSubmitResultDto
        {
            RenewalRequestId = renewal.Id,
            ApprovalRequestId = approvalRequest.Id,
            Status = approvalRequest.Status,
            Message = "续签申请已提交，等待审批"
        };
    }

    public async Task ExecuteRenewalAsync(Guid renewalRequestId, CancellationToken ct = default)
    {
        var renewal = await _uow.RenewalRequests.GetByIdAsync(renewalRequestId, ct)
            ?? throw new KeyNotFoundException("续签请求不存在");

        if (renewal.Status != "PendingApproval")
            throw new InvalidOperationException($"续签请求状态为 {renewal.Status}，无法执行（仅 PendingApproval 可执行）");

        var oldContract = await _uow.Contracts.GetByIdAsync(renewal.OldContractId, ct)
            ?? throw new KeyNotFoundException("原合同不存在");

        if (oldContract.Status != "Active" && oldContract.Status != "Expired")
            throw new InvalidOperationException("原合同状态不允许续签");

        using var conn = _db.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            var now = ChinaTime.Now;
            var newId = Guid.NewGuid();

            // 1. 乐观锁：原子性地标记 RenewalRequest 防止重复执行
            var locked = await conn.ExecuteAsync(
                "UPDATE RenewalRequests SET Status = 'Executing', UpdatedAt = @Now WHERE Id = @Id AND Status = 'PendingApproval'",
                new { renewal.Id, Now = now }, tx);
            if (locked == 0)
                throw new InvalidOperationException("续签请求已被其他操作处理，请刷新后重试");

            // 2. 处理旧合同状态（区分 Active 和 Expired）
            int affected;
            if (oldContract.Status == "Expired")
            {
                // 已到期合同 → 标记 Renewed（终态）
                affected = await conn.ExecuteAsync(
                    "UPDATE Contracts SET Status = 'Renewed' WHERE Id = @Id AND Status = 'Expired'",
                    new { Id = renewal.OldContractId }, tx);
            }
            else
            {
                // 未到期合同 → 继续保持 Active，只校验状态未被修改
                affected = await conn.ExecuteAsync(
                    "UPDATE Contracts SET Status = 'Active' WHERE Id = @Id AND Status = 'Active'",
                    new { Id = renewal.OldContractId }, tx);
            }
            if (affected == 0)
                throw new InvalidOperationException("原合同状态已被修改，续签执行失败");

            // 3. 创建新合同
            var depositAmount = renewal.DepositHandling == "TRANSFER"
                ? renewal.OldDepositAmount
                : (renewal.NewDepositAmount ?? renewal.OldDepositAmount);

            var startDate = oldContract.EndDate.AddDays(1);

            await conn.ExecuteAsync(@"
                INSERT INTO Contracts (Id, ContractNo, RoomId, RentAmount, DepositAmount, StartDate, EndDate, PaymentCycle, Status, CompanyId,
                    PreviousContractId, RenewalCount, OriginalContractId, MarketPriceAtRenewal,
                    CreatedBy, CreatedAt)
                VALUES (@Id, @ContractNo, @RoomId, @RentAmount, @DepositAmount, @StartDate, @EndDate, @PaymentCycle, 'Active', @CompanyId,
                    @PreviousContractId, @RenewalCount, @OriginalContractId, @MarketPrice,
                    @CreatedBy, @CreatedAt)",
                new
                {
                    Id = newId, ContractNo = renewal.ContractNo,
                    RoomId = oldContract.RoomId, RentAmount = renewal.NewRent,
                    DepositAmount = depositAmount, StartDate = startDate,
                    EndDate = renewal.NewEndDate, PaymentCycle = oldContract.PaymentCycle,
                    CompanyId = oldContract.CompanyId,
                    PreviousContractId = oldContract.Id,
                    RenewalCount = oldContract.RenewalCount + 1,
                    OriginalContractId = oldContract.OriginalContractId ?? oldContract.Id,
                    MarketPrice = renewal.MarketReferencePrice,
                    CreatedBy = renewal.CreatedBy, CreatedAt = now
                }, tx);

            // 3. 复制租客
            var tenants = await conn.QueryAsync<dynamic>(
                "SELECT TenantId, IsPrimary FROM ContractTenants WHERE ContractId = @Id",
                new { Id = renewal.OldContractId }, tx);

            foreach (var t in tenants)
            {
                await conn.ExecuteAsync(@"
                    INSERT INTO ContractTenants (Id, ContractId, TenantId, IsPrimary, CreatedBy, CreatedAt)
                    VALUES (@Id, @ContractId, @TenantId, @IsPrimary, @CreatedBy, @CreatedAt)",
                    new { Id = Guid.NewGuid(), ContractId = newId, t.TenantId, t.IsPrimary, CreatedBy = renewal.CreatedBy, CreatedAt = now }, tx);
            }

            // 4. 复制费用配置
            var fees = await conn.QueryAsync<dynamic>(
                "SELECT FeeCodeId, BillingMode, Amount, Unit, UnitPrice FROM ContractFeeConfigs WHERE ContractId = @Id AND IsActive = 1",
                new { Id = renewal.OldContractId }, tx);

            foreach (var f in fees)
            {
                await conn.ExecuteAsync(@"
                    INSERT INTO ContractFeeConfigs (Id, ContractId, FeeCodeId, BillingMode, Amount, Unit, UnitPrice, IsActive, CreatedBy, CreatedAt)
                    VALUES (@Id, @ContractId, @FeeCodeId, @BillingMode, @Amount, @Unit, @UnitPrice, 1, @CreatedBy, @CreatedAt)",
                    new { Id = Guid.NewGuid(), ContractId = newId, f.FeeCodeId, f.BillingMode, f.Amount, f.Unit, f.UnitPrice, CreatedBy = renewal.CreatedBy, CreatedAt = now }, tx);
            }

            // 5. 押金处理
            if (renewal.DepositHandling == "TRANSFER")
            {
                // 原押金转移
                await conn.ExecuteAsync(@"
                    INSERT INTO DepositLogs (Id, ContractId, Amount, Balance, Action, Remark, CreatedBy, CreatedAt)
                    VALUES (@Id, @ContractId, @Amount, @Balance, @Action, @Remark, @CreatedBy, @CreatedAt)",
                    new
                    {
                        Id = Guid.NewGuid(), ContractId = renewal.OldContractId,
                        Amount = -renewal.OldDepositAmount, Balance = 0m,
                        Action = "TransferOut", Remark = "续签押金转出新合同",
                        CreatedBy = renewal.CreatedBy, CreatedAt = now
                    }, tx);

                await conn.ExecuteAsync(@"
                    INSERT INTO DepositLogs (Id, ContractId, Amount, Balance, Action, Remark, CreatedBy, CreatedAt)
                    VALUES (@Id, @ContractId, @Amount, @Balance, @Action, @Remark, @CreatedBy, @CreatedAt)",
                    new
                    {
                        Id = Guid.NewGuid(), ContractId = newId,
                        Amount = renewal.OldDepositAmount, Balance = renewal.OldDepositAmount,
                        Action = "TransferIn", Remark = "续签押金转入",
                        CreatedBy = renewal.CreatedBy, CreatedAt = now
                    }, tx);
            }
            else if (renewal.DepositHandling == "NEW")
            {
                var diff = (renewal.NewDepositAmount ?? 0) - renewal.OldDepositAmount;

                // 退旧押金
                await conn.ExecuteAsync(@"
                    INSERT INTO DepositLogs (Id, ContractId, Amount, Balance, Action, Remark, CreatedBy, CreatedAt)
                    VALUES (@Id, @ContractId, @Amount, 0, 'Refund', @Remark, @CreatedBy, @CreatedAt)",
                    new
                    {
                        Id = Guid.NewGuid(), ContractId = renewal.OldContractId,
                        Amount = -renewal.OldDepositAmount,
                        Remark = $"续签退押金，新押金 ¥{renewal.NewDepositAmount:N2}",
                        CreatedBy = renewal.CreatedBy, CreatedAt = now
                    }, tx);

                // 收新押金
                await conn.ExecuteAsync(@"
                    INSERT INTO DepositLogs (Id, ContractId, Amount, Balance, Action, Remark, CreatedBy, CreatedAt)
                    VALUES (@Id, @ContractId, @Amount, @Amount, 'Collection', @Remark, @CreatedBy, @CreatedAt)",
                    new
                    {
                        Id = Guid.NewGuid(), ContractId = newId,
                        Amount = renewal.NewDepositAmount,
                        Remark = "续签新收押金",
                        CreatedBy = renewal.CreatedBy, CreatedAt = now
                    }, tx);
            }

            // 6. 更新 RenewalRequest
            await conn.ExecuteAsync(
                "UPDATE RenewalRequests SET NewContractId = @NewContractId, Status = 'Completed', UpdatedAt = @Now WHERE Id = @Id",
                new { NewContractId = newId, renewal.Id, Now = now }, tx);

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task<List<RenewalHistoryDto>> GetHistoryAsync(Guid contractId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var rows = await conn.QueryAsync<RenewalHistoryDto>(@"
            SELECT Id, ContractNo, PreviousRent, NewRent,
                   CONVERT(NVARCHAR(10), NewEndDate, 23) AS NewEndDate,
                   DepositHandling, Status, CreatedAt, Remark, NewContractId
            FROM RenewalRequests
            WHERE OldContractId = @Id
            ORDER BY CreatedAt DESC", new { Id = contractId });
        return rows.ToList();
    }

    public async Task<List<RenewalChainNodeDto>> GetRenewalChainAsync(Guid contractId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        conn.Open();

        // 先找到原始合同（OriginalContractId 或自身）
        var rootId = await conn.QuerySingleAsync<Guid?>(@"
            SELECT COALESCE(OriginalContractId, Id) FROM Contracts WHERE Id = @Id",
            new { Id = contractId });

        if (rootId == null)
            return new List<RenewalChainNodeDto>();

        // 获取整条链
        var chain = await conn.QueryAsync<RenewalChainNodeDto>(@"
            WITH ContractChain AS (
                SELECT Id, ContractNo, Status, RentAmount, StartDate, EndDate, RenewalCount
                FROM Contracts WHERE Id = @RootId
                UNION ALL
                SELECT c.Id, c.ContractNo, c.Status, c.RentAmount, c.StartDate, c.EndDate, c.RenewalCount
                FROM Contracts c
                INNER JOIN ContractChain cc ON cc.Id = c.PreviousContractId
            )
            SELECT Id AS ContractId, ContractNo, Status, RentAmount,
                   CONVERT(NVARCHAR(10), StartDate, 23) AS StartDate,
                   CONVERT(NVARCHAR(10), EndDate, 23) AS EndDate, RenewalCount,
                   CASE WHEN Id = @TargetId THEN 1 ELSE 0 END AS IsCurrent
            FROM ContractChain
            ORDER BY RenewalCount", new { RootId = rootId, TargetId = contractId });
        return chain.ToList();
    }

    public async Task<ContractOperationsDto> GetAllowedOperationsAsync(Guid contractId, CancellationToken ct = default)
    {
        var contract = await _uow.Contracts.GetByIdAsync(contractId, ct);
        if (contract == null) throw new KeyNotFoundException("合同不存在");

        var dto = new ContractOperationsDto();

        if (contract.Status == "Renewed")
        {
            // 已续签合同完全只读
            dto.CanModifyRent = false;
            dto.CanTerminate = false;
            dto.CanRenew = false;
            dto.CanSuspend = false;
            dto.CanResume = false;
            dto.CanAdjustFee = false;
            return dto;
        }

        // 检查是否有待审批流
        using var conn = _db.CreateConnection();
        conn.Open();
        var pendingType = await conn.QuerySingleOrDefaultAsync<string>(@"
            SELECT at.Code FROM ApprovalRequests ar
            INNER JOIN ApprovalTypes at ON at.Id = ar.ApprovalTypeId
            WHERE ar.ContractId = @Id AND ar.Status = 'Pending'",
            new { Id = contractId });

        if (pendingType != null)
        {
            dto.PendingApprovalType = pendingType;
            dto.CanModifyRent = false;
            dto.CanTerminate = false;
            dto.CanRenew = false;
            dto.CanAdjustFee = false;
        }

        // 根据合同状态限制
        if (contract.Status != "Active")
        {
            dto.CanSuspend = false;
            dto.CanResume = contract.Status == "Suspended";
        }

        return dto;
    }

    public async Task<RejectedRenewalDto?> GetLastRejectedAsync(Guid contractId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        return await conn.QuerySingleOrDefaultAsync<RejectedRenewalDto>(
            _sql.Get("Lease.Select.RenewalRequest.LastRejected"),
            new { Id = contractId });
    }

    public async Task EnsureNoPendingApprovalAsync(Guid contractId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var hasPending = await conn.QuerySingleAsync<int>(
            "SELECT COUNT(1) FROM ApprovalRequests WHERE ContractId = @Id AND Status = 'Pending'",
            new { Id = contractId });

        if (hasPending > 0)
            throw new InvalidOperationException("该合同存在待审批的申请，请处理完成后再提交");
    }
}
