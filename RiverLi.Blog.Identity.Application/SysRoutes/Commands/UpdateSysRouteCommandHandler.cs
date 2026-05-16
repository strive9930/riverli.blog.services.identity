using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.SysRoutes.Commands;

public class UpdateSysRouteCommandHandler : IRequestHandler<UpdateSysRouteCommand, Result<bool>>
{
    private readonly IdentityServiceDbContext _context;
    private readonly ILogger<UpdateSysRouteCommandHandler> _logger;

    public UpdateSysRouteCommandHandler(
        IdentityServiceDbContext context,
        ILogger<UpdateSysRouteCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateSysRouteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("开始更新系统路由，ID: {RouteId}", request.Id);
            
            var route = await _context.SysRoutes.FindAsync(request.Id);
            if (route == null)
            {
                _logger.LogWarning("系统路由不存在，ID: {RouteId}", request.Id);
                return Result<bool>.FailResult("路由不存在");
            }

            route.Path = request.Path;
            route.Method = request.Method.ToUpper();
            route.RequiredPermission = request.RequiredPermission;
            route.ServiceName = request.ServiceName;
            route.RouteGroupId = request.RouteGroupId;
            route.FrontendRouteId = request.FrontendRouteId;
            route.IsEnabled = request.IsEnabled;

            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("成功更新系统路由，ID: {RouteId}", request.Id);
            return Result<bool>.SuccessResult(true, "路由更新成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新系统路由失败，ID: {RouteId}", request.Id);
            return Result<bool>.FailResult("路由更新失败");
        }
    }
}
