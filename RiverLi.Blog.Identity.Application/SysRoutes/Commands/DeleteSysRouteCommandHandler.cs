using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.SysRoutes.Commands;

public class DeleteSysRouteCommandHandler : IRequestHandler<DeleteSysRouteCommand, Result<bool>>
{
    private readonly IdentityServiceDbContext _context;
    private readonly ILogger<DeleteSysRouteCommandHandler> _logger;

    public DeleteSysRouteCommandHandler(
        IdentityServiceDbContext context,
        ILogger<DeleteSysRouteCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteSysRouteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("开始删除系统路由，ID: {RouteId}", request.Id);
            
            var route = await _context.SysRoutes.FindAsync(request.Id);
            if (route == null)
            {
                _logger.LogWarning("系统路由不存在，ID: {RouteId}", request.Id);
                return Result<bool>.FailResult("路由不存在");
            }

            _context.SysRoutes.Remove(route);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("成功删除系统路由，ID: {RouteId}", request.Id);
            return Result<bool>.SuccessResult(true, "路由删除成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除系统路由失败，ID: {RouteId}", request.Id);
            return Result<bool>.FailResult("路由删除失败");
        }
    }
}
