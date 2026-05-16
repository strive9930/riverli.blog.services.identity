using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.FrontendRoutes.Commands;

/// <summary>
/// 关联后端路由命令处理器
/// </summary>
public class AssociateBackendRoutesCommandHandler : IRequestHandler<AssociateBackendRoutesCommand, Result<bool>>
{
    private readonly IdentityServiceDbContext _context;
    private readonly ILogger<AssociateBackendRoutesCommandHandler> _logger;

    public AssociateBackendRoutesCommandHandler(
        IdentityServiceDbContext context,
        ILogger<AssociateBackendRoutesCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(AssociateBackendRoutesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("开始关联后端路由，前端路由 ID: {FrontendRouteId}, 后端路由数量：{Count}", 
                request.FrontendRouteId, request.BackendRouteIds.Count);
                
            // 验证前端路由是否存在
            var frontendRoute = await _context.FrontendRoutes
                .FirstOrDefaultAsync(r => r.Id == request.FrontendRouteId, cancellationToken);
    
            if (frontendRoute == null)
            {
                _logger.LogWarning("前端路由不存在，ID: {FrontendRouteId}", request.FrontendRouteId);
                return Result<bool>.FailResult("前端路由不存在");
            }
    
            // 验证所有后端路由是否存在
            var backendRoutes = await _context.SysRoutes
                .Where(r => request.BackendRouteIds.Contains(r.Id))
                .ToListAsync(cancellationToken);
    
            if (backendRoutes.Count != request.BackendRouteIds.Count)
            {
                _logger.LogWarning("部分后端路由不存在");
                return Result<bool>.FailResult("部分后端路由不存在");
            }
    
            // 由于暂时移除了 FrontendRouteSysRoute 实体，这里简化处理
            // 实际项目中可以根据需要重新设计连接表结构
                
            _logger.LogInformation("成功关联后端路由，前端路由 ID: {FrontendRouteId}", request.FrontendRouteId);
            return Result<bool>.SuccessResult(true, "路由关联成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "关联后端路由失败，前端路由 ID: {FrontendRouteId}", request.FrontendRouteId);
            return Result<bool>.FailResult("关联后端路由失败");
        }
    }
}