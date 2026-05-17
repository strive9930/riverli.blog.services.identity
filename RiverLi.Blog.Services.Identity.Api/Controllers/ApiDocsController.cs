using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiverLi.Blog.Identity.Api.Helpers;
//using RiverLi.Blog.Infrastructure.Shared.Controllers;

namespace RiverLi.Blog.Identity.Api.Controllers
{
    /// <summary>
    /// API 文档和路由配置控制器
    /// 提供自动生成的 API 路由信息供前端使用
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ApiDocsController : BaseApiController
    {
        /// <summary>
        /// 获取所有面向前端的 API 路由列表
        /// </summary>
        [HttpGet("routes")]
        [AllowAnonymous]
        public IActionResult GetApiRoutes()
        {
            var routes = FrontendRouteGenerator.GetPublicApiRoutes();
            return Success(routes);
        }

        /// <summary>
        /// 获取 OpenAPI/Swagger 格式的 API 配置
        /// </summary>
        [HttpGet("swagger-config")]
        [AllowAnonymous]
        public IActionResult GetSwaggerConfig()
        {
            var config = FrontendRouteGenerator.GenerateSwaggerConfig();
            return Success(config);
        }

        /// <summary>
        /// 获取 JSON 格式的前端路由配置文件
        /// </summary>
        [HttpGet("frontend-routes.json")]
        [AllowAnonymous]
        public IActionResult GetFrontendRoutesJson()
        {
            var json = FrontendRouteGenerator.GenerateFrontendRoutesJson();
            return Content(json, "application/json");
        }

        /// <summary>
        /// 获取简化的 API 接口清单（Markdown 格式）
        /// </summary>
        [HttpGet("simple-list")]
        [AllowAnonymous]
        public IActionResult GetSimpleApiList()
        {
            var routes = FrontendRouteGenerator.GetPublicApiRoutes();
            
            // 按路径分组
            var grouped = routes.GroupBy(r => r.Path.Split('/').FirstOrDefault())
                .OrderBy(g => g.Key)
                .ToList();

            var markdown = new System.Text.StringBuilder();
            markdown.AppendLine("# RiverLi Blog Identity API 接口清单\n");
            markdown.AppendLine($"**生成时间**: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\n");
            markdown.AppendLine($"**总计**: {routes.Count} 个接口\n\n");
            markdown.AppendLine("---\n\n");

            foreach (var group in grouped)
            {
                markdown.AppendLine($"## /{group.Key}\n");
                markdown.AppendLine("| 方法 | 路径 | 认证 | 说明 |\n");
                markdown.AppendLine("|------|------|------|------|\n");

                foreach (var route in group.OrderBy(r => r.Path))
                {
                    var authMark = route.RequiresAuth ? "✅" : "❌";
                    markdown.AppendLine($"| {route.Method} | `/{route.Path}` | {authMark} | {route.Summary} |\n");
                }

                markdown.AppendLine("\n---\n\n");
            }

            return Content(markdown.ToString(), "text/markdown");
        }

        /// <summary>
        /// 获取 TypeScript 类型定义文件
        /// </summary>
        [HttpGet("types.d.ts")]
        [AllowAnonymous]
        public IActionResult GetTypeScriptDefinitions()
        {
            var ts = @"// RiverLi Blog Identity API - TypeScript Definitions
// 自动生成于: " + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") + @"

// API 响应基础结构
export interface ApiResponse<T = any> {
  success: boolean;
  message: string;
  data: T;
  errors?: Record<string, string[]>;
}

// 用户相关类型
export interface UserInfo {
  id: string;
  email: string;
  nickName?: string;
  avatar?: string;
  phoneNumber?: string;
  isEnabled: boolean;
  roles: string[];
  permissions: string[];
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresIn: number;
  tokenType: string;
  user: UserInfo;
}

export interface RegisterRequest {
  email: string;
  password: string;
  nickName?: string;
  phoneNumber?: string;
}

// 菜单相关类型
export interface MenuTree {
  id: string;
  name: string;
  title: string;
  path: string;
  component: string;
  icon?: string;
  sort: number;
  parentId?: string;
  children: MenuTree[];
}

export interface MenuTreeNode {
  id: string;
  name: string;
  title: string;
  path: string;
  children?: MenuTreeNode[];
}

// 权限相关类型
export interface PermissionInfo {
  id: string;
  name: string;
  code: string;
  description?: string;
  group?: string;
  isEnabled: boolean;
}

export interface RoleInfo {
  id: string;
  name: string;
  code: string;
  description?: string;
  isEnabled: boolean;
}

// 仪表盘统计
export interface DashboardStats {
  totalArticles: number;
  totalComments: number;
  totalViews: number;
  recentVisits: Array<{
    date: string;
    count: number;
  }>;
}

// API Service 接口定义
export interface AuthApiService {
  register(data: RegisterRequest): Promise<ApiResponse<UserInfo>>;
  login(data: LoginRequest): Promise<ApiResponse<LoginResponse>>;
  getCurrentUser(): Promise<ApiResponse<UserInfo>>;
}

export interface UserFrontendApiService {
  getInfo(): Promise<ApiResponse<UserInfo>>;
  getMenus(): Promise<ApiResponse<MenuTree[]>>;
  getPermissions(): Promise<ApiResponse<string[]>>;
  getRoles(): Promise<ApiResponse<RoleInfo[]>>;
  getDashboardStats(): Promise<ApiResponse<DashboardStats>>;
}

// 辅助工具类型
export type HttpMethod = 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH';

export interface ApiEndpoint {
  method: HttpMethod;
  path: string;
  authRequired: boolean;
  description?: string;
}

// 路由配置
export const API_BASE_URL = '/api';

export const AUTH_ENDPOINTS = {
  REGISTER: { method: 'POST' as HttpMethod, path: '/auth/register', authRequired: false },
  LOGIN: { method: 'POST' as HttpMethod, path: '/auth/login', authRequired: false },
  GET_CURRENT_USER: { method: 'GET' as HttpMethod, path: '/auth/me', authRequired: true },
} as const;

export const USER_FRONTEND_ENDPOINTS = {
  GET_INFO: { method: 'GET' as HttpMethod, path: '/userfrontend/info', authRequired: true },
  GET_MENUS: { method: 'GET' as HttpMethod, path: '/userfrontend/menus', authRequired: true },
  GET_PERMISSIONS: { method: 'GET' as HttpMethod, path: '/userfrontend/permissions', authRequired: true },
  GET_ROLES: { method: 'GET' as HttpMethod, path: '/userfrontend/roles', authRequired: true },
  GET_DASHBOARD_STATS: { method: 'GET' as HttpMethod, path: '/userfrontend/dashboard/stats', authRequired: true },
} as const;
";
            return Content(ts, "application/typescript");
        }
    }
}
