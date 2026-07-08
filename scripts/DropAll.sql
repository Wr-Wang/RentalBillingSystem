-- ===================================================================
-- DropAll.sql - 删除数据库中所有表
-- 说明：本系统无外键约束，删除顺序无关
-- ===================================================================
EXEC sp_MSforeachtable 'DROP TABLE ?'

SELECT NAME AS [剩余表] FROM sys.tables WHERE type = 'U'
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE type = 'U')
    PRINT N'所有表已删除'
GO
