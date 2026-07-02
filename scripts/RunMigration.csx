#!/usr/bin/env dotnet-script
// 执行续签功能数据库迁移脚本
#r "System.Data.SqlClient"

using System.Data.SqlClient;
using System.IO;

var connStr = "Server=.;Database=RBS;User Id=sa;Password=123456;TrustServerCertificate=true;MultipleActiveResultSets=true;Max Pool Size=100;Min Pool Size=10;Connection Timeout=30;";
var sql = File.ReadAllText(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location) ?? ".", "..", "..", "..", "scripts", "RenewalSchema.sql"));

using var conn = new SqlConnection(connStr);
conn.Open();
foreach (var batch in sql.Split("GO", StringSplitOptions.RemoveEmptyEntries))
{
    if (string.IsNullOrWhiteSpace(batch)) continue;
    using var cmd = new SqlCommand(batch, conn);
    cmd.CommandTimeout = 120;
    await cmd.ExecuteNonQueryAsync();
    Console.WriteLine($"Batch executed: {batch[..Math.Min(80, batch.TrimEnd().Length)]}...");
}
Console.WriteLine("Migration completed successfully!");
