using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Infrastructure.Shared.Consul;
using RiverLi.Blog.Infrastructure.Shared.Extensions;
using RiverLi.Blog.Infrastructure.Shared.OpenApi; // 引入共享库扩展
using RiverLi.Blog.Infrastructure.Shared.Repositories;
using RiverLi.Blog.Infrastructure.Shared.Security;
using riverli.blog.services.identity.Application.Interfaces;
using riverli.blog.services.identity.Domain.Entities;
using riverli.blog.services.identity.Infrastructure.Auth;
using riverli.blog.services.identity.Infrastructure.Data;
using riverli.blog.services.identity.Api.Filters;
using riverli.blog.services.identity.Infrastructure.Repositories;
using riverli.blog.services.identity.Infrastructure.Security;
using RiverLi.DDD.Core.Domain.Repositories;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 0. 从 Consul 配置中心拉取远程配置（覆盖本地 appsettings）
// ==========================================
builder.Configuration.AddConsulConfiguration(builder.Configuration.GetSection("Consul"));

// ==========================================
// 1. 【共享基建注入】(通过 Shared 库一键接管)
// ==========================================

// 注册控制器和全局无感拦截器
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalApiAuthorizeFilter>();
});

// 🌟 核心：Identity 自己也挂载自上报引擎
builder.Services.AddApiSelfReporting();

// 1.1 注册日志服务 (区分微服务名称)
builder.Services.AddLoggingSupport(builder.Configuration, "IdentityService");

// 1.2 一键注册核心基建 (包含 异常处理、CORS、OpenAPI、JSON、ICurrentUser 等)
builder.Services.AddInfrastructureSharedServices(options => 
{
    options.EnableGlobalExceptionHandler = true;
    options.EnableCors = true;
    // 将原本在 AddCors 里的域名配置移到这里
    options.AllowedOrigins = new[] { "http://localhost:5000", "http://localhost:3000", "http://localhost:5173" };
    
    options.EnableOpenApiDocumentation = true;
    options.ScalarTitle = "RiverLi Blog Identity API";
    options.ScalarVersion = "v1";
    options.ScalarDescription = "身份认证微服务接口文档";
});

// 1.3 注册健康检查
builder.Services.AddHealthCheckSupport(builder.Configuration);


// ==========================================
// 2. 【业务专属注入】(Identity 微服务特有逻辑)
// ==========================================

// 2.1 数据库配置
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("未获取到连接字符串");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
var isOder = builder.Configuration["isOder"];
Console.WriteLine($"isOder: {isOder}");
// 2.2 注册 ASP.NET Core Identity
builder.Services.AddIdentity<AppUser, AppRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.User.RequireUniqueEmail = false; // 设为 false，避免 AddToRoles 等非邮箱操作连带触发唯一性校验
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// 2.3 配置 JWT 认证
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "Bearer";
        options.DefaultChallengeScheme = "Bearer";
    })
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };
    });

// 2.4 注册业务依赖
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(riverli.blog.services.identity.Application.Features.Menus.Queries.GetUserMenusQuery).Assembly));

// 将 RiverDbContext (EfRepository 依赖的基类) 转发到 AppDbContext
builder.Services.AddScoped<RiverLi.Blog.Infrastructure.Shared.Data.RiverDbContext>(
    sp => sp.GetRequiredService<AppDbContext>());
// 1. 注入具体实现
builder.Services.AddScoped<IApiPermissionChecker, ApiPermissionChecker>();

// 2. 在添加控制器时，将过滤器打入全局配置
builder.Services.AddControllers(options =>
{
    // 👇 这句话加上，全系统所有 API 立刻进入最高警戒状态！
    options.Filters.Add<GlobalApiAuthorizeFilter>();
});

builder.Services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddMemoryCache();

// 注册全局 API 授权过滤器（替代手动 [RequirePermission]）
builder.Services.AddScoped<GlobalApiAuthorizationFilter>();
builder.Services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
{
    options.Filters.AddService<GlobalApiAuthorizationFilter>();
});
builder.Services.AddMicroserviceOpenApi();


// ==========================================
// 3. 【中间件管道配置】
// ==========================================
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // .NET 9 原生路由挂载组件
    app.MapOpenApi(); 
    
    // 挂载高颜值的 Scalar UI 皮肤
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("RiverLi Blog - 身份认证微服务")
            .WithTheme(ScalarTheme.DeepSpace); // 科技感十足的深空主题
    });
}

// 3.1 一键启用共享中间件 (包含 CORS、异常拦截、请求头转发 等)
app.UseInfrastructureSharedMiddlewares();

// 3.2 播种初始数据
await DbSeeder.SeedAsync(app.Services);

// 3.3 路由与鉴权 (顺序极其重要)
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 3.4 挂载共享终结点 (健康检查、Scalar 文档等)
app.MapInfrastructureSharedEndpoints(options => {
    options.EnableOpenApiDocumentation = app.Environment.IsDevelopment();
    options.ScalarTitle = "RiverLi Blog Identity API";
});

app.Run();