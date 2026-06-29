using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RBS.Api.Services;
using RBS.Application;
using RBS.Application.Services.Organization;
using RBS.Core.Interfaces.Services;
using RBS.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ===== 服务注册 =====

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// OpenAPI / Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Authentication
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

// CORS — allow Vue dev server
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

// HttpContext
builder.Services.AddHttpContextAccessor();

// Current User Service
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Infrastructure layer (EF Core, Repositories, UnitOfWork)
builder.Services.AddInfrastructureData(builder.Configuration);

// Application layer
builder.Services.AddApplicationLayer();

// API 日志 — 共享通道（Singleton）+ 后台批量写入
builder.Services.AddSingleton<RBS.Api.Middleware.ApiLogChannel>();
builder.Services.AddHostedService<RBS.Api.Services.ApiLogWriterService>();

var app = builder.Build();

// ===== 中间件管道 =====

// 全局异常捕获（需放在最前面）
app.UseMiddleware<RBS.Api.Middleware.ExceptionLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowWebApp");

app.UseAuthentication();
app.UseAuthorization();

// API 调用日志（需在 Auth 之后，才能捕获用户信息）
app.UseMiddleware<RBS.Api.Middleware.ApiLoggingMiddleware>();

app.MapControllers();

app.Run();
