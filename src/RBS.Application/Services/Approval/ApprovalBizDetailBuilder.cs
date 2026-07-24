using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Approval;
using RBS.Core.Common;
using RBS.Core.Entities.Approval;
using RBS.Core.Entities.Contract;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;
using System.Text.RegularExpressions;

namespace RBS.Application.Services.Approval;

/// <summary>
/// 审批业务详情构建器 — 从结构化数据或旧版 Description 解析构建审批对比视图
/// 拆分自 ApprovalService，职责单一：只负责"审批详情页展示什么数据"
/// 新增审批业务类型时只需在此类中添加分支，不影响审批流程
/// </summary>
public class ApprovalBizDetailBuilder
{
    private readonly IUnitOfWork _uow;
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;

    public ApprovalBizDetailBuilder(IUnitOfWork uow, IDbConnectionFactory db, ISqlLoader sql)
    {
        _uow = uow;
        _db = db;
        _sql = sql;
    }

    /// <summary>
    /// 获取审批业务详情（新旧对比数据）
    /// 优先从 ApprovalBizData 结构化数据构建，无则回退 Description 正则解析
    /// </summary>
    /// <param name="approval">审批请求实体</param>
    /// <returns>业务详情 DTO，无数据时返回仅有标题的空对象</returns>
    public async Task<ApprovalBizDetailDto?> GetBizDetailAsync(ApprovalRequest approval)
    {
        // 优先从 ApprovalBizData 表读取结构化业务数据（新审批，含精确字段）
        var bizData = await _uow.ApprovalBizData.GetByApprovalRequestIdAsync(approval.Id);
        if (bizData != null)
        {
            try
            {
                return await BuildFromStructuredData(bizData, approval);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BizDetail] BuildFromStructuredData failed for approval {approval.Id}: {ex}");
            }
        }

        // 回退：旧审批无结构化数据时，从实体关联数据构建
        var fallback = await BuildFromDescriptionAsync(approval);
        if (fallback != null) return fallback;

