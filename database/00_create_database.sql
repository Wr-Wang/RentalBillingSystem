-- ===================================================================
-- 00_create_database.sql — 创建 RBS 数据库（幂等）
--
-- 用法（sqlcmd）：
--   sqlcmd -S . -U sa -P "你的密码" -C -d master ^
--          -v DatabaseName=RBS -i database/00_create_database.sql
--
-- 说明：
--   本脚本只负责“建库”。建表与种子数据由 init-db.ps1 / init-db.sh
--   依次执行 scripts/Init.sql、SeedBase.sql、SeedBiz.sql 完成。
--   数据库已存在时不会删除或重建，直接跳过。
-- ===================================================================
SET NOCOUNT ON;
GO

IF DB_ID(N'$(DatabaseName)') IS NULL
BEGIN
    PRINT N'创建数据库 [$(DatabaseName)] ...';
    CREATE DATABASE [$(DatabaseName)];
    PRINT N'数据库 [$(DatabaseName)] 创建完成。';
END
ELSE
BEGIN
    PRINT N'数据库 [$(DatabaseName)] 已存在，跳过创建。';
END
GO
