# RentalBillingSystem (RBS) — 住宅房屋租赁收租结算系统

> 一套面向中国住宅租赁市场的专业收租结算系统，覆盖房源管理、合同签订、租金应收、多渠道收款、账务处理、催缴管理、报表统计全链路业务闭环。

---

## 目录

- [项目概述](#项目概述)
- [技术栈](#技术栈)
- [系统架构](#系统架构)
- [核心功能](#核心功能)
- [快速开始](#快速开始)
- [项目结构](#项目结构)
- [开发指南](#开发指南)
- [API 文档](#api-文档)
- [部署](#部署)

---

## 项目概述

RentalBillingSystem（RBS）是一个面向住宅租赁市场的 **B2B 收租结算系统**，基于自研调度引擎（BackgroundService + 数据库驱动排期），覆盖从房源管理到会计凭证生成的完整业务闭环。

### 核心业务流程

```
房源管理 → 合同签订 → 租金应收 → 多渠道收款 → 账务处理 → 催缴管理 → 报表统计
                                            ↑
                                     自研调度引擎定时触发
```

### 适用场景

- 长租公寓运营/管理公司（集中式/分散式）
- 物业管理公司租赁业务
- 企业自有房屋租赁管理

---

## 技术栈

### 后端

| 技术 | 版本 | 用途 |
|------|------|------|
| .NET | 10.0 | 后端运行平台 |
| C# | 12+ | 开发语言（Nullable + ImplicitUsings） |
| ASP.NET Core | 10.0 | Web API 框架 |
| Dapper | 2.1.79 | 微 ORM（无 EF Core） |
| SQL Server | 2022+ | 数据库 |
| JWT Bearer | 10.0 | 认证（HS256, 120min） |
| BCrypt.Net | 4.2.0 | 密码哈希 |
| QuestPDF | 2024.12.0 | PDF 账单生成 |
| Serilog.AspNetCore | 10.0.0 | 结构化日志 |
| Swashbuckle | 10.2.3 | API 文档（Swagger） |
| Microsoft.Data.SqlClient | 6.1.1 | SQL Server 驱动 |
| Microsoft.AspNetCore.OpenApi | 10.0.9 | OpenAPI 支持 |

### 前端

| 技术 | 版本 | 用途 |
|------|------|------|
| Vue | 3.5.38 | 前端框架（Composition API） |
| Vite | 8.1.0 | 构建工具 |
| Element Plus | 2.14.2 | UI 组件库（中文语言包） |
| Pinia | 3.0.4 | 状态管理 |
| Vue Router | 4.6.4 | 路由管理 |
| Axios | 1.18.1 | HTTP 客户端 |
| ECharts | 6.1.0 | 数据可视化 |
| ExcelJS | 4.4.0 | Excel 导出 |
| vue-echarts | 8.0.1 | Vue ECharts 集成 |
| jsZip | 3.10.1 | ZIP 打包 |

---

## 系统架构

### 分层架构（Clean Architecture + DDD）

```
┌─────────────────────────────────────────────────────────────┐
│                   Presentation Layer                         │
│   RBS.Api (ASP.NET Web API) + Vue 3 SPA (web/)              │
│   ─ 45 个 Controller / 60+ 个前端页面 / JWT 认证 / 10 种角色             │
├─────────────────────────────────────────────────────────────┤
│                   Application Layer                          │
│   RBS.Application — 应用服务 / DTO / 用例编排                 │
│   ─ 合同服务 / 应收/收款服务 / 审批服务 / 银行对账 / 7 个定时任务      │
├─────────────────────────────────────────────────────────────┤
│                   Domain Layer (Core)                        │
│   RBS.Core — 领域实体 / 值对象 / 聚合根 / 仓储接口           │
│   ─ Contract / ReceivablePlan / Receipt / ApprovalRequest    │
│   ─ Money / Period / ContractStatus / BillingMode           │
├─────────────────────────────────────────────────────────────┤
│                   Infrastructure Layer                       │
│   RBS.Infrastructure — Dapper 数据访问 / PDF / 调度引擎      │
│   ─ SqlMaps.xml（~1800 行 SQL）/ 自定义调度引擎 / AuditLog   │
└─────────────────────────────────────────────────────────────┘
```

### DDD 构建块

| 概念 | 说明 |
|------|------|
| **聚合根** | Contract, ReceivablePlan, Receipt, User, ApprovalRequest |
| **值对象** | Money（金额+货币）, Period（账期 yyyy-MM）, ContractStatus（状态机） |
| **领域事件** | ContractActivatedEvent, ReceivableSettledEvent, ApprovalCompletedEvent 等 |
| **领域服务** | ContractDomainService, BillingDomainService, ApprovalDomainService, AccountingDomainService, ReceiptDomainService, DepositSettlementDomainService |
| **仓储** | `IRepository<T>` 泛型 + `DapperRepository<T>` 自动表名推断 |
| **工作单元** | `IUnitOfWork` + `DapperUnitOfWork`（快照变更追踪） |
| **多租户** | `IHasCompany` 接口 + 自动 CompanyId 过滤 |

### 数据访问

```
IUnitOfWork
  ├── IContractRepository          ← 合同仓储
  ├── IReceivablePlanRepository    ← 应收计划仓储
  ├── IReceiptRepository           ← 收款仓储
  ├── IApprovalRequestRepository   ← 审批请求仓储
  ├── IUserRepository              ← 用户仓储
  ├── IRepository<T> (泛型)        ← 30+ 通用仓储
  │     └── DapperRepository<T>    ← 自动表名推断、CompanyId 过滤
  └── CommitAsync() / BeginTransactionAsync()
```

所有 SQL 集中在 `src/RBS.Infrastructure/Data/SqlMaps/SqlMaps.xml`（~1800 行），命名规范：`{Domain}.{Action}.{Entity}.{Detail}`。

**数据访问设计要点**：
- **无 EF Core**：纯 Dapper + 手写 SQL，无存储过程
- **无外键约束**：本系统禁止使用外键约束，通过领域层保证数据一致性（见[数据库规范](memory/db-conventions.md)）
- **审计镜像表**：每张业务表对应一张 `{Table}_Audit` 镜像表（相同字段 + AuditId/AuditAction/AuditVersionNo/AuditChangedAt），通过应用层 `AuditLogWriter` 在事务提交后异步写入
- **乐观锁**：RowVersion 字段由应用层 DapperUnitOfWork 的 `CommitWithRetryAsync()` 负责（最多 3 次重试）
- **软删除**：统一使用 `IsActive = 0` 标记删除，非物理删除

### 定时调度引擎

```
SchedulingHostedService (BackgroundService, 60s 轮询)
  ├── JobScheduleGenerator (每小时生成待执行实例)
  ├── BillJob          每月25日 20:00  生成应收账单
  ├── SettleJob        每月1日 22:00   结算（含利息）
  ├── TerminateJob     每日 02:00     到期合同终止
  ├── ReceiptJob       每日 03:00     收款处理
  ├── AutoRenewJob     每日 00:00     自动续签
  ├── CollectionJob    每月15日 21:00 催缴
  └── RenewalReminderJob 每日 08:00  续签提醒
```

**并发策略**：
- **公司间并行**（Parallel.ForEachAsync），**公司内串行**（按 TargetDate 升序）
- **原子抢占**：UPDATE ... WHERE Status='Pending' 防止重复执行
- **依赖链**：上游失败 → 阻断下游；成功/跳过/取消均不阻断
- **心跳机制**：每个排期独立心跳线程（30 秒间隔），用于进程崩溃恢复
- **僵死恢复**：启动时检测心跳超时（任务 5 分钟/排期 10 分钟）并自动重置为 Pending

---

## 核心功能

### 合同管理

- **完整状态机**：Draft → PendingApproval → Active → Suspended/Expired/Terminated/Renewed
- **费用配置**：周期费用 + 一次性收费，版本化调价，按天分摊
- **多租户**：支持合租场景，主租户/合租标识
- **续签链**：合同续签形成链条，押金转移/重新收取
- **审批驱动**：创建/调价/终止/暂停/租户变更统一走审批流
- **变更历史**：所有操作可追溯

### 应收管理

- **自动生成**：每月定时生成应收计划（ReceivablePlan）
- **按天分摊**：月中入住/搬出/调价按天计算
- **多费用类型**：租金/物业费/押金/网络费等，固定金额/抄表计量
- **账单存储**：生成账单快照（DebitNote），支持多版本追溯和 PDF 导出

### 收款管理

- **多渠道**：银行转账/微信/支付宝/现金等
- **多计划分配**：一笔收款按比例分配到多笔应收
- **冲销/退款**：财务主管+总监双人复核

### 审批工作流

- **多级别**：0~N 级审批，每级指定审批角色
- **金额路由**：不同金额自动路由到不同级别
- **类型配置**：合同创建/调价/终止/续签/暂停等独立配置
- **事件回调**：审批通过/驳回后自动执行业务逻辑

### 会计

- **科目表**：支持树形科目结构（4 级编码体系）
- **自动分录**：每笔收款确认后自动生成记账凭证
- **日记账**：支持按科目/期间/对方科目多维度查询
- **总账**：逐笔追溯，支持按科目层级汇总
- **报表**：资产负债表、利润表，支持多公司合并报表

### 催缴管理

- **多阶段**：短信→电话→上门→法务，阶段可配置
- **自动触发**：逾期自动进入催缴流程
- **利息**：日利率/宽限期/上限可配置

### 银行对账

- **流水导入**：支持银行流水批量导入，自动去重
- **自动匹配**：按金额/日期自动匹配收款记录和银行流水
- **余额调节表**：自动生成银行余额调节表，追溯未达账项

### 批量导入

- **通用导入框架**：校验→暂存→提交审批→审批通过后执行
- **房源批量导入**：Excel 批量导入房屋信息，纳入审批流程
- **扩展点**：IImportTypeHandler 接口，支持扩展更多导入类型

### 续签管理

- **续签看板**：集中展示即将到期的合同
- **一键续签**：支持押金转移/新收，提交审批后自动处理
- **自动续签**：定时任务检测到期前 7 天的合同，自动提交续签审批
- **续签提醒**：提前 14 天通知运营人员合同即将到期

### 通知中心

- **系统通知**：审批待办、收款确认、逾期提醒统一汇聚
- **角色定向**：按角色推送通知，支持已读/未读管理

### 变更审计

- **全量追踪**：所有聚合根继承 AuditableEntity，自动记录创建/修改人、时间、IP、主机名
- **审计查询**：按实体类型/操作人/时间范围检索变更历史

### 其他

- **抄表管理**：水电气读数录入/导入，逾期自动估读
- **报表中心**：收租率/欠费明细/收款日报月报/出租率/费用收入统计
- **多公司**：集团模式，可切换公司视角，支持多公司合并报表
- **节假日管理**：配置节假日日历，影响应收计划生成和催缴计算
- **调度监控仪表盘**：可视化查看任务执行历史、耗时分布、失败详情
- **API 日志**：记录所有 API 请求/响应，支持按用户/路径/耗时检索
- **角色权限**：10 种预定义角色（Admin / OpsSupervisor / Operator / FinanceSupervisor / FinanceDirector / Accountant / DeptManager / GeneralManager / Legal），基于菜单+权限码的细粒度控制

---

## 快速开始

### 环境要求

- .NET 10.0 SDK
- SQL Server 2022+（Express/Developer 即可）
- Node.js 20+（前端）
- Git

### 数据库初始化

```bash
# 使用 sqlcmd 执行初始化脚本
sqlcmd -S . -U sa -P "your_password" -d master -i scripts/Init.sql
sqlcmd -S . -U sa -P "your_password" -d RBS -i scripts/SeedBase.sql
sqlcmd -S . -U sa -P "your_password" -d RBS -i scripts/SeedBiz.sql
```

### 启动后端

```bash
# 修改连接字符串
# src/RBS.Api/appsettings.json → ConnectionStrings.DefaultConnection

# 启动 API
cd src/RBS.Api
dotnet run
# 默认监听 https://localhost:5001
# Swagger: https://localhost:5001/swagger
```

### 启动前端

```bash
cd web
npm install
npm run dev
# 默认监听 http://localhost:5173
```

---

## 项目结构

```
RentalBillingSystem/
├── RBS.slnx                          # 解决方案文件
│
├── src/
│   ├── RBS.Core/                     # 领域层（60+ 实体）
│   │   ├── Entities/                 # 领域实体 + 值对象
│   │   │   ├── Base/                 # 基类 (AuditableEntity, AggregateRoot, ValueObjects)
│   │   │   ├── Contract/             # Contract, ContractFeeConfig, Tenant, 续签请求
│   │   │   ├── Billing/              # ReceivablePlan, Receipt, DebitNote, 催缴, 抄表
│   │   │   ├── Accounting/           # Voucher, JournalEntry, AccountingSubject, Period
│   │   │   ├── Approval/             # ApprovalRequest, ApprovalBizData, 级别/类型配置
│   │   │   ├── Banking/              # BankStatement, BankMatch, BankReconciliation
│   │   │   ├── Organization/         # User, Role, Company, Menu
│   │   │   ├── Property/             # HousingUnit, RoomType, FloorLevelBand, 定价标准
│   │   │   ├── Import/               # ImportBatch, ImportBatchItem
│   │   │   ├── Scheduling/           # TaskLog, TaskStepLog, ExecutionHeartbeat
│   │   │   └── SystemConfig/         # ApiLog, AutoRenewConfig, JobSchedule, 节假日等
│   │   ├── DomainServices/           # ContractDomainService, BillingDomainService
│   │   └── Interfaces/               # 仓储接口、UnitOfWork、领域服务接口
│   │
│   ├── RBS.Application/              # 应用层
│   │   ├── Services/                 # 应用服务（20+ 服务类）
│   │   │   ├── Contract/             # 合同服务、续签服务、租客服务
│   │   │   ├── Billing/              # 应收/收款/押金/账单/银行对账/日记账
│   │   │   ├── Accounting/           # 科目/凭证/自动凭证
│   │   │   ├── Approval/             # 审批服务
│   │   │   ├── Import/               # 通用导入服务
│   │   │   ├── Reporting/            # 报表服务
│   │   │   ├── Scheduling/           # 定时任务 (BillJob, SettleJob 等)
│   │   │   ├── Organization/         # 用户/角色/菜单/公司/认证
│   │   │   ├── Property/             # 房屋/房型/楼层/定价
│   │   │   └── SystemConfig/         # 调度/节假日/利息/税率/通知等
│   │   ├── DTOs/                     # 数据传输对象
│   │   ├── Common/Interfaces/        # 应用服务接口
│   │   └── EventHandlers/            # 领域事件处理器
│   │
│   ├── RBS.Infrastructure/           # 基础设施层
│   │   ├── Data/
│   │   │   ├── Repositories/         # Dapper 仓储实现（30+ 仓储）
│   │   │   ├── UnitOfWork/           # DapperUnitOfWork（快照变更追踪）
│   │   │   ├── SqlMaps/SqlMaps.xml   # 全部 SQL（~1800 行）
│   │   │   └── TypeHandlers/         # 值对象 Dapper 类型映射
│   │   ├── Scheduling/               # 调度引擎（心跳/僵死恢复/公司并行）
│   │   └── PdfGeneration/            # PDF 账单生成
│   │
│   └── RBS.Api/                      # API 层
│       ├── Controllers/              # 45 个 Controller
│       ├── Middleware/                # ExceptionLogging, ApiLogging, JWT
│       ├── Services/                  # CurrentUserService, TokenService, ApiLogWriter
│       ├── Program.cs                 # 启动入口
│       └── appsettings.json           # 配置文件
│
├── web/                              # Vue 3 前端（60+ 页面）
│   ├── src/
│   │   ├── views/                    # 页面组件
│   │   │   ├── dashboard/            # 仪表盘
│   │   │   ├── building/             # 房屋管理（列表/详情/批量导入）
│   │   │   ├── contract/             # 合同管理（列表/详情/新建）
│   │   │   ├── tenant/               # 租客管理
│   │   │   ├── receipt/              # 收款管理（登记/确认）
│   │   │   ├── bill/                 # 账单管理（列表/生成/预览）
│   │   │   ├── collection/           # 催缴管理（总览/配置/记录）
│   │   │   ├── meter/                # 抄表管理
│   │   │   ├── approval/             # 审批中心（待审批/我的提交/历史）
│   │   │   ├── accounting/           # 会计管理（科目/日记账/报表）
│   │   │   ├── bank/                 # 银行对账（流水导入/自动匹配/余额调节）
│   │   │   ├── report/               # 报表中心（收租率/欠费/日报/月报/收入/出租率/多公司）
│   │   │   ├── renewal/              # 待续签看板
│   │   │   ├── notification/         # 通知中心
│   │   │   ├── audit/                # 变更审计
│   │   │   ├── system/               # 系统设置（公司/用户/角色/菜单/审批/费用/调度等）
│   │   │   ├── login/                # 登录页
│   │   │   └── error/                # 404 页面
│   │   ├── store/                    # Pinia 状态管理
│   │   ├── router/                   # 路由配置（60+ 路由）
│   │   ├── api/                      # API 客户端
│   │   ├── layout/                   # 布局组件（MainLayout, RouteView）
│   │   └── components/               # 公共组件
│   └── package.json
│
├── docs/                             # 项目文档
│   ├── 需求文档.md
│   ├── 详细设计.md
│   └── ref/                          # 技术参考
│       └── 字段用途参考.md            # 应收生成日期字段职责说明
│
├── scripts/                          # 数据库脚本
│   ├── Init.sql                      # 建表（612KB, 60+ 张表）
│   ├── SeedBase.sql                  # 基础种子数据
│   ├── SeedBiz.sql                   # 业务种子数据
│   ├── DropAll.sql                   # 清库脚本
│   └── Cleanup.sql                   # 清理脚本
│
└── logs/                             # 运行日志（自动生成）
```

---

## 开发指南

### 命名规范

- **API 路由**：禁止使用连字符(`-`)，采用无分隔符小写形式（如 `trialbalance`、`feecodes`）
- **SQL Maps**：`{Domain}.{Action}.{Entity}.{Detail}`（如 `Identity.Select.User.ById`）
- **实体**：英文 PascalCase，属性使用 `{ get; private set; }` 模式
- **DTO**：后缀 `Dto` / `Request` / `Response`
- **前端路由**：path 小驼峰（如 `contracts/create`），name PascalCase（如 `ContractCreate`）

### 审批流开发模式

添加新的审批操作遵循以下步骤：

1. 在 `SqlMaps.xml` 中添加 `ApprovalBizData` 插入 SQL
2. 在 `SeedBase.sql` 中添加审批类型
3. 在 `ContractsController` 中添加端点（无审批直接执行，有审批存暂存→提审批）
4. 在 `ApprovalCompletedEventHandler` 中添加 case 分支
5. 在数据库中执行审批类型种子数据

### 约定优于配置

| 约定 | 说明 |
|------|------|
| **时间** | 全部使用 `ChinaTime.Now`（UTC+8） |
| **主键** | Guid + `NEWSEQUENTIALID()` |
| **审计字段** | CreatedBy/CreatedAt/UpdatedAt 由基类自动维护 |
| **多租户** | 实现 `IHasCompany` 接口自动获得公司级数据隔离 |
| **乐观锁** | RowVersion + `CommitWithRetryAsync()`（最多 3 次重试），由应用层负责 |
| **变更追踪** | 继承 AuditableEntity 自动获得审计日志；每张业务表对应 `*_Audit` 镜像表 |
| **软删除** | 统一 `IsActive = 0`，非物理删除 |
| **外键** | 禁止使用数据库外键约束，由领域层保证引用完整性 |
| **API 日志** | Channel 模式（Singleton Channel + BackgroundService 批量写入），非阻塞记录 |
| **JSON 序列化** | CamelCase + 忽略 null + 字典键 CamelCase |

### 角色与权限

系统预定义 10 种角色，通过菜单-权限码体系实现按钮级权限控制：

| 角色 | 编码 | 可见范围 |
|------|------|---------|
| 超级管理员 | `Admin` | 全部功能 + 系统设置 |
| 运营主管 | `OpsSupervisor` | 运营全模块 |
| 运营专员 | `Operator` | 合同/租客/抄表/催缴 |
| 财务主管 | `FinanceSupervisor` | 财务全模块 |
| 财务总监 | `FinanceDirector` | 审批/报表/审核 |
| 会计 | `Accountant` | 会计/凭证/报表 |
| 部门经理 | `DeptManager` | 合同/报表 |
| 总经理 | `GeneralManager` | 合同/报表/审批 |
| 法务 | `Legal` | 催缴/审批 |
| 未登录 | — | 仅登录页 |

权限控制从前端路由守卫→菜单可见性→API 请求认证三级实现，后端通过 JWT Claims + `[Authorize]` 属性最终裁决。

---

## API 文档

API 遵循 RESTful 风格，基础路径 `/api`。

| 模块 | 前缀 | 主要端点 |
|------|------|---------|
| 认证 | `/api/auth` | login, me |
| 合同 | `/api/contracts` | CRUD, terminate, suspend, resume, feeadjust, renewal |
| 应收 | `/api/receivables` | list, generate, preview |
| 收款 | `/api/receipts` | CRUD, confirm, reject, reverse |
| 审批 | `/api/approvals` | submit, approve, reject, pending, history |
| 审批类型/级别 | `/api/approvaltypes`, `/api/approvallevels` | CRUD |
| 租客 | `/api/tenants` | CRUD, search |
| 房屋 | `/api/housingunits` | CRUD, tree, import |
| 房型 | `/api/roomtypes` | CRUD |
| 费用 | `/api/feecodes` | CRUD |
| 楼层/定价 | `/api/floorlevelbands`, `/api/roompricingstandards` | CRUD |
| 账单 | `/api/debitnotes` | CRUD, preview, export PDF |
| 押金 | `/api/deposits` | CRUD, transfer, refund |
| 银行对账 | `/api/banking` | import, automatch, reconcile |
| 抄表 | `/api/meterreadings` | CRUD, confirm, estimate |
| 会计科目 | `/api/accountingsubjects` | tree |
| 凭证/日记账 | `/api/vouchers`, `/api/journalentries` | CRUD, post, reverse |
| 财务报表 | `/api/financialstatements` | balanceSheet, incomeStatement |
| 催缴 | `/api/collectionstages`, `/api/collectionrecords` | CRUD, auto |
| 利息 | `/api/interestconfig` | CRUD |
| 税率 | `/api/taxrateconfigs` | CRUD |
| 支付通道 | `/api/paymentchannels` | CRUD |
| 自动续签 | `/api/autorenewconfig` | CRUD |
| 节假日 | `/api/holidaycalendars` | CRUD |
| 通知 | `/api/notification` | list, read |
| 调度 | `/api/scheduler` | jobs, executions, trigger, monitor |
| 任务监控 | `/api/taskmonitor` | logs, steps, waterfull |
| 导入 | `/api/imports` | submit, batchDetail |
| 审计 | `/api/audit` | query |
| 日志 | `/api/apilogs`, `/api/systemlogs` | query |
| 组织 | `/api/users`, `/api/roles`, `/api/companies`, `/api/menus` | CRUD |
| 系统 | `/api/health` | healthcheck |

启动 API 后访问 `https://localhost:5001/swagger` 查看完整文档。

---

## 部署

### 生产环境推荐

| 组件 | 推荐 |
|------|------|
| 部署方式 | IIS / Windows Service |
| 反向代理 | Nginx / IIS ARR |
| 数据库 | SQL Server 2022 Standard+ |
| 前端 | Nginx 静态文件 |
| 日志 | Serilog + 文件 + 数据库 |
| 监控 | 内置调度监控页面 |

### 构建发布

```bash
# 后端
dotnet publish src/RBS.Api -c Release -o publish/api
# 前端
cd web && npm run build  # 输出到 web/dist
```

---

## License

Private / 企业内部使用
