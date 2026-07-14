using riverli.blog.services.identity.Application.DTOs;
using riverli.blog.services.identity.Domain.Entities;

namespace riverli.blog.services.identity.Application.Utils;

public static class TreeBuilder
{
    /// <summary>
    /// 将扁平的菜单实体列表转换为树形结构的 DTO 列表 (高性能版)
    /// </summary>
    public static List<MenuTreeDto> BuildMenuTree(List<Menu> flatMenus)
    {
        if (flatMenus == null || flatMenus.Count == 0)
            return new List<MenuTreeDto>();

        // 1. 先将所有的 Entity 映射为 DTO
        var allMenuDtos = flatMenus.Select(m => new MenuTreeDto
        {
            Id = m.Id,
            Name = m.Name,
            Title = m.Title,
            Path = m.Path,
            Component = m.Component,
            Icon = m.Icon,
            ParentId = m.ParentId,
            SortOrder = m.SortOrder,
            Type = (int)m.Type
        }).ToList();

        // 2. 核心优化：使用 ToLookup 按照 ParentId 对数据进行分组。
        // 这样在内存中查找子节点时，速度极快
        var lookup = allMenuDtos.ToLookup(m => m.ParentId);

        // 3. 定义内部递归函数
        List<MenuTreeDto> Build(Guid? parentId)
        {
            return lookup[parentId]
                .OrderBy(m => m.SortOrder) // 保证同级菜单按 SortOrder 排序
                .Select(m => 
                {
                    // 递归查找当前节点的子节点
                    m.Children = Build(m.Id);
                    return m;
                })
                .ToList();
        }

        // 4. 从顶级节点 (ParentId 为 null) 开始构建整棵树
        return Build(null);
    }
}