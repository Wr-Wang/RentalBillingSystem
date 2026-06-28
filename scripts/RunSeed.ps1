# 菜单种子数据导入脚本
# 用法: .\scripts\RunSeed.ps1

$connectionString = "Server=.;Database=RBS;User Id=sa;Password=123456;TrustServerCertificate=true;"

# 读取 SQL 文件
$seedSql = Get-Content -Path "$PSScriptRoot\SeedMenus.sql" -Raw -Encoding UTF8
$roleSql = Get-Content -Path "$PSScriptRoot\SeedAdminRolePermissions.sql" -Raw -Encoding UTF8

# 使用 System.Data.SqlClient 连接数据库执行
try {
    Add-Type -AssemblyName System.Data

    $conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $conn.Open()
    Write-Host "Connected to database RBS" -ForegroundColor Green

    # 执行种子数据
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $seedSql
    $cmd.CommandTimeout = 120
    $result = $cmd.ExecuteNonQuery()
    Write-Host "SeedMenus.sql executed successfully" -ForegroundColor Green

    # 执行角色权限分配
    $cmd2 = $conn.CreateCommand()
    $cmd2.CommandText = $roleSql
    $cmd2.CommandTimeout = 120
    $result2 = $cmd2.ExecuteNonQuery()
    Write-Host "SeedAdminRolePermissions.sql executed successfully" -ForegroundColor Green

    $conn.Close()
    Write-Host "`nAll seed data imported successfully!" -ForegroundColor Green
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
