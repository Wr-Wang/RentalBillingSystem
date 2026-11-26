# 数据库初始化

本目录提供 **一键建库 + 建表 + 种子数据** 脚本，执行完成后即可用种子账号登录系统。

## 文件说明

| 文件 | 作用 |
|------|------|
| `00_create_database.sql` | 创建数据库（已存在则跳过，不删数据） |
| `init-db.ps1` | Windows / PowerShell 一键初始化入口 |
| `init-db.sh` | Linux / macOS / Git Bash 一键初始化入口 |

建表和种子数据复用仓库已有的 `scripts/` 脚本，不重复维护：

| 顺序 | 脚本 | 内容 |
|------|------|------|
| 1 | `scripts/Init.sql` | 60+ 张业务表、索引、字段说明 |
| 2 | `scripts/SeedBase.sql` | 公司、角色、登录用户、菜单、字典、配置 |
| 3 | `scripts/SeedBiz.sql` | 房源、房型、合同、调度任务等演示数据 |

> **只对空库执行。** `SeedBase.sql` 会重建菜单表，`SeedBiz.sql` 会清空并重插 `HousingUnits`
> 演示房源。为避免误删生产数据，库中一旦已有表，脚本会跳过全部初始化步骤。
> 需要彻底重建请使用 `-Recreate` / `--recreate`（会删除整个库）。

## 前置条件

- SQL Server 2019+（推荐 2022）
- `sqlcmd` 命令行工具
  - Windows：随 SQL Server / ODBC 客户端安装，或 `winget install Microsoft.Sqlcmd`
  - Linux：安装 `mssql-tools18`
- 连接账号具备建库、建表、写入数据权限（`sa` 或 `sysadmin` 成员）

## 一键初始化

### Windows PowerShell

```powershell
# 在仓库根目录执行；不传 -Password 会交互式输入
.\database\init-db.ps1 -Server "." -Username sa -Password "你的密码"
```

### Linux / macOS / Windows Git Bash

```bash
# 在仓库根目录执行（用 bash 调用，避免文件未带可执行位）
bash database/init-db.sh -S . -U sa -P "你的密码"
```

### 常用参数

| PowerShell | Bash | 默认值 | 说明 |
|------------|------|--------|------|
| `-Server` | `-S` / `--server` | `.` | SQL Server 地址，如 `.\SQLEXPRESS`、`localhost,1433` |
| `-Database` | `-d` / `--database` | `RBS` | 数据库名 |
| `-Username` | `-U` / `--username` | `sa` | 登录名 |
| `-Password` | `-P` / `--password` | 交互输入 | 也可用环境变量 `SQLCMDPASSWORD` |
| `-SqlCmd` | `--sqlcmd` | 自动查找 | sqlcmd 完整路径 |
| `-SkipSeedBiz` | `--skip-seed-biz` | 否 | 新建库时只建库、建表、灌基础数据 |
| `-Recreate` | `--recreate` | 否 | **删除同名库后重建**（需确认） |
| `-Force` | `--force` | 否 | 配合 `-Recreate` 跳过确认 |

示例：

```powershell
# 只建基础数据，不灌业务演示数据
.\database\init-db.ps1 -Password "123456" -SkipSeedBiz

# 彻底重建（会清空 RBS 库所有数据）
.\database\init-db.ps1 -Password "123456" -Recreate -Force
```

```bash
bash database/init-db.sh -S "localhost,1433" -P "123456" --recreate --force
```

## 默认登录账号

种子用户的密码统一为 **`123456`**（`SeedBase.sql` 中的 BCrypt 哈希已按此密码生成）。

| 用户名 | 密码 | 角色 | 所属公司 |
|--------|------|------|----------|
| `admin` | `123456` | 系统管理员（超级管理员） | — |
| `zhangsan` | `123456` | 运营主管 | GS001 上海茂源置业有限公司 |
| `lisi` | `123456` | 运营人员 | GS001 |
| `wangwu` | `123456` | 财务主管 | GS001 |
| `zhaoliu` | `123456` | 会计 | GS001 |
| `company_a` | `123456` | 公司账号（只读） | GS001 |

> 生产环境请立即修改所有默认密码。

## 验证初始化结果

```bash
sqlcmd -S . -U sa -P "你的密码" -C -d RBS -Q "SELECT COUNT(*) AS Tables FROM sys.tables WHERE type='U'"
sqlcmd -S . -U sa -P "你的密码" -C -d RBS -Q "SELECT Username, DisplayName, IsActive FROM Users"
```

或用 API 验证登录：

```bash
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"123456"}'
```

返回 `token` 即表示数据库与登录链路正常。

## 重置登录密码

系统使用 BCrypt 存储密码（`Users.PasswordHash`）。下面的哈希对应明文 `123456`：

```sql
UPDATE [Users]
SET [PasswordHash] = '$2b$12$ZnELF2QgBXIilzbMir2uh.RWwIZYXExVvq4camCr.tNfzPH7SShk.',
    [UpdatedAt]    = GETDATE()
WHERE [Username] = 'admin';
```

重新执行 `SeedBase.sql` 不会覆盖已存在用户的密码（脚本只在用户不存在时插入）。

## 常见问题

| 现象 | 原因与处理 |
|------|------------|
| `未找到 sqlcmd` | 安装 SQL Server Command Line Utilities，或用 `-SqlCmd` / `--sqlcmd` 指定路径 |
| `The server certificate was not trusted` | 脚本已带 `-C`；若仍失败，确认服务端证书或改用 `TrustServerCertificate=true` 的连接方式 |
| 中文显示乱码 | 脚本已使用 `-f 65001` 按 UTF-8 读取；Windows 控制台可先执行 `chcp 65001` |
| `已存在名为 'MS_Description' 的属性` | `Init.sql` 不是幂等脚本；脚本会对已有表的库整体跳过，重建请用 `-Recreate` |
| 登录提示用户名或密码错误 | 确认 `Users` 表有记录；`admin` 的密码应为 `123456`，可用上方 SQL 重置 |
| 重跑初始化 | 已有表时自动跳过全部步骤，不会改数据；需重新灌演示数据请用 `-Recreate` |
| `QUOTED_IDENTIFIER` 相关建索引失败 | 手工执行时漏了 `sqlcmd -I`；或改用 `init-db.ps1` / `init-db.sh` |

## 修改连接字符串

初始化完成后，确认后端配置指向同一个库：

```json
// src/RBS.Api/appsettings.json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=RBS;User Id=sa;Password=你的密码;TrustServerCertificate=true;..."
}
```
