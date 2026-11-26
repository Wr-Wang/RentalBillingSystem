<#
.SYNOPSIS
    RBS 数据库一键初始化：建库 → 建表 → 基础种子数据 → 业务种子数据。

.DESCRIPTION
    按顺序执行：
      1. database/00_create_database.sql   建库（已存在则跳过）
      2. scripts/Init.sql                  建表 + 扩展属性（库中已有表时跳过）
      3. scripts/SeedBase.sql              公司、角色、用户、菜单、字典等基础数据
      4. scripts/SeedBiz.sql               房源、合同、调度任务等业务演示数据

    只对“空库”执行建表和种子数据。种子脚本包含重置菜单（DELETE RoleMenus/Menus）
    和演示房源（DELETE HousingUnits）的逻辑，对已有数据的库重复执行会造成数据丢失，
    因此库中一旦有表，脚本默认跳过全部初始化步骤。需要彻底重建请加 -Recreate。

.PARAMETER Server
    SQL Server 地址。默认 "."（本机默认实例）；命名实例/端口写法：".\SQLEXPRESS"、"localhost,1433"。

.PARAMETER Database
    数据库名，默认 RBS。

.PARAMETER Username
    SQL Server 登录名，默认 sa。

.PARAMETER Password
    登录密码。不传时依次尝试环境变量 SQLCMDPASSWORD、交互式输入。
    推荐用交互式输入或环境变量，避免密码出现在命令历史/进程列表中。

.PARAMETER SqlCmd
    sqlcmd.exe 完整路径。不传时自动从 PATH 及常见安装目录查找。

.PARAMETER SkipSeedBiz
    新建库时跳过 SeedBiz.sql（只建库、建表、灌基础数据）。

.PARAMETER Recreate
    先删除同名数据库再重建。会丢失该库全部数据，默认需要输入 yes 确认。

.PARAMETER Force
    配合 -Recreate 使用，跳过确认提示（自动化场景）。

.EXAMPLE
    .\init-db.ps1 -Server "." -Password "123456"

.EXAMPLE
    .\init-db.ps1 -Server "localhost,1433" -Database RBS -Username sa -Password "123456" -SkipSeedBiz

.EXAMPLE
    .\init-db.ps1 -Recreate -Force -Password "123456"
#>
[CmdletBinding()]
param(
    [string]$Server = ".",
    [string]$Database = "RBS",
    [string]$Username = "sa",
    [string]$Password = "",
    [string]$SqlCmd = "",
    [switch]$SkipSeedBiz,
    [switch]$Recreate,
    [switch]$Force
)

$ErrorActionPreference = "Stop"

# 确保中文输出在 Windows PowerShell / Git Bash 下不乱码
try { [Console]::OutputEncoding = [System.Text.Encoding]::UTF8 } catch { }

# ---------------------------------------------------------------------------
# 路径与参数校验
# ---------------------------------------------------------------------------
$repoRoot     = Split-Path -Parent $PSScriptRoot
$scriptsDir   = Join-Path $repoRoot "scripts"
$createScript = Join-Path $PSScriptRoot "00_create_database.sql"
$initScript   = Join-Path $scriptsDir "Init.sql"
$seedBase     = Join-Path $scriptsDir "SeedBase.sql"
$seedBiz      = Join-Path $scriptsDir "SeedBiz.sql"

foreach ($file in @($createScript, $initScript, $seedBase, $seedBiz)) {
    if (-not (Test-Path $file)) { throw "找不到脚本：$file" }
}

if ($Database -notmatch '^[A-Za-z_][A-Za-z0-9_]*$') {
    throw "数据库名不合法：$Database（只能包含字母、数字、下划线，且不能以数字开头）"
}

# ---------------------------------------------------------------------------
# 定位 sqlcmd
# ---------------------------------------------------------------------------
if (-not $SqlCmd) {
    $found = Get-Command sqlcmd -ErrorAction SilentlyContinue
    if ($found) { $SqlCmd = $found.Source }
}
if (-not $SqlCmd -or -not (Test-Path $SqlCmd)) {
    $candidates = @(
        (Join-Path $env:ProgramFiles "Microsoft SQL Server\Client SDK\ODBC\180\Tools\Binn\sqlcmd.exe"),
        (Join-Path $env:ProgramFiles "Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\sqlcmd.exe"),
        (Join-Path ${env:ProgramFiles(x86)} "Microsoft SQL Server\Client SDK\ODBC\180\Tools\Binn\sqlcmd.exe"),
        (Join-Path ${env:ProgramFiles(x86)} "Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\sqlcmd.exe")
    ) | Where-Object { $_ -and (Test-Path $_) }
    $SqlCmd = $candidates | Select-Object -First 1
}
if (-not $SqlCmd) {
    throw "未找到 sqlcmd。请安装 SQL Server Command Line Utilities（winget install Microsoft.Sqlcmd），或用 -SqlCmd 指定完整路径。"
}

