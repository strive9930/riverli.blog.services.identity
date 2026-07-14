// riverli.blog.services.identity.Infrastructure/Security/ApiPermissionChecker.cs
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Infrastructure.Shared.Security;
using riverli.blog.services.identity.Domain.Entities;
using riverli.blog.services.identity.Infrastructure.Data;

namespace riverli.blog.services.identity.Infrastructure.Security
{
    public class ApiPermissionChecker : IApiPermissionChecker
    {
        private readonly AppDbContext _dbContext;

        public ApiPermissionChecker(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> IsGrantedAsync(Guid userId, string method, string routeTemplate)
        {
            // 1. 超管一律放行
            // 超级管理员默认放行所有权限
            var isAdmin = await _dbContext.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_dbContext.Set<AppRole>(), ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .AnyAsync(name => name == "Admin");
            
            if (isAdmin) return true;

            // 2. 核心比对逻辑：用户 -> 角色 -> 角色API关联表 -> API资源表
            // 判断依据：Method 和 Route 完全一致
            var hasPermission = await _dbContext.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_dbContext.Set<Domain.Entities.RoleApi>(), ur => ur.RoleId, ra => ra.RoleId, (ur, ra) => ra.ApiId)
                .Join(_dbContext.Set<Domain.Entities.ApiResource>(), apiId => apiId, api => api.Id, (apiId, api) => api)
                .AnyAsync(api => api.Method == method && api.Route == routeTemplate);

            return hasPermission;
        }
        /// <summary>
        /// 全局过滤器调用：通过路由模板 + HTTP 方法匹配 ApiResource，再检查用户权限
        /// </summary>
        public async Task<bool> HasApiPermissionAsync(Guid userId, string routePattern, string httpMethod)
        {
            // 1. 先找是否有匹配的 ApiResource 记录
            var apiResource = await _dbContext.ApiResources
                .FirstOrDefaultAsync(a => a.Route == routePattern && a.Method == httpMethod);

            // 2. 无匹配记录 → 该端点未纳入权限管理，默认放行
            if (apiResource == null)
                return true;

            // 3. 公开接口 → 直接放行
            if (apiResource.IsPublic)
                return true;

            // 4. 检查用户是否通过角色关联了该 API 资源
            var hasPermission = await _dbContext.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_dbContext.RoleApis, ur => ur.RoleId, ra => ra.RoleId, (ur, ra) => ra.ApiId)
                .AnyAsync(apiId => apiId == apiResource.Id);

            return hasPermission;
        }
    }
}