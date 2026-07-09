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

RentalBillingSystem（RBS）是一个面向住宅租赁市场的 **B2B 收租结算系统**，以 BackgroundService + Cronos 为调度引擎，覆盖从房源管理到会计凭证生成的完整业务闭环。

### 核心业务流程

```
房源管理 → 合同签订 → 租金应收 → 多渠道收款 → 账务处理 → 催缴管理 → 报表统计
                                            ↑
                                      Cronos 定时调度
```

### 适用场景

- 公租公寓运营商寓管理公司（集中式/分散式）
- 长
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
| Serilog | 10.0 | 结构化日志 |
| QuestPDF | 2024.12 | PDF 账单生成 |
| Swashbuckle | 10.2 | API 文档（Swagger） |
| BCrypt.Net | 4.2 | 密码哈希 |

### 前端

| 技术 | 版本 | 用途 |
|------|------|------|
| Vue | 3.5 | 前端框架（Composition API） |
| Vite | 8.1 | 构建工具 |
| Element Plus | 2.14 | UI 组件库（中文语言包） |
| Pinia | 3.0 | 状态管理 |
| Vue Router | 4.6 | 路由管理 |
| Axios | 1.18 | HTTP 客户端 |
| ECharts | 6.1 | 数据可视化 |
| ExcelJS | 4.4 | Excel 导出 |

---

## 系统架构

### 分层架构（Clean Architecture + DDD）

```
┌─────────────────────────────────────────────────────────────┐
│                   Presentation Layer                         │
│   RBS.Api (ASP.NET Web API) + Vue 3 SPA (web/)              │
│   ─ 42 个 Controller / 28 个前端页面 / JWT 认证              │
├─────────────────────────────────────────────────────────────┤
│                   Application Layer                          │
│   RBS.Application — 应用服务 / DTO / 用例编排                 │
│   ─ 合同服务 / 应收服务 / 审批服务 / 7 个定时任务             │
├─────────────────────────────────────────────────────────────┤
│                   Domain Layer (Core)                        │
│   RBS.Core — 领域实体 / 值对象 / 聚合根 / 仓储接口           │
│   ─ Contract / ReceivablePlan / Receipt / ApprovalRequest    │
│   ─ Money / Period / ContractStatus / BillingMode           │
├─────────────────────────────────────────────────────────────┤
│                   Infrastructure Layer                       │
│   RBS.Infrastructure — Dapper 数据访问 / PDF / 调度引擎      │
│   ─ SqlMaps.xml（1300+ 行 SQL）/ 自定义调度引擎 / AuditLog   │
└─────────────────────────────────────────────────────────────┘
```

### DDD 构建块

| 概念 | 说明 |
|------|------|
| **聚合根** | Contract, ReceivablePlan, Receipt, User, ApprovalRequest |
| **值对象** | Money（金额+货币）, Period（账期 yyyy-MM）, ContractStatus（状态机） |
| **领域事件** | ContractActivatedEvent, ReceivableSettledEvent, ApprovalCompletedEvent 等 |
| **领域服务** | ContractDomainService, BillingDomainService |
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

所有 SQL 集中在 `src/RBS.Infrastructure/Data/SqlMaps/SqlMaps.xml`，命名规范：`{Domain}.{Action}.{Entity}.{Detail}`。

### 定时调度引擎

```
SchedulingHostedService (BackgroundService, 60s 轮询)
  ├── JobScheduleGenerator (每小时生成待执行实例)
  ├── BillJob          每月25日 20:00  生成应收账单
  ├── SettleJob        每月1日 22:00   结算（含滞纳金）
  ├── TerminateJob     每日 02:00     到期合同终止
  ├── ReceiptJob       每日 03:00     收款处理
  ├── AutoRenewJob     每日 00:00     自动续签
  ├── CollectionJob    每月15日 21:00 催缴
  └── RenewalReminderJob 每日 08:00  续签提醒
```

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

- **科目表**：支持树形科目结构
- **自动分录**：每笔收款自动生成记账凭证
- **凭证管理**：过账→审核→反过账

### 催缴管理

- **多阶段**：短信→电话→上门→法务，阶段可配置
- **自动触发**：逾期自动进入催缴流程
- **滞纳金**：日利率/宽限期/上限可配置

### 其他

