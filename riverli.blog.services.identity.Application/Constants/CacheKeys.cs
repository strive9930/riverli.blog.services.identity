// Application/Constants/CacheKeys.cs
namespace riverli.blog.services.identity.Application.Constants;

public static class CacheKeys
{
    /// <summary>
    /// 全局菜单树的缓存键
    /// </summary>
    public const string SysMenuTree = "SYS_MENU_TREE_CACHE";

    /// <summary>
    /// API 资源列表的缓存键
    /// </summary>
    public const string SysApiResources = "SYS_API_RESOURCES_CACHE";

    /// <summary>
    /// API 资源树的缓存键
    /// </summary>
    public const string SysApiTree = "SYS_API_TREE_CACHE";
}