using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RBS.Core.Interfaces.Services;

namespace RBS.Infrastructure.Data;

/// <summary>
/// 设计时 DbContext 工厂 — 供 dotnet ef 工具在编译时创建 AppDbContext
/// 避免 "Unable to resolve service for type 'DbContextOptions'" 错误
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // 从环境变量读取连接字符串，若不存在则使用开发环境默认值
        var connectionString = Environment.GetEnvironmentVariable("RBS_CONNECTION_STRING")
            ?? "Server=.;Database=RBS;User Id=sa;Password=123456;TrustServerCertificate=true;MultipleActiveResultSets=true;Max Pool Size=100;Min Pool Size=10;Connection Timeout=30;";

        optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
            sqlOptions.EnableRetryOnFailure(3);
            sqlOptions.CommandTimeout(60);
        });

        return new AppDbContext(
            optionsBuilder.Options,
            new DesignTimeCurrentUserService(),
            new DesignTimeTenantService());
    }

    /// <summary>设计时使用的空当前用户服务</summary>
    private class DesignTimeCurrentUserService : ICurrentUserService
    {
        public Guid UserId => Guid.Empty;
        public string? Username => "DesignTime";
        public bool IsSuperAdmin => true;
        public Guid? HomeCompanyId => null;
        public List<Guid> RoleIds => new();
        public List<string> Permissions => new();
    }

    /// <summary>设计时使用的空多租户服务</summary>
    private class DesignTimeTenantService : ITenantService
    {
        public Guid? HomeCompanyId => null;
        public bool IsSuperAdmin => true;
        public Guid? EffectiveCompanyId => null;
        public Guid DefaultCompanyId => Guid.Empty;
        public bool IsViewingAll => true;
    }
}