- **抄表管理**：水电气读数录入/导入，逾期自动估读
- **银行对账**：导入流水→自动匹配→余额调节
- **报表中心**：收租率/欠费明细/收款日报月报/出租率
- **多公司**：集团模式，可切换公司视角

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
│   ├── RBS.Core/                     # 领域层
│   │   ├── Entities/                 # 领域实体 + 值对象
│   │   │   ├── Base/                 # 基类 (AuditableEntity, AggregateRoot, ValueObjects)
│   │   │   ├── Contract/             # Contract, ContractFeeConfig, Tenant
│   │   │   ├── Billing/              # ReceivablePlan, Receipt, DebitNote
│   │   │   ├── Accounting/           # Voucher, JournalEntry, AccountingSubject
│   │   │   ├── Approval/             # ApprovalRequest, ApprovalBizData
│   │   │   ├── Organization/         # User, Role, Company
│   │   │   └── Property/             # HousingUnit, RoomType
│   │   ├── DomainServices/           # ContractDomainService, BillingDomainService
│   │   └── Interfaces/               # 仓储接口、UnitOfWork、领域服务接口
│   │
│   ├── RBS.Application/              # 应用层
│   │   ├── Services/                 # 应用服务
│   │   │   ├── Contract/             # 合同服务
│   │   │   ├── Billing/              # 应收/收款/押金服务
│   │   │   ├── Approval/             # 审批服务
│   │   │   ├── Scheduling/           # 定时任务 (BillJob 等)
│   │   │   └── Organization/         # 组织架构服务
│   │   ├── DTOs/                     # 数据传输对象
│   │   ├── Common/Interfaces/        # 应用服务接口
│   │   └── EventHandlers/            # 领域事件处理器
│   │
│   ├── RBS.Infrastructure/           # 基础设施层
│   │   ├── Data/
│   │   │   ├── Repositories/         # Dapper 仓储实现
│   │   │   ├── UnitOfWork/           # DapperUnitOfWork
│   │   │   ├── SqlMaps/SqlMaps.xml   # 全部 SQL（1300+ 行）
│   │   │   └── TypeHandlers/         # 值对象 Dapper 类型映射
│   │   ├── Scheduling/               # 调度引擎
│   │   └── PdfGeneration/            # PDF 账单生成
│   │
│   └── RBS.Api/                      # API 层
│       ├── Controllers/              # 42 个 Controller
│       ├── Middleware/                # ExceptionLogging, ApiLogging, JWT
│       ├── Program.cs                 # 启动入口
│       └── appsettings.json           # 配置文件
│
├── web/                              # Vue 3 前端
│   ├── src/
│   │   ├── views/                    # 28 个页面
│   │   │   ├── contract/             # 合同管理（列表/详情/新建）
│   │   │   ├── tenant/               # 租客管理
│   │   │   ├── receipt/              # 收款管理
│   │   │   ├── bill/                 # 账单管理
│   │   │   ├── approval/             # 审批中心
│   │   │   ├── accounting/           # 会计管理
│   │   │   ├── report/               # 报表中心
│   │   │   └── system/               # 系统设置
│   │   ├── store/                    # Pinia 状态管理
│   │   ├── router/                   # 路由配置
│   │   ├── api/                      # API 客户端
│   │   └── components/               # 公共组件
│   └── package.json
│
├── docs/                             # 项目文档
│   ├── 需求文档.md
│   └── 详细设计.md
│
├── scripts/                          # 数据库脚本
│   ├── Init.sql                      # 建表（612KB）
│   ├── SeedBase.sql                  # 基础种子数据
│   └── SeedBiz.sql                   # 业务种子数据
│
└── logs/                             # 运行日志（自动生成）
```

---

## 开发指南

### 命名规范

- **API 路由**：禁止使用连字符(`-`)，采用无分隔符小写形式
- **SQL Maps**：`{Domain}.{Action}.{Entity}.{Detail}`（如 `Identity.Select.User.ById`）
- **实体**：英文 PascalCase，属性使用 `{ get; private set; }` 模式
- **DTO**：后缀 `Dto` / `Request` / `Response`

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
| **乐观锁** | RowVersion 字段由应用层负责，非数据库自动维护 |
| **变更追踪** | 继承 AuditableEntity 自动获得审计日志 |

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
| 租客 | `/api/tenants` | CRUD, search |
| 房屋 | `/api/housingunits` | CRUD, tree |
| 费用 | `/api/feecodes` | CRUD |
| 会计 | `/api/accountingsubjects` | tree |
| 调度 | `/api/scheduler` | jobs, executions, trigger |

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
