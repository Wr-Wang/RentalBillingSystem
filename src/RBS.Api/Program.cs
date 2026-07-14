using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RBS.Api.Services;
using RBS.Application;
using RBS.Application.Services.Organization;
using RBS.Core.Interfaces.Services;
using RBS.Infrastructure.Data;

// ============================================================
// RBS API 入口 — 启动配置与中间件管道
// ============================================================
// 说明：
// 1. 服务注册阶段注册所有依赖（Controllers, Swagger, JWT, CORS,
//    Infrastructure, Application, API 日志）
// 2. 中间件管道按顺序配置：异常捕获 → Swagger(Dev) → CORS →
//    认证 → 授权 → API 日志 → 路由
// 3. API 日志使用 Channel 机制 + 后台批量写入，避免阻塞请求
// ============================================================

var builder = WebApplication.CreateBuilder(args);

// ===== 服务注册 =====

// Controllers — JSON 序列化配置：CamelCase、忽略 null、字典键 CamelCase
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ===== OpenAPI / Swagger =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===== JWT 身份认证 =====
// 配置 JwtBearer 认证方案，验证 Issuer/Audience/Lifetime/SigningKey
// ClockSkew=TimeSpan.Zero 消除默认 5 分钟时钟偏差
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyForJwtTokenGenerationAtLeast32Chars!";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// ===== CORS — 允许 Vue 开发服务器跨域访问 =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ===== HttpContext 访问器 =====
// 用于 TenantService 和 CurrentUserService 读取当前请求的 JWT Claims
builder.Services.AddHttpContextAccessor();

// ===== 当前用户服务 =====
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// ===== 基础设施层（仓储、工作单元、多租户、审计、PDF、调度） =====
builder.Services.AddInfrastructureData(builder.Configuration);

// ===== 应用层（DTO、映射、领域事件处理） =====
builder.Services.AddApplicationLayer();

// ===== API 日志通道 =====
// ApiLogChannel（Singleton）作为日志缓冲区
// ApiLogWriterService（后台服务）批量将日志写入数据库
builder.Services.AddSingleton<RBS.Api.Middleware.ApiLogChannel>();
builder.Services.AddHostedService<RBS.Api.Services.ApiLogWriterService>();

var app = builder.Build();

// ===== 中间件管道（顺序敏感） =====

// 1. 全局异常捕获（需放在最前面，捕获所有下游异常）
app.UseMiddleware<RBS.Api.Middleware.ExceptionLoggingMiddleware>();

// 2. Swagger（仅开发环境）
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 3. CORS（在认证之前）
app.UseCors("AllowWebApp");

// 4. 认证 + 授权
app.UseAuthentication();
app.UseAuthorization();

// 5. API 调用日志（需在 Auth 之后，才能捕获用户信息）
app.UseMiddleware<RBS.Api.Middleware.ApiLoggingMiddleware>();

// 6. 路由
app.MapControllers();

app.Run();
