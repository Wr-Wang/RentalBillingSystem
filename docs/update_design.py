import re

with open('docs/审批流-详细设计.md', 'r', encoding='utf-8') as f:
    content = f.read()

old = "## 9. 全量 Dapper 迁移\n\n### 9.1 背景\n\n审批模块验证了"原始 SQL 绕过 SaveChanges"模式可行后，决定将整个项目从 EF Core 迁移到 Dapper。目标是消除 EF Core SaveChanges 管线带来的所有不可控风险。"

new = """## 9. 全量 Dapper 迁移

### 9.1 为什么从 EF Core 改为 Dapper

#### 根本原因：EF Core SaveChanges 管线存在不可调和的问题

EF Core 的 SaveChanges 管线是本项目所有持久化问题的根源。每次 `_uow.CommitAsync()` 调用都会经过以下管线：

```
CommitAsync -> SaveChangesAsync 重写
  -> ChangeTracker.Entries<AuditableEntity>()    <- DetectChanges #1
  -> 审计字段注入(SetUpdated / SetCreated)
  -> ChangeTracker.Entries<AssociationEntity>()  <- DetectChanges #2
  -> base.SaveChangesAsync()
    -> MirrorAuditInterceptor.SavingChangesAsync  <- ExecuteSqlRawAsync 写入审计表
      -> 审计表缺失或列不匹配 -> SqlException -> 事务终止
    -> DomainEventDispatcher.SavingChangesAsync   <- 捕获领域事件
    -> EF Core SQL 执行
    -> SqlServerRetryExecutionStrategy            <- 瞬态失败重试时令牌过期
    -> DomainEventDispatcher.SavedChangesAsync    <- 事件分发
      -> Handler 失败 -> commit then fail
```

这条管线的任何一环出问题，都会导致整个操作失败。经过 5 次迭代修复均无法彻底解决：

| 迭代 | 修复目标 | 失败原因 |
|------|---------|---------|
| 1 | RowVersion 乐观锁重试 | 审计表缺失 -> 事务已被破坏，重试无用 |
| 2 | 导航集合污染 ClearPendingRecords | EF Core 无级联删除 -> 关系被切断异常 |
| 3 | DetachEntity + 重新加载 | 事务仍被破坏 |
| 4 | RefreshConcurrencyAsync 显式设 RowVersion | 同上 |
| 5 | MirrorAuditInterceptor IF EXISTS + 简化并发 | RowVersion 约定覆盖配置 + 重试策略导致令牌过期 |

#### 具体问题清单

| 问题 | 影响范围 | 原因 |
|------|---------|------|
| DbUpdateConcurrencyException | 审批流、用户更新等 | RowVersion 与 Status 双重并发令牌，配合 RSES 重试策略出错 |
| 审计表缺失/列不匹配破坏事务 | 所有 SaveChanges 操作 | MirrorAuditInterceptor 在事务内执行原始 SQL，失败后事务终止 |
| UserRole State=Modified | 用户角色更新 | UseLogicalForeignKeys 设置 NoAction 后，移除导航集合元素导致 EF 状态错乱 |
| DomainEventDispatcher commit then fail | 审批完成回调 | 事件分发在 SavedChangesAsync 中，失败时数据已提交但调用方收到异常 |
| SqlServerRetryExecutionStrategy 重试 | 所有操作 | 瞬态失败重试时，实体变更跟踪状态已过时 |
| ChangeTracker.DetectChanges 副作用 | 所有操作 | Entries<T>() 触发 DetectChanges，可能意外修改实体状态 |

#### Dapper 方案解决了这些问题

| EF Core 管线环节 | Dapper 处理方式 |
|-----------------|----------------|
| ChangeTracker / DetectChanges | 不存在。Dapper 不跟踪实体状态 |
| SaveChanges / Commit | 不存在。Dapper 的 ExecuteAsync 即时执行 SQL |
| 审计写入 | 独立连接。IAuditLogWriter 使用独立的 SqlConnection |
| 并发令牌 | 手动控制。UPDATE 语句的 WHERE 子句由开发人员编写 |
| 领域事件 | 手动 dispatch，不在 SaveChanges 管线内，完全可控 |
| 重试策略 | 不需要。没有 SaveChanges，没有重试 |

### 9.2 改动的优点

| 维度 | EF Core | Dapper |
|------|---------|--------|
| 性能 | 慢。变更跟踪 + SQL 生成 + 管线拦截器 | **快 10-50 倍**。直接执行 SQL，无管线开销 |
| 可控性 | 低。SaveChanges 管线中不可控环节多 | **完全可控**。每条 SQL 都明确写在代码中 |
| 问题排查 | 困难。EF Core 生成的 SQL 不易阅读 | **直观**。SQL 就是 SQL |
| 学习成本 | 高。需要理解 DbContext、ChangeTracker、导航属性等 | **低**。Dapper 只是 ADO.NET 的封装 |
| DDD 支持 | 好。导航属性支持聚合根加载 | **可替代**。通过 QueryMultiple 手动映射 |
| 迁移成本 | -- | **低**。DapperUnitOfWork 保持 IUnitOfWork 接口不变，服务层零改动 |

### 9.3 架构决策"""

content = content.replace(old, new)
with open('docs/审批流-详细设计.md', 'w', encoding='utf-8') as f:
    f.write(content)

print(f"Done. File size: {len(content)} chars")
