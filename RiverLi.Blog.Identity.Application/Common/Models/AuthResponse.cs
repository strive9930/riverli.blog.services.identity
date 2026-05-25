using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiverLi.Blog.Identity.Application.Common.Models
{
    /// <summary>
    /// 身份认证成功后的返回信息
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// JWT 访问令牌
        /// </summary>
        public string Token { get; set; } = string.Empty;
        
        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime Expiration { get; set; }
        
        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid UserId { get; set; }
        
        /// <summary>
        /// 用户昵称
        /// </summary>
        public string? Nickname { get; set; }
        
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// 邮箱
        /// </summary>
        public string? Email { get; set; }
        
        /// <summary>
        /// 头像 URL
        /// </summary>
        public string? Avatar { get; set; }
        
        /// <summary>
        /// 用户角色列表
        /// </summary>
        public List<RoleInfo> Roles { get; set; } = new();
        
        /// <summary>
        /// 用户权限码列表
        /// </summary>
        public List<string> Permissions { get; set; } = new();
        
        /// <summary>
        /// 用户菜单树（仅后台管理菜单）
        /// </summary>
        public List<MenuTreeItem> Menus { get; set; } = new();
        
        /// <summary>
        /// 是否为管理员
        /// </summary>
        public bool IsAdmin { get; set; }
        
        /// <summary>
        /// 账户创建时间
        /// </summary>
        public DateTime? CreatedAt { get; set; }
    }
    
    /// <summary>
    /// 角色信息
    /// </summary>
    public class RoleInfo
    {
        /// <summary>
        /// 角色 ID
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// 角色名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 角色编码
        /// </summary>
        public string Code { get; set; } = string.Empty;
        
        /// <summary>
        /// 角色描述
        /// </summary>
        public string? Description { get; set; }
    }
    
    /// <summary>
    /// 菜单树项
    /// </summary>
    public class MenuTreeItem
    {
        /// <summary>
        /// 菜单 ID
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// 菜单名称（英文标识）
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 菜单标题（显示名称）
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// 菜单路径
        /// </summary>
        public string Path { get; set; } = string.Empty;
        
        /// <summary>
        /// 菜单图标
        /// </summary>
        public string? Icon { get; set; }
        
        /// <summary>
        /// 排序值
        /// </summary>
        public int Sort { get; set; }
        
        /// <summary>
        /// 所需权限码
        /// </summary>
        public string? RequiredPermission { get; set; }
        
        /// <summary>
        /// 子菜单列表
        /// </summary>
        public List<MenuTreeItem> Children { get; set; } = new();
    }
}
