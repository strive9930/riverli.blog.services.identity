using MediatR;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.DDD.Core.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Infrastructure.Data;
using System.Text.Json;

namespace RiverLi.Blog.Identity.Application.Permissions.Commands;

#region 权限分组管理处理器

/// <summary>
/// 获取权限分组的查询处理器
/// </summary>
public class GetPermissionGroupsQueryHandler : IRequestHandler<GetPermissionGroupsQuery, Result<List<PermissionGroupDto>>>
{
    private readonly IdentityServiceDbContext _context;

    public GetPermissionGroupsQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<PermissionGroupDto>>> Handle(GetPermissionGroupsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // 从权限表中按Group字段分组统计
            var groups = await _context.Permissions
                .GroupBy(p => p.Group)
                .Select(g => new PermissionGroupDto
                {
                    Name = g.Key,
                    Code = g.Key.Replace(" ", "").ToLower(), // 简单的代码生成逻辑
                    Description = $"{g.Key}权限分组",
                    PermissionCount = g.Count(),
                    CreateTime = DateTime.UtcNow
                })
                .OrderBy(g => g.Name)
                .ToListAsync(cancellationToken);

            return Result<List<PermissionGroupDto>>.SuccessResult(groups);
        }
        catch (Exception ex)
        {
            return Result<List<PermissionGroupDto>>.FailResult($"获取权限分组失败: {ex.Message}");
        }
    }
}

/// <summary>
/// 创建权限分组的命令处理器
/// </summary>
public class CreatePermissionGroupCommandHandler : IRequestHandler<CreatePermissionGroupCommand, Result<PermissionGroupDto>>
{
    private readonly IdentityServiceDbContext _context;

    public CreatePermissionGroupCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PermissionGroupDto>> Handle(CreatePermissionGroupCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 注意：这里实际上是在创建一个属于该分组的示例权限
            // 因为权限分组概念在数据库层面是通过Permission.Group字段体现的
            var samplePermission = new Permission
            {
                Id = Guid.NewGuid(),
                Name = $"{request.Name}示例权限",
                Code = $"{request.Code}.sample",
                Description = request.Description,
                Group = request.Name, // 使用Name作为分组标识
                ClaimType = "Permission",
                ClaimValue = $"{request.Code}.sample",
                RoleId = ""
            };

            _context.Permissions.Add(samplePermission);
            await _context.SaveChangesAsync(cancellationToken);

            var result = new PermissionGroupDto
            {
                Name = request.Name,
                Code = request.Code,
                Description = request.Description,
                PermissionCount = 1,
                CreateTime = DateTime.UtcNow
            };

            return Result<PermissionGroupDto>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            return Result<PermissionGroupDto>.FailResult($"创建权限分组失败: {ex.Message}");
        }
    }
}

#endregion

#region 权限导入导出处理器

/// <summary>
/// 导出权限的查询处理器
/// </summary>
public class ExportPermissionsQueryHandler : IRequestHandler<ExportPermissionsQuery, Result<string>>
{
    private readonly IdentityServiceDbContext _context;

    public ExportPermissionsQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(ExportPermissionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.Permissions.AsQueryable();

            if (!string.IsNullOrEmpty(request.Group))
            {
                query = query.Where(p => p.Group == request.Group);
            }

            var permissions = await query
                .Select(p => new
                {
                    p.Name,
                    p.Code,
                    p.Description,
                    p.Group,
                    p.ClaimType,
                    p.ClaimValue
                })
                .ToListAsync(cancellationToken);

            var exportData = new
            {
                ExportTime = DateTime.UtcNow,
                TotalCount = permissions.Count,
                Permissions = permissions
            };

            var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            return Result<string>.SuccessResult(json);
        }
        catch (Exception ex)
        {
            return Result<string>.FailResult($"导出权限失败: {ex.Message}");
        }
    }
}

/// <summary>
/// 导入权限的命令处理器
/// </summary>
public class ImportPermissionsCommandHandler : IRequestHandler<ImportPermissionsCommand, Result<string>>
{
    private readonly IdentityServiceDbContext _context;

    public ImportPermissionsCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(ImportPermissionsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var importedCount = 0;
            var skippedCount = 0;
            var errorMessages = new List<string>();

            foreach (var importPermission in request.Permissions)
            {
                try
                {
                    // 检查权限编码是否已存在
                    if (await _context.Permissions.AnyAsync(p => p.Code == importPermission.Code, cancellationToken))
                    {
                        skippedCount++;
                        continue;
                    }

                    var permission = new Permission
                    {
                        Id = Guid.NewGuid(),
                        Name = importPermission.Name,
                        Code = importPermission.Code,
                        Description = importPermission.Description,
                        Group = importPermission.Group,
                        ClaimType = importPermission.ClaimType ?? "Permission",
                        ClaimValue = importPermission.ClaimValue ?? importPermission.Code,
                        RoleId = "",
                        CreateTime = DateTime.UtcNow, // 设置创建时间
                        IsEnabled = true // 默认启用
                    };

                    _context.Permissions.Add(permission);
                    importedCount++;
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"导入权限 '{importPermission.Name}' 失败: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            var resultMessage = $"导入完成：成功导入 {importedCount} 个权限";
            if (skippedCount > 0)
            {
                resultMessage += $"，跳过 {skippedCount} 个重复权限";
            }
            if (errorMessages.Any())
            {
                resultMessage += $"，{errorMessages.Count} 个权限导入失败";
            }

            return Result<string>.SuccessResult(resultMessage);
        }
        catch (Exception ex)
        {
            return Result<string>.FailResult($"导入权限失败: {ex.Message}");
        }
    }
}

#endregion