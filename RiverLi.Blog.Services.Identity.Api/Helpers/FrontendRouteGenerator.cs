using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace RiverLi.Blog.Identity.Api.Helpers
{
    /// <summary>
    /// 前端路由自动生成器
    /// 用于从 API 控制器生成前端可用的路由配置
    /// </summary>
    public static class FrontendRouteGenerator
    {
        /// <summary>
        /// 获取所有面向前端的 API 路由信息
        /// </summary>
        public static List<ApiRouteInfo> GetPublicApiRoutes()
        {
            var routes = new List<ApiRouteInfo>();
            var assembly = Assembly.GetExecutingAssembly();
            
            // 获取所有控制器
            var controllers = assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(ControllerBase)) 
                         && !t.Name.Contains("Management")) // 排除管理后台控制器
                .ToList();

            foreach (var controller in controllers)
            {
                var controllerRoute = GetControllerRoute(controller);
                var requiresAuth = ControllerRequiresAuth(controller);
                
                // 获取控制器的所有方法
                var methods = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.DeclaringType != typeof(ControllerBase) 
                             && m.DeclaringType != typeof(object));

                foreach (var method in methods)
                {
                    var routeInfo = ExtractRouteInfo(method, controllerRoute, requiresAuth);
                    if (routeInfo != null)
                    {
                        routes.Add(routeInfo);
                    }
                }
            }

            return routes;
        }

        /// <summary>
        /// 获取控制器的路由前缀
        /// </summary>
        private static string GetControllerRoute(Type controllerType)
        {
            var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();
            if (routeAttr != null)
            {
                var template = routeAttr.Template;
                // 处理 [controller] 占位符
                if (template.Contains("[controller]"))
                {
                    var controllerName = controllerType.Name.Replace("Controller", "");
                    template = template.Replace("[controller]", controllerName.ToLower());
                }
                return template.TrimStart('/');
            }
            return "";
        }

        /// <summary>
        /// 检查控制器是否需要认证
        /// </summary>
        private static bool ControllerRequiresAuth(Type controllerType)
        {
            return controllerType.GetCustomAttribute<AuthorizeAttribute>() != null;
        }

        /// <summary>
        /// 从方法中提取路由信息
        /// </summary>
        private static ApiRouteInfo? ExtractRouteInfo(MethodInfo method, string controllerRoute, bool requiresAuth)
        {
            // 检查 HTTP 方法属性
            string? httpMethod = null;
            string? template = null;
            
            if (method.GetCustomAttribute<HttpGetAttribute>() is HttpGetAttribute getAttr)
            {
                httpMethod = "GET";
                template = getAttr.Template;
            }
            else if (method.GetCustomAttribute<HttpPostAttribute>() is HttpPostAttribute postAttr)
            {
                httpMethod = "POST";
                template = postAttr.Template;
            }
            else if (method.GetCustomAttribute<HttpPutAttribute>() is HttpPutAttribute putAttr)
            {
                httpMethod = "PUT";
                template = putAttr.Template;
            }
            else if (method.GetCustomAttribute<HttpDeleteAttribute>() is HttpDeleteAttribute deleteAttr)
            {
                httpMethod = "DELETE";
                template = deleteAttr.Template;
            }

            if (httpMethod == null) return null;

            var methodRequiresAuth = method.GetCustomAttribute<AuthorizeAttribute>() != null;
            template = template ?? "";
            
            // 构建完整路径
            var fullPath = string.IsNullOrEmpty(template) 
                ? controllerRoute 
                : $"{controllerRoute}/{template.TrimStart('/')}";

            // 清理路径中的双斜杠
            fullPath = fullPath.Replace("//", "/");

            // 获取方法的 Summary 注释
            var summary = GetMethodSummary(method);

            return new ApiRouteInfo
            {
                Method = httpMethod,
                Path = fullPath,
                RequiresAuth = requiresAuth || methodRequiresAuth,
                Summary = summary,
                OperationId = method.Name
            };
        }

        /// <summary>
        /// 获取方法的 XML 注释摘要
        /// </summary>
        private static string GetMethodSummary(MethodInfo method)
        {
            // 这里可以读取 XML 文档获取注释
            // 简化版本直接返回方法名
            return method.Name;
        }

        /// <summary>
        /// 生成 OpenAPI/Swagger 兼容的路由配置
        /// </summary>
        public static object GenerateSwaggerConfig()
        {
            var routes = GetPublicApiRoutes();
            
            return new
            {
                openapi = "3.0.1",
                info = new
                {
                    title = "RiverLi Blog Identity API",
                    version = "v1",
                    description = "面向前端用户的身份认证和权限管理 API"
                },
                servers = new[]
                {
                    new { url = "http://localhost:5001", description = "开发环境" },
                    new { url = "https://api.riverli.com", description = "生产环境" }
                },
                paths = routes.GroupBy(r => r.Path)
                    .ToDictionary(
                        g => $"/{g.Key}",
                        g => g.ToDictionary(
                            r => r.Method.ToLower(),
                            r => new
                            {
                                summary = r.Summary,
                                operationId = r.OperationId,
                                security = r.RequiresAuth ? new[] { new { BearerToken = new string[0] } } : null,
                                responses = new
                                {
                                    _200 = new
                                    {
                                        description = "成功",
                                        content = new
                                        {
                                            application_json = new
                                            {
                                                schema = new
                                                {
                                                    type = "object",
                                                    properties = new
                                                    {
                                                        success = new { type = "boolean" },
                                                        message = new { type = "string" },
                                                        data = new { type = "object" }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        )
                    ),
                components = new
                {
                    securitySchemes = new
                    {
                        BearerToken = new
                        {
                            type = "http",
                            scheme = "bearer",
                            bearerFormat = "JWT"
                        }
                    }
                }
            };
        }

        /// <summary>
        /// 生成前端路由配置文件（JSON 格式）
        /// </summary>
        public static string GenerateFrontendRoutesJson()
        {
            var routes = GetPublicApiRoutes();
            
            var config = new
            {
                generatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                baseUrl = "/api",
                endpoints = routes.Select(r => new
                {
                    method = r.Method,
                    path = r.Path,
                    authRequired = r.RequiresAuth,
                    description = r.Summary,
                    operationId = r.OperationId
                }).OrderBy(e => e.path).ToList(),
                
                // 按功能分组
                groups = new
                {
                    auth = routes.Where(r => r.Path.StartsWith("auth/")).Select(MapToEndpoint).ToList(),
                    user = routes.Where(r => r.Path.StartsWith("userfrontend/")).Select(MapToEndpoint).ToList(),
                    permissions = routes.Where(r => r.Path.StartsWith("permissions/")).Select(MapToEndpoint).ToList()
                }
            };

            return System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }

        private static object MapToEndpoint(ApiRouteInfo route)
        {
            return new
            {
                method = route.Method,
                path = route.Path,
                authRequired = route.RequiresAuth,
                description = route.Summary
            };
        }
    }

    /// <summary>
    /// API 路由信息
    /// </summary>
    public class ApiRouteInfo
    {
        /// <summary>
        /// HTTP 方法 (GET, POST, PUT, DELETE)
        /// </summary>
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// API 路径（不包含 /api 前缀）
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// 是否需要认证
        /// </summary>
        public bool RequiresAuth { get; set; }

        /// <summary>
        /// 接口描述
        /// </summary>
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// 操作 ID（方法名）
        /// </summary>
        public string OperationId { get; set; } = string.Empty;
    }
}