# ---------------------------------------------------------------------------
# 密码：参数 → 环境变量 → 交互输入
# ---------------------------------------------------------------------------
if (-not $Password) {
    if ($env:SQLCMDPASSWORD) {
        $Password = $env:SQLCMDPASSWORD
    } else {
        $secure = Read-Host "请输入 SQL Server 用户 [$Username] 的密码" -AsSecureString
        $Password = [System.Net.NetworkCredential]::new("", $secure).Password
    }
}
if (-not $Password) { throw "密码不能为空。" }
$env:SQLCMDPASSWORD = $Password

# ---------------------------------------------------------------------------
# 辅助函数
# ---------------------------------------------------------------------------
function Invoke-SqlFile {
    param(
        [Parameter(Mandatory = $true)][string]$File,
        [Parameter(Mandatory = $true)][string]$TargetDb,
        [string[]]$Variables = @()
    )

    $leaf = Split-Path -Leaf $File
    Write-Host "`n>>> $leaf  →  数据库 [$TargetDb]" -ForegroundColor Cyan

    $sqlArgs = @(
        "-S", $Server,
        "-U", $Username,
        "-d", $TargetDb,
        "-b", "-r", "1",     # 出错即中止，错误输出到 stderr
        "-f", "65001",       # 按 UTF-8 读取含中文的脚本
        "-C",                # 信任服务器证书（ODBC 18 客户端默认强制加密）
        "-I",                # 开启 QUOTED_IDENTIFIER（筛选索引/计算列索引必需）
        "-i", $File
    )
    if ($Variables.Count -gt 0) {
        $sqlArgs = @("-v") + $Variables + $sqlArgs
    }

    & $SqlCmd @sqlArgs
    if ($LASTEXITCODE -ne 0) {
        throw "执行 $leaf 失败（sqlcmd 退出码：$LASTEXITCODE）。"
    }
}

function Get-UserTableCount {
    param([Parameter(Mandatory = $true)][string]$TargetDb)

    $output = & $SqlCmd -S $Server -U $Username -d $TargetDb -h -1 -W -C `
        -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.tables WHERE type = 'U';"
    if ($LASTEXITCODE -ne 0) { throw "查询 [$TargetDb] 表数量失败。" }

    $line = $output | ForEach-Object { "$_".Trim() } | Where-Object { $_ -match '^\d+$' } | Select-Object -First 1
    if (-not $line) { return 0 }
    return [int]$line
}

# ---------------------------------------------------------------------------
# 主流程
# ---------------------------------------------------------------------------
Write-Host "============================================" -ForegroundColor Green
Write-Host " RBS 数据库初始化" -ForegroundColor Green
Write-Host " 服务器 : $Server"
Write-Host " 数据库 : $Database"
Write-Host " 登录名 : $Username"
Write-Host "============================================" -ForegroundColor Green

if ($Recreate) {
    if (-not $Force) {
        $answer = Read-Host "警告：将删除数据库 [$Database] 及其全部数据。输入 yes 继续"
        if ($answer -ne "yes") { Write-Host "已取消。"; return }
    }
    Write-Host "`n>>> 删除数据库 [$Database]" -ForegroundColor Yellow
    $dropSql = @"
IF DB_ID(N'$Database') IS NOT NULL
BEGIN
    ALTER DATABASE [$Database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [$Database];
END
"@
    & $SqlCmd -S $Server -U $Username -d master -b -r 1 -C -Q $dropSql
    if ($LASTEXITCODE -ne 0) { throw "删除数据库 [$Database] 失败。" }
}

# 1. 建库
Invoke-SqlFile -File $createScript -TargetDb "master" -Variables @("DatabaseName=$Database")

# 2. 建表 + 种子数据（仅当库为空时执行）
$tableCount = Get-UserTableCount -TargetDb $Database
if ($tableCount -gt 0) {
    Write-Host "`n>>> [$Database] 已有 $tableCount 张表，判定为已初始化。" -ForegroundColor Yellow
    Write-Host "    跳过 Init.sql / SeedBase.sql / SeedBiz.sql（种子脚本会重置菜单和房源数据）。" -ForegroundColor Yellow
    Write-Host "    如需彻底重建，请加 -Recreate 参数。" -ForegroundColor Yellow
} else {
    Invoke-SqlFile -File $initScript -TargetDb $Database
    Invoke-SqlFile -File $seedBase -TargetDb $Database

    if ($SkipSeedBiz) {
        Write-Host "`n>>> 已指定 -SkipSeedBiz，跳过 SeedBiz.sql。" -ForegroundColor Yellow
    } else {
        Invoke-SqlFile -File $seedBiz -TargetDb $Database
    }
}

# ---------------------------------------------------------------------------
# 完成
# ---------------------------------------------------------------------------
$finalCount = Get-UserTableCount -TargetDb $Database
Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host " 初始化完成：[$Database] 共有 $finalCount 张表" -ForegroundColor Green
Write-Host " 默认登录：admin / 123456（超级管理员）" -ForegroundColor Green
Write-Host " 其余种子账号见 database/README.md" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
