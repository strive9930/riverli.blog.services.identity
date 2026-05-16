using MediatR;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.DDD.Core.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Infrastructure.Data;

namespace RiverLi.Blog.Identity.Application.Roles.Commands;

/// <summary>
/// 为用户分配权限的命令
/// </summary>
public record AssignPermissionsToUserCommand(
    Guid UserId,
    List<Guid> PermissionIds
) : IRequest<Result<bool>>;

/// <summary>
/// 为用户分配权限的命令处理器
/// </summary>
public class AssignPermissionsToUserCommandHandler : IRequestHandler<AssignPermissionsToUserCommand, Result<bool>>
{
    private readonly IdentityServiceDbContext _context;

    public AssignPermissionsToUserCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(AssignPermissionsToUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Permissions)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                return Result<bool>.FailResult("用户不存在");
            }

            // 获取要分配的权限实体
            var permissions = await _context.Permissions
                .Where(p => request.PermissionIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            // 清空现有权限
            user.Permissions.Clear();
            // 添加新权限
            permissions.ForEach(p => user.Permissions.Add(p));

            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.SuccessResult(true, "用户权限分配成功");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error assigning permissions to user {request.UserId}: {ex.Message}");
            return Result<bool>.FailResult("用户权限分配失败");
        }
    }
}

/// <summary>
/// 清空用户权限的命令
/// </summary>
public record ClearUserPermissionsCommand(Guid UserId) : IRequest<Result<bool>>;

/// <summary>
/// 清空用户权限的命令处理器
/// </summary>
public class ClearUserPermissionsCommandHandler : IRequestHandler<ClearUserPermissionsCommand, Result<bool>>
{
    private readonly IdentityServiceDbContext _context;

    public ClearUserPermissionsCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(ClearUserPermissionsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Permissions)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                return Result<bool>.FailResult("用户不存在");
            }

            // 清空所有权限
            user.Permissions.Clear();
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.SuccessResult(true, "用户权限清空成功");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing permissions for user {request.UserId}: {ex.Message}");
            return Result<bool>.FailResult("用户权限清空失败");
        }
    }
}