using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MediatR;
using RiverLi.Blog.Identity.Application.Users.Commands.Register;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Auth;
using RiverLi.Blog.Identity.Infrastructure.Data;
using System.Reflection;
using Microsoft.OpenApi.Models;
using RiverLi.Blog.Identity.Application.Common.Interfaces;
using RiverLi.Blog.Identity.Application.Common.Services;
using RiverLi.Blog.Identity.Application.Users.Commands.User;
using RiverLi.Blog.Identity.Application.Common.Behaviors;
//using RiverLi.Blog.Infrastructure.Shared.Auth;
using RiverLi.DDD.Core.Application.Common.Interfaces;
using Scalar.AspNetCore;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace RiverLi.Blog.Identity.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // 配置 Serilog - 在 WebApplicationBuilder 创建之后
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(builder.Configuration)
                    .Enrich.FromLogContext()
                    .CreateLogger();

                Log.Information("=== 应用程序启动 ===");
                Log.Information("正在启动 RiverLi Blog Identity API...");

                // 将 Serilog 集成到 ASP.NET Core 日志系统
                builder.Host.UseSerilog();

                // Add services to the container.
                builder.Services.AddOpenApi(options =>
                {
                
                    options.AddDocumentTransformer((document, context, cancellationToken) =>
                    {
                        // 显式定义多个 Server 入口
                        document.Servers = new List<OpenApiServer>
                        {
                            // 这样在 5000 端口查看文档时，默认请求地址就是网关自己
                            //new() { Url = "/api/auth" },
                        };
                        return Task.CompletedTask;
                    });
                });
                builder.Services.AddControllers()
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                        options.JsonSerializerOptions.WriteIndented = true;
                        options.JsonSerializerOptions.MaxDepth = 64;
                    });

                builder.Services.AddEndpointsApiExplorer();
                
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("GatewayPolicy", policy =>
                    {
                        policy.WithOrigins("http://localhost:5000", "http://localhost:3000") // 允许网关和前端开发服务器的请求
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials(); // 允许携带凭证
                    });
                    
                    // 开发环境下添加更宽松的策略
                    options.AddPolicy("DevelopmentPolicy", policy =>
                    {
                        policy.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
                });
                
                // 1. 数据库配置 (MySQL)
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                Console.WriteLine($"Connection String: {connectionString}");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("未获取到DefaultConnection连接字符串，请检查appsettings.json配置");
                }
                builder.Services.AddDbContext<IdentityServiceDbContext>(options =>
                    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

                // 2. 注册 ASP.NET Core Identity 服务
                builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
                {
                    // 密码强度设置
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 6;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;

                    // 用户设置
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<IdentityServiceDbContext>()
                .AddDefaultTokenProviders();


                // 3. 注册业务服务
                builder.Services.AddScoped<JwtTokenService>();
                builder.Services.AddScoped<IPermissionCalculator, PermissionCalculator>();
                builder.Services.AddScoped<ILoginHistoryService, LoginHistoryService>();
                builder.Services.AddHttpContextAccessor();
                // TODO: 实现 CurrentUser 类或从其他包引入
                // builder.Services.AddScoped<RiverLi.DDD.Core.Application.Common.Interfaces.ICurrentUser, CurrentUser>();
                
                // 3.1 配置JWT认证
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
                            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? "YourSuperSecretKey12345"))
                    };
                });
                
                // 3.2 注册 MediatR - 添加详细诊断
                builder.Services.AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssembly(typeof(RiverLi.Blog.Identity.Application.Users.Commands.Login.LoginHandler).Assembly);
                    // 注册全局异常处理管道
                    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RiverLi.Blog.Identity.Application.Common.Behaviors.ExceptionHandlingBehavior<,>));
                    Console.WriteLine("MediatR 注册完成");
                });
                var app = builder.Build();

                // 根据环境使用不同的CORS策略
                if (app.Environment.IsDevelopment())
                {
                    app.UseCors("DevelopmentPolicy");
                }
                else
                {
                    app.UseCors("GatewayPolicy");
                }
                
                // 5. 中间件配置
                if (app.Environment.IsDevelopment())
                {
                    app.MapOpenApi(); // 生成 /openapi/v1.json
                    app.MapScalarApiReference(options =>
                    {
                        options
                            .WithTitle("RiverLi Blog Identity API")
                            .WithTheme(ScalarTheme.Moon)
                            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
                    });
                }

                // 自动迁移数据库 (可选,生产环境请谨慎开启)
                using (var scope = app.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<IdentityServiceDbContext>();
                    try
                    {
                        // 检查是否有待应用的迁移
                        var pendingMigrations = db.Database.GetPendingMigrations();
                        if (pendingMigrations.Any())
                        {
                            Log.Information("应用数据库迁移...");
                            db.Database.Migrate();
                            Log.Information("数据库迁移完成");
                        }
                        else
                        {
                            Log.Information("数据库已是最新版本");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "数据库迁移失败，但应用程序将继续运行");
                    }
                }

                app.UseHttpsRedirection();

                app.UseAuthentication(); // 必须在 Authorization 之前
                app.UseAuthorization();


                app.MapControllers();

                // 按顺序执行种子数据初始化
                await SeedData.EnsureSeedData(app.Services);  // 1. 创建管理员账号和角色
                await FrontendRouteSeeder.EnsureFrontendRouteSeedData(app.Services);  // 2. 创建前端路由和路由分组
                await BackendRouteSeeder.EnsureBackendRouteSeedData(app.Services);  // 3. 创建后端路由和路由分组
                await PermissionSeedData.EnsurePermissionSeedData(app.Services);  // 4. 创建权限数据
                await MenuDataSeeder.EnsureMenuData(app.Services);  // 5. 创建菜单组和菜单数据
                await MenuSeedData.EnsureMenuSeedData(app.Services);  // 6. 更新菜单权限
                await AdminMenuInitializer.InitializeAdminMenus(app.Services);  // 7. 初始化管理员菜单

                Log.Information("应用程序启动成功!");
                await app.RunAsync();
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine("=== ReflectionTypeLoadException 详细诊断 ===");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"\nLoader Exceptions ({ex.LoaderExceptions?.Length ?? 0}个):");
                if (ex.LoaderExceptions != null)
                {
                    for (int i = 0; i < ex.LoaderExceptions.Length; i++)
                    {
                        var loaderEx = ex.LoaderExceptions[i];
                        Console.WriteLine($"  [{i}] {loaderEx?.GetType().Name}: {loaderEx?.Message}");
                        if (loaderEx?.InnerException != null)
                        {
                            Console.WriteLine($"      Inner: {loaderEx.InnerException.Message}");
                        }
                    }
                }

                // 尝试获取无法加载的类型信息
                try
                {
                    var assembly = typeof(RiverLi.Blog.Identity.Application.Users.Commands.Login.LoginHandler).Assembly;
                    Console.WriteLine($"\n尝试加载程序集中的类型...");
                    var types = assembly.GetTypes();
                    Console.WriteLine($"成功加载了 {types.Length} 个类型");
                }
                catch (ReflectionTypeLoadException typeLoadEx)
                {
                    Console.WriteLine($"\n程序集类型加载失败:");
                    if (typeLoadEx.Types != null)
                    {
                        Console.WriteLine($"可访问的类型数量: {typeLoadEx.Types.Count(t => t != null)}");
                        Console.WriteLine($"无法加载的类型数量: {typeLoadEx.Types.Count(t => t == null)}");

                        // 显示可访问的类型
                        var accessibleTypes = typeLoadEx.Types.Where(t => t != null).ToList();
                        Console.WriteLine($"\n可访问的类型列表:");
                        foreach (var type in accessibleTypes.Take(10)) // 只显示前10个
                        {
                            Console.WriteLine($"  - {type?.FullName}");
                        }
                        if (accessibleTypes.Count > 10)
                        {
                            Console.WriteLine($"  ... 还有 {accessibleTypes.Count - 10} 个类型");
                        }
                    }
                }
                catch (Exception assemblyEx)
                {
                    Console.WriteLine($"程序集分析失败: {assemblyEx.Message}");
                }

                Console.WriteLine("===========================================");
                throw;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "应用程序启动失败!");
                throw;
            }
            finally
            {
                Log.Information("应用程序关闭...");
                Log.CloseAndFlush();
            }
        }
    }
}