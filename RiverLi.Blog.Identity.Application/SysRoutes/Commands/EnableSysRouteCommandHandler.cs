using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.SysRoutes.Commands;

public class EnableSysRouteCommandHandler : IRequestHandler<EnableSysRouteCommand, Result<string>>
{
    private readonly IdentityServiceDbContext _context;
    private readonly ILogger<EnableSysRouteCommandHandler> _logger;

    public EnableSysRouteCommandHandler(
        IdentityServiceDbContext context,
        ILogger<EnableSysRouteCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(EnableSysRouteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("开始启用系统路由，ID: {RouteId}", request.Id);
            
            var route = await _context.SysRoutes.FindAsync(request.Id);
            if (route == null)
            {
                _logger.LogWarning("系统路由不存在，ID: {RouteId}", request.Id);
                return Result<string>.FailResult("路由不存在");
            }

            route.IsEnabled = true;
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("成功启用系统路由，ID: {RouteId}", request.Id);
            return Result<string>.SuccessResult("路由已启用");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启用系统路由失败，ID: {RouteId}", request.Id);
            return Result<string>.FailResult("启用路由失败");
        }
    }
}
