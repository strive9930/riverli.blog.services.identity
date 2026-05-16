using MediatR;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.DDD.Core.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Infrastructure.Data;

namespace RiverLi.Blog.Identity.Application.Roles.Queries;

/// <summary>
/// 清空角色权限的命令
/// </summary>
public record ClearRolePermissionsCommand(
    Guid RoleId
) : IRequest<Result<bool>>;

/// <summary>
/// 清空角色权限的命令处理器
/// </summary>
public class ClearRolePermissionsCommandHandler : IRequestHandler<ClearRolePermissionsCommand, Result<bool>>
{
    private readonly IdentityServiceDbContext _context;

    public ClearRolePermissionsCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(ClearRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var role = await _context.Roles
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

            if (role == null)
            {
                return Result<bool>.FailResult("角色不存在");
            }

            // 清空所有权限
            role.Permissions.Clear();
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.SuccessResult(true, "角色权限清空成功");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing permissions for role {request.RoleId}: {ex.Message}");
            return Result<bool>.FailResult("角色权限清空失败");
        }
    }
}