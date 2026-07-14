namespace riverli.blog.services.identity.Domain.Enums;

/// <summary>
/// 菜单类型枚举
/// </summary>
public enum MenuType
{
    /// <summary>
    /// 目录：仅用于分组导航，无实际页面
    /// </summary>
    Directory = 0,

    /// <summary>
    /// 页面：可访问的具体路由页面
    /// </summary>
    Page = 1,

    /// <summary>
    /// 按钮：用于细粒度权限控制，如 "user:add"
    /// </summary>
    Button = 2
}