        // 最终回退：至少返回标题
        return new ApprovalBizDetailDto
        {
            Title = approval.Title ?? "",
            BizType = approval.TargetEntityType,
            Fields = new List<BizFieldDto>()
        };
    }

    /// <summary>
    /// 从 ApprovalBizData 结构化数据构建详情
    /// 按 ChangeType 分发：RENT_ADJUST / FEE_ADJUST / RECEIVABLE_GENERATE / TERMINATE
    /// </summary>
    private async Task<ApprovalBizDetailDto?> BuildFromStructuredData(
        ApprovalBizData bizData, ApprovalRequest approval)
    {
        var dto = new ApprovalBizDetailDto
        {
            Title = approval.Title ?? "",
            EffectiveDate = bizData.EffectiveDate?.ToString("yyyy-MM-dd")
        };

        switch (bizData.ChangeType)
        {
            case "RENT_ADJUST":
                var diff = (bizData.NewAmount ?? 0) - (bizData.OldAmount ?? 0);
                var pct = bizData.OldAmount > 0 ? diff / bizData.OldAmount.Value * 100 : 0;
                dto.BizType = "RENT_ADJUST";
                dto.Fields = new List<BizFieldDto>
                {
                    new() { Label = "调整前月租", OldValue = $"¥{bizData.OldAmount:N2}" },
                    new() { Label = "调整后月租", NewValue = $"¥{bizData.NewAmount:N2}", IsChanged = true },
                    new() { Label = "调整差额",   NewValue = $"{(diff >= 0 ? "+" : "")}¥{diff:N2} ({(pct >= 0 ? "+" : "")}{pct:F1}%)", IsChanged = true },
                    new() { Label = "生效日期",   NewValue = bizData.EffectiveDate?.ToString("yyyy-MM-dd"), IsChanged = true },
                    new() { Label = "调整原因",   NewValue = bizData.Reason },
                };
                break;

            case "FEE_ADJUST":
                var feeItems = await _uow.ApprovalFeeItems.GetByApprovalRequestIdAsync(approval.Id);
                dto.BizType = "FEE_ADJUST";
                dto.Fields = new List<BizFieldDto>
                {
                    new() { Label = "调价项目数", NewValue = $"{feeItems.Count} 项", IsChanged = true },
                    new() { Label = "生效日期",   NewValue = bizData.EffectiveDate?.ToString("yyyy-MM-dd"), IsChanged = true },
                };

                // 逐项查询当前活跃配置，补充原数据信息（需独立连接，因处于事务外）
                dto.FeeItems = new List<BizFeeItemDto>();
                using (var conn = _db.CreateConnection())
                {
                    conn.Open();
                    foreach (var item in feeItems)
                    {
                        var oldConfig = conn.QuerySingleOrDefault(
                            _sql.Get("Lease.Select.ContractFeeConfig.FullCurrentByContractAndFee"),
                            new { ContractId = item.ContractId, FeeCodeId = item.FeeCodeId });
                        var chargeType = (string?)oldConfig?.ChargeType;
                        if (string.IsNullOrEmpty(chargeType))
                        {
                            var feeCodeInfo = conn.QuerySingleOrDefault(
                                _sql.Get("FeeCode.Select.FeeCode.ChargeTypeById"),
                                new { Id = item.FeeCodeId });
                            chargeType = (string?)feeCodeInfo?.ChargeType;
                        }
                        dto.FeeItems.Add(new BizFeeItemDto
                        {
                            FeeName = item.FeeName,
                            OldAmount = item.OldAmount,
                            NewAmount = item.NewAmount,
                            ChargeType = chargeType ?? "Recurring",
                            BillingMode = item.BillingMode,
                            EffectiveDate = item.EffectiveDate
                        });
                    }
                }
                break;

            case "RECEIVABLE_GENERATE":
                dto.BizType = "RECEIVABLE_GENERATE";
                dto.Fields = new List<BizFieldDto>
                {
                    new() { Label = "生成金额", NewValue = $"¥{bizData.NewAmount:N2}", IsChanged = true },
                    new() { Label = "原因", NewValue = bizData.Reason },
                };
                break;

            case "TERMINATE":
                dto.BizType = "TERMINATE";
                dto.Fields = new List<BizFieldDto>
                {
                    new() { Label = "实际终止日期", NewValue = bizData.ActualEndDate?.ToString("yyyy-MM-dd"), IsChanged = true },
                    new() { Label = "押金处理方式", NewValue = bizData.DepositReturn switch
                    {
                        "DEDUCTION" => "抵扣欠费后退还",
                        "FULL" => "全额退还",
                        "NO_REFUND" => "不退还",
                        _ => bizData.DepositReturn
                    }, IsChanged = true },
                    new() { Label = "终止原因", NewValue = bizData.Reason },
                };
                break;
        }
        return dto.Fields.Count > 0 ? dto : null;
    }

    /// <summary>
    /// 从实体关联数据构建详情（旧审批无 ApprovalBizData 时的回退路径）
    /// 按 TargetEntityType 分发：ContractRenewal / ContractActivation / ContractModify
    /// 最后尝试正则解析 Description 文本提取结构化数据（兼容遗留数据）
    /// </summary>
    private async Task<ApprovalBizDetailDto?> BuildFromDescriptionAsync(ApprovalRequest approval)
    {
        var desc = approval.Description;
        var dto = new ApprovalBizDetailDto { Title = approval.Title ?? "" };

        // === 续签审批：加载 RenewalRequest + 原合同信息 ===
        if (approval.TargetEntityType == "ContractRenewal" && approval.TargetEntityId != Guid.Empty)
        {
            var renewal = await _uow.RenewalRequests.GetByIdAsync(approval.TargetEntityId);
            if (renewal != null)
            {
                var oldContract = await _uow.Contracts.GetByIdAsync(renewal.OldContractId);
                dto.Fields.Add(new BizFieldDto { Label = "原合同号", OldValue = oldContract?.ContractNo, NewValue = renewal.ContractNo, IsChanged = true });
                dto.Fields.Add(new BizFieldDto { Label = "月租金", OldValue = $"¥{renewal.PreviousRent:N2}", NewValue = $"¥{renewal.NewRent:N2}", IsChanged = true });
                dto.Fields.Add(new BizFieldDto { Label = "到期日", OldValue = oldContract?.EndDate?.ToString("yyyy-MM-dd") ?? "不限", NewValue = renewal.NewEndDate.ToString("yyyy-MM-dd"), IsChanged = true });
                var oldDeposit = renewal.OldDepositAmount;
                var newDeposit = renewal.DepositHandling == "NEW" ? (renewal.NewDepositAmount ?? oldDeposit) : oldDeposit;
                dto.Fields.Add(new BizFieldDto { Label = "押金", OldValue = $"¥{oldDeposit:N2}", NewValue = $"¥{newDeposit:N2}", IsChanged = newDeposit != oldDeposit });
                dto.Fields.Add(new BizFieldDto { Label = "押金处理方式", OldValue = null, NewValue = renewal.DepositHandling == "TRANSFER" ? "原押金延续" : "重新收取", IsChanged = false });
                if (!string.IsNullOrEmpty(renewal.Remark))
                    dto.Fields.Add(new BizFieldDto { Label = "备注", OldValue = null, NewValue = renewal.Remark, IsChanged = true });
            }
            return dto.Fields.Count > 0 ? dto : null;
        }

        // === 合同创建审批：加载暂存请求中的房源/租客/费用信息 ===
        if (approval.TargetEntityType == "ContractActivation" && approval.TargetEntityId != Guid.Empty)
        {
            var req = await _uow.ContractCreateRequests.GetByIdAsync(approval.TargetEntityId);
            if (req != null)
            {
                using var conn = _db.CreateConnection(); conn.Open();
                var room = conn.QuerySingleOrDefault(_sql.Get("ContractCreate.Select.Room.FullCode"), new { Id = req.RoomId });
                var tenants = conn.Query(_sql.Get("ContractCreate.Select.Tenants.NamesByRequest"), new { Id = req.Id }).ToList();
                var fees = conn.Query(_sql.Get("ContractCreate.Select.Fees.NamesByRequest"), new { Id = req.Id }).ToList();

                dto.BizType = "ContractActivation";
                dto.Fields.Add(new BizFieldDto { Label = "合同编号", NewValue = req.ContractNo });
                dto.Fields.Add(new BizFieldDto { Label = "房屋", NewValue = room?.FullCode ?? "" });
                dto.Fields.Add(new BizFieldDto { Label = "起租日期", NewValue = req.StartDate.ToString("yyyy-MM-dd") });
                dto.Fields.Add(new BizFieldDto { Label = "到期日期", NewValue = req.EndDate?.ToString("yyyy-MM-dd") ?? "不限" });
                dto.Fields.Add(new BizFieldDto { Label = "付款周期", NewValue = req.PaymentCycle switch { "Monthly" => "月付", "Quarterly" => "季付", "HalfYearly" => "半年付", "Yearly" => "年付", _ => req.PaymentCycle } });
                dto.Fields.Add(new BizFieldDto { Label = "租客", NewValue = string.Join("、", tenants.Select(t => (string)t.Name)) });

                // 费用明细展示（区分一次性/周期性）
                dto.FeeItems = fees.Select(f => new BizFeeItemDto
                {
                    FeeName = f.Name, NewAmount = f.Amount,
                    ChargeType = f.ChargeType, BillingMode = f.BillingMode,
                    EffectiveDate = f.EffectiveDate
                }).ToList();
                var onceFees = fees.Where(f => f.ChargeType == "OneTime").ToList();
                var recFees = fees.Where(f => f.ChargeType == "Recurring").ToList();
                if (onceFees.Count > 0)
                    dto.Fields.Add(new BizFieldDto { Label = $"一次性费用（{onceFees.Count} 项）", NewValue = $"¥{onceFees.Sum(f => (decimal)f.Amount):N2}" });
                if (recFees.Count > 0)
                    dto.Fields.Add(new BizFieldDto { Label = $"周期性费用（{recFees.Count} 项）", NewValue = $"¥{recFees.Sum(f => (decimal)f.Amount):N2}/月" });
                dto.Fields.Add(new BizFieldDto { Label = $"费用合计（{fees.Count} 项）", NewValue = $"¥{fees.Sum(f => (decimal)f.Amount):N2}" });
            }
            return dto.Fields.Count > 0 ? dto : null;
        }

        // === 合同修改审批：展示新旧字段对比 ===
        if (approval.TargetEntityType == "ContractModify" && approval.TargetEntityId != Guid.Empty)
        {
            var req = await _uow.ContractModifyRequests.GetByIdAsync(approval.TargetEntityId);
            if (req != null)
            {
                using var conn = _db.CreateConnection(); conn.Open();
                var contract = conn.QuerySingleOrDefault<dynamic>(
                    _sql.Get("Lease.Select.Contract.Default"), new { Id = req.ContractId });
                var tenant = conn.QuerySingleOrDefault<dynamic>(
                    _sql.Get("Lease.Select.ContractTenant.PrimaryByContract"), new { Id = req.ContractId });
                var oldPhone = tenant?.TenantPhone as string ?? "";

                dto.BizType = "ContractModify";
                if (req.StartDate.HasValue)
                    dto.Fields.Add(new BizFieldDto { Label = "起租日期", OldValue = contract?.StartDate is DateTime sd ? sd.ToString("yyyy-MM-dd") : "", NewValue = req.StartDate.Value.ToString("yyyy-MM-dd"), IsChanged = true });
                if (req.EndDate.HasValue)
                    dto.Fields.Add(new BizFieldDto { Label = "到期日期", OldValue = contract?.EndDate is DateTime ed ? ed.ToString("yyyy-MM-dd") : "不限", NewValue = req.EndDate.Value.ToString("yyyy-MM-dd"), IsChanged = true });
                if (!string.IsNullOrEmpty(req.PaymentCycle))
                    dto.Fields.Add(new BizFieldDto { Label = "付款周期", OldValue = contract?.PaymentCycle, NewValue = req.PaymentCycle, IsChanged = true });
                if (req.PaymentDueDay.HasValue)
                    dto.Fields.Add(new BizFieldDto { Label = "付款到期日", OldValue = (contract?.PaymentDueDay?.ToString() ?? "5") + "日", NewValue = req.PaymentDueDay + "日", IsChanged = true });
                if (req.AllowDepositAsLastRent.HasValue)
                {
                    var oldVal = contract?.AllowDepositAsLastRent is bool b ? (b ? "是" : "否") : "否";
                    dto.Fields.Add(new BizFieldDto { Label = "押金抵最后月租", OldValue = oldVal, NewValue = req.AllowDepositAsLastRent.Value ? "是" : "否", IsChanged = true });
                }
                if (!string.IsNullOrEmpty(req.TenantPhone))
                    dto.Fields.Add(new BizFieldDto { Label = "租客电话", OldValue = oldPhone, NewValue = req.TenantPhone, IsChanged = true });
                if (!string.IsNullOrEmpty(req.Remark))
                    dto.Fields.Add(new BizFieldDto { Label = "备注", OldValue = null, NewValue = req.Remark, IsChanged = true });
            }
            return dto.Fields.Count > 0 ? dto : null;
        }

        // === 旧版 Description 正则解析（兼容遗留数据） ===
        // 针对 TargetEntityType="Contract" 的旧审批，从描述文本正则提取金额/日期/原因
        if (!string.IsNullOrEmpty(desc) && approval.TargetEntityType == "Contract" && approval.Title?.StartsWith("[合同终止]") == false)
        {
            var match = Regex.Match(desc, @"→\s*¥([\d,]+)");
            if (match.Success)
            {
                dto.BizType = "RENT_ADJUST";
                dto.Fields.Add(new BizFieldDto { Label = "调整后月租", NewValue = $"¥{match.Groups[1].Value}" });
                var dateMatch = Regex.Match(desc, @"生效日期[：:](\S+)");
                if (dateMatch.Success)
                    dto.Fields.Add(new BizFieldDto { Label = "生效日期", NewValue = dateMatch.Groups[1].Value });
                var reasonMatch = Regex.Match(desc, @"调整原因[：:](\S+)");
                if (reasonMatch.Success)
                    dto.Fields.Add(new BizFieldDto { Label = "调整原因", NewValue = reasonMatch.Groups[1].Value });
            }
        }
        else if (!string.IsNullOrEmpty(desc) && approval.TargetEntityType == "Contract" && approval.Title?.StartsWith("[合同终止]") == true)
        {
            // 旧终止审批：仅展示合同号和终止原因
            dto.Fields.Add(new BizFieldDto { Label = "合同号", OldValue = null, NewValue = approval.TargetEntityId.ToString(), IsChanged = false });
            dto.Fields.Add(new BizFieldDto { Label = "终止原因", OldValue = null, NewValue = approval.Description, IsChanged = true });
        }

        return dto.Fields.Count > 0 ? dto : null;
    }
}
