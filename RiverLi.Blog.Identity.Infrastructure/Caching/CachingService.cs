using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.Blog.Identity.Domain.Entities;

namespace RiverLi.Blog.Identity.Infrastructure.Caching
{
    /// <summary>
    /// 缓存服务 - 用于优化常用查询性能
    /// </summary>
    public interface ICachingService
    {
        Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpirationRelativeToNow = null);
        void Remove(string key);
        void RemoveByPrefix(string prefix);
    }

    /// <summary>
    /// 内存缓存服务实现
    /// </summary>
    public class CachingService : ICachingService
    {
        private readonly IMemoryCache _cache;
        private readonly IServiceProvider _serviceProvider;
        private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(10);

        public CachingService(IMemoryCache cache, IServiceProvider serviceProvider)
        {
            _cache = cache;
            _serviceProvider = serviceProvider;
        }

        public async Task<T?> GetOrCreateAsync<T>(
            string key, 
            Func<Task<T>> factory, 
            TimeSpan? absoluteExpirationRelativeToNow = null)
        {
            return await _cache.GetOrCreateAsync(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow ?? DefaultCacheDuration;
                return factory();
            });
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public void RemoveByPrefix(string prefix)
        {
            // 注意：IMemoryCache 不支持直接按前缀删除
            // 这里可以通过在 key 中包含版本号来实现批量失效
            // 或者使用更复杂的缓存包装器
            var versionKey = $"{prefix}:version";
            var version = _cache.Get<int>(versionKey) + 1;
            _cache.Set(versionKey, version);
        }

        /// <summary>
        /// 获取带版本号的缓存键
        /// </summary>
        public string GetVersionedKey(string baseKey)
        {
            var versionKey = $"{baseKey}:version";
            var version = _cache.Get<int>(versionKey);
            return $"{baseKey}:v{version}";
        }
    }

    /// <summary>
    /// 缓存键常量
    /// </summary>
    public static class CacheKeys
    {
        public const string AllPermissions = "permissions:all";
        public const string AllRoles = "roles:all";
        public const string AllFrontendRoutes = "frontend-routes:all";
        public const string AllMenus = "menus:all";
        public const string MenuTree = "menus:tree";
        public const string FrontendRouteTree = "frontend-routes:tree";
        public const string RouteGroups = "route-groups:all";
        
        public static string RolePermissions(Guid roleId) => $"role:{roleId}:permissions";
        public static string UserPermissions(Guid userId) => $"user:{userId}:permissions";
        public static string UserRoles(Guid userId) => $"user:{userId}:roles";
    }
}
