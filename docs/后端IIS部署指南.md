# RBS 后端 API IIS 部署指南

> 本文档指导你**手动将 RBS.Api（.NET 10 Web API）部署到 Windows IIS**，包含发布终端命令、IIS 建站步骤、配置修改与排错。
>
> 部署架构：
> ```
> 浏览器 ── HTTP ──▶ IIS ──(ASP.NET Core Module v2, inprocess)──▶ RBS.Api.exe ──▶ SQL Server
> ```

---

## 目录

- [0. 部署方式选择](#0-部署方式选择)
- [1. 服务器前置准备](#1-服务器前置准备)
- [2. 数据库准备](#2-数据库准备)
- [3. 发布命令（终端）](#3-发布命令终端)
- [4. 发布产物说明 + web.config](#4-发布产物说明--webconfig)
- [5. IIS 手动部署步骤](#5-iis-手动部署步骤)
- [6. 修改配置文件](#6-修改配置文件)
- [7. 启动与验证](#7-启动与验证)
- [8. 排错对照表](#8-排错对照表)
- [9. 更新发布流程](#9-更新发布流程)

---

## 0. 部署方式选择

| 方式 | 优点 | 缺点 | 建议 |
|---|---|---|---|
| **框架依赖**（推荐） | 发布包小（~几十 MB），更新方便 | 服务器必须装 .NET 10 Hosting Bundle | ✅ 常用 |
| **自包含** | 服务器无需装 .NET | 发布包大（~150 MB+），需指定运行时 | 服务器不便装 .NET 时用 |

本指南以**框架依赖**为主，自包含命令在 3.2 给出。

---

## 1. 服务器前置准备

### 1.1 安装 IIS

```powershell
# 管理员 PowerShell 一键开启 IIS 全部必要功能（含管理工具）
Install-WindowsFeature -Name Web-Server, Web-Mgmt-Console -IncludeManagementTools
```

或：控制面板 → 程序 → 启用或关闭 Windows 功能 → 勾选「Internet Information Services」。

### 1.2 安装 .NET 10 Hosting Bundle（关键！）

> **必须装 Hosting Bundle**，它包含：
> - ASP.NET Core Runtime 10.x
> - **ASP.NET Core Module v2**（IIS 转发请求给进程的模块，不装它必报 502.5）

1. 打开 https://dotnet.microsoft.com/download/dotnet/10.0
2. 下载 **Hosting Bundle**（Windows 版本）
3. 运行安装（安装过程中会提示关闭浏览器窗口）
4. 安装完成后**重启 IIS**，让模块加载：
   ```powershell
   iisreset
   ```

### 1.3 验证

```powershell
# 查看已安装的 .NET 运行时（应看到 Microsoft.AspNetCore.App 10.x）
dotnet --list-runtimes

# 确认 IIS 里有 ASP.NET Core Module v2
# IIS 管理器 → 服务器节点 → 模块 → 应存在 AspNetCoreModuleV2
```

---

## 2. 数据库准备

- SQL Server（2022+）可被 API 服务器访问（本机部署则无需配置，跨服务器需放行 **1433** 端口入站规则）。
- `RBS` 数据库已初始化（已执行 `scripts/Init.sql` + `SeedBase.sql` + `SeedBiz.sql`）。
- 记录可用的 **SQL 登录账号/密码**（用于改连接串）。

---

## 3. 发布命令（终端）

### 3.1 标准发布（框架依赖，推荐）

在**解决方案根目录**（`RBS.slnx` 所在目录）执行：

```bash
# -c Release    发布 Release 版
# -o publish/api 输出到根目录下的 publish/api 文件夹（可自定义）
dotnet publish src/RBS.Api -c Release -o publish/api
```

发布完成后输出类似：

```
已确定要还原的项目的依赖项...  (无需还原)
  RBS.Api -> publish/api\
```

### 3.2 自包含发布（服务器不装 .NET 时）

```bash
dotnet publish src/RBS.Api -c Release -r win-x64 --self-contained true -o publish/api
```

> `-r win-x64` 指定运行时平台；服务器是 32 位系统用 `win-x86`。

### 3.3 可选：编译前先还原/构建

```bash
dotnet restore src/RBS.Api
dotnet build src/RBS.Api -c Release
```

---

## 4. 发布产物说明 + web.config

### 4.1 关键产物（publish/api 下）

| 文件/目录 | 说明 |
|---|---|
| `RBS.Api.exe` / `RBS.Api.dll` | 应用本体 |
| `web.config` | **SDK 自动生成**的 IIS 配置（见下），不要删 |
| `appsettings.json` | 配置文件（部署时要改连接串，见第 6 章） |
| `Content/Fonts/SimHei.ttf` | PDF 账单中文字体（发布时自动拷贝） |
| `RBS.Infrastructure.dll` 等 | 各层程序集 |
| `logs/` | **运行时自动创建**，Serilog 每日日志 `rbs-YYYYMMDD.log` |

### 4.2 web.config（自动生成的标准内容）

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath=".\RBS.Api.exe" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="inprocess" />
    </system.webServer>
  </location>
</configuration>
```

关键属性说明：
- `hostingModel="inprocess"` — 进程内托管（默认，性能好）。若遇疑难 500.30，可临时改成 `outofprocess` 对比测试。
- `stdoutLogEnabled="false"` — 排错时可临时改 `true`，`stdout` 日志写到 `logs\stdout-*.log`；排查完记得改回 false。
- `processPath` — 指向 `RBS.Api.exe`，一般不用改。

---

## 5. IIS 手动部署步骤

### 5.1 准备发布目录

把 `publish/api` 整个目录复制到服务器，例如：`D:\sites\rbs-api\`（内容直接放该目录下，不要再套一层 publish/api）。

### 5.2 创建应用池

IIS 管理器 → **应用程序池** → 右键「添加应用程序池」：

| 配置项 | 值 |
|---|---|
| 名称 | `rbs-api-pool` |
| .NET CLR 版本 | **无托管代码**（No Managed Code）⚠️ 关键 |
| 托管管道模式 | 集成（Integrated） |
| 启动模式 | 可选（StartMode 默认 OnDemand 即可） |

> ASP.NET Core 不依赖 CLR 托管，必须选「无托管代码」。

### 5.3 创建站点

IIS 管理器 → **网站** → 右键「添加网站」：

| 配置项 | 值 |
|---|---|
| 网站名称 | `rbs-api` |
| 物理路径 | `D:\sites\rbs-api`（你的发布目录） |
| 应用程序池 | 选 `rbs-api-pool` |
| 绑定 | 类型 `http`，端口 `8080`（或你定的端口，主机名可留空） |

> 端口避开 80（若被其他站点占用）。**外部访问**记得在 Windows 防火墙放行该端口：
> ```powershell
> New-NetFirewallRule -DisplayName "RBS API 8080" -Direction Inbound -Protocol TCP -LocalPort 8080 -Action Allow
> ```

### 5.4 目录权限（重要）

给应用池身份授予发布目录权限（否则报 500 权限错误）：

```powershell
# 应用池默认身份是 IIS AppPool\rbs-api-pool
icacls "D:\sites\rbs-api" /grant "IIS AppPool\rbs-api-pool:(OI)(CI)R"        # 读 + 执行
icacls "D:\sites\rbs-api\logs" /grant "IIS AppPool\rbs-api-pool:(OI)(CI)M"   # logs 目录要可写（Serilog 写日志）
```

> 若没有 `logs` 目录，先在 `D:\sites\rbs-api` 下手动创建一个空的 `logs` 文件夹再授权。

---

## 6. 修改配置文件

### 6.1 appsettings.json（发布目录里改）

编辑 `D:\sites\rbs-api\appsettings.json`：

```jsonc
{
  "ConnectionStrings": {
    // ← 改成服务器能连的 SQL 地址和账号密码
    "DefaultConnection": "Server=你的SQL服务器;Database=RBS;User Id=sa;Password=换成强密码;TrustServerCertificate=true;MultipleActiveResultSets=true;Max Pool Size=100;Min Pool Size=10;Connection Timeout=30;"
  },
  "JwtSettings": {
    // ← 换成强随机密钥（≥32 字符），生产环境必须改
    "SecretKey": "换成至少32位的随机字符串xxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "Issuer": "RBS",
    "Audience": "RBS",
    "ExpirationMinutes": 120
  }
  // Amap.ApiKey 按需保留
}
```

改完 → **重启应用池/站点**生效（见 7.2）。

### 6.2 控制 Swagger 开关（`Swagger:Enabled`）

从 v1.x 起，Swagger 由**配置开关**控制，不再依赖环境变量：

- **开发环境**（`ASPNETCORE_ENVIRONMENT=Development`）——**始终开启**，无需配置。
- **生产环境**——默认**关闭**，需在 `appsettings.json` 中显式开启：

```jsonc
{
  "Swagger": {
    "Enabled": true   // true=开启接口文档 / false=关闭（默认建议 false）
  }
}
```

> ⚠️ **安全提醒**：`/swagger` 会暴露全部接口文档。生产环境对外网公开时，调试完请设回 `false`，或搭配防火墙/IP 白名单使用。
>
> 环境变量仍可用：设 `Swagger__Enabled=true`（IIS 系统环境变量）效果等同，两种方式二选一。

### 6.3 CORS（生产前端跨域时）

`Program.cs` 的 CORS 策略 `AllowWebApp` 目前只允许 `http://localhost:5173` 和 `5174`。若生产前端部署在**另一个域名**且跨域访问 API，需把前端域名加进白名单：

```csharp
policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "https://你的前端域名")
```

> 注意：`WithOrigins` + `AllowCredentials()` 的组合**不支持通配符**，必须写具体域名。
> 若前端与 API **同源**（由 IIS ARR / 同一站点反代），则不需要 CORS，可跳过本步。

---

## 7. 启动与验证

### 7.1 启动

- 应用池、站点创建完成后默认启动。若停止，IIS 管理器里右键「启动」。

### 7.2 验证接口

| 检查项 | 地址 | 期望 |
|---|---|---|
| 健康检查 | `http://服务器:8080/api/health` | 返回健康状态（200） |
| Swagger 接口文档 | `http://服务器:8080/swagger` | 开启条件：`Swagger:Enabled=true`（或开发环境） |

验证接口最直接的方式（服务器上或浏览器）：

```powershell
# 服务器本地测试
curl http://localhost:8080/api/health
```

> `Program.cs` 已确认存在 `/api/health` 健康检查端点。

### 7.3 验证前端联通

前端 `vite.config.js` 代理的 `target` 在开发环境指后端；**生产环境**前端构建后（`npm run build`），把 `/api` 请求反向代理到 IIS 的 `http://服务器:8080`（Nginx / IIS ARR），或前端直接改 `baseURL` 为 `http://服务器:8080`。

---

## 8. 排错对照表

| 现象 | 常见原因 | 处理 |
|---|---|---|
| **502.5 Process Failure** | Hosting Bundle 未装/版本不符；进程崩溃 | 重装对应版本 Hosting Bundle → `iisreset`；看事件日志 + 临时开 `stdoutLogEnabled=true` 看 `logs\stdout-*.log` |
| **500.30 In-Process Startup Failure** | 启动异常：连接串错误、配置问题、缺文件 | 查看「Windows 事件查看器 → 应用程序」最新错误；核对 appsettings.json |
| **500.31 / 500.33** | .NET 运行时版本缺失/不匹配 | 确认装了 .NET 10 的 Hosting Bundle |
| **503 Service Unavailable** | 应用池已停止/崩溃 | 检查应用池状态；看事件日志 |
| **401 无权限/认证失败** | JwtSettings 配置不一致；token 无效 | 前后端 JWT SecretKey/Issuer/Audience 一致；重新登录 |
| **404** | 路由不存在；web.config 被删/损坏 | 确认走 `/api/` 前缀；web.config 存在且内容完整 |
| **500 服务器错误** | 代码异常（数据库查询、PDF 生成等） | 看 `logs\rbs-*.log` 的 Serilog 日志 |
| **连不上数据库** | 连接串错误、SQL 1433 未放行、SQL 账号无权 | 用 SSMS 用同账号测连；防火墙放行 1433 |
| **日志目录写不进去** | 应用池身份无写权限 | 执行 5.4 的 icacls 授权 |
| **CORS 报错** | 前端域名不在白名单 | 见 6.3 加域名；或同源部署 |

**日志位置汇总**：
- 应用日志（Serilog）：`D:\sites\rbs-api\logs\rbs-YYYYMMDD.log`
- 启动错误（stdout）：临时 `stdoutLogEnabled=true` 后写 `logs\stdout-*.log`
- Windows 事件日志：`事件查看器 → Windows 日志 → 应用程序`

---

## 9. 更新发布流程

每次发新版后端：

```bash
# 1. 重新发布
dotnet publish src/RBS.Api -c Release -o publish/api
```

```powershell
# 2. 服务器上：先停止应用池（释放被占用的 exe/dll）
Stop-WebAppPool -Name "rbs-api-pool"

# 3. 覆盖发布目录（把新 publish/api 内容拷贝到 D:\sites\rbs-api）
#    注意：不要覆盖你改过的 appsettings.json！
#    建议单独备份 appsettings.json，或只拷贝 exe/dll/其他文件

# 4. 启动应用池
Start-WebAppPool -Name "rbs-api-pool"

# 5. 验证
curl http://localhost:8080/api/health
```

> 优雅停机替代方案：在发布目录放一个空的 `app_offline.htm` 文件 → IIS 自动卸载应用 → 拷贝文件 → 删除 `app_offline.htm` → 自动重启。

---

## 附：常用命令速查

```bash
# 发布（开发机）
dotnet publish src/RBS.Api -c Release -o publish/api

# 服务器查看运行时
dotnet --list-runtimes

# 重启 IIS
iisreset

# 应用池启停（PowerShell）
Stop-WebAppPool -Name "rbs-api-pool"
Start-WebAppPool -Name "rbs-api-pool"

# 本地验证
curl http://localhost:8080/api/health
```

*本文档针对 .NET 10 / ASP.NET Core Module v2 / IIS 10 编写。*
