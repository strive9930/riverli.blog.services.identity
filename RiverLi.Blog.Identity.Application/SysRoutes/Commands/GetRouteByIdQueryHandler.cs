using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.SysRoutes.Commands;

public class GetRouteByIdQueryHandler : IRequestHandler<GetRouteByIdQuery, Result<SysRoute>>
{
    private readonly IdentityServiceDbContext _context;
    private readonly ILogger<GetRouteByIdQueryHandler> _logger;

    public GetRouteByIdQueryHandler(
        IdentityServiceDbContext context,
        ILogger<GetRouteByIdQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<SysRoute>> Handle(GetRouteByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("查询系统路由详情，ID: {RouteId}", request.Id);
            
            var route = await _context.SysRoutes
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (route == null)
            {
                _logger.LogWarning("系统路由不存在，ID: {RouteId}", request.Id);
                return Result<SysRoute>.FailResult("路由不存在");
            }

            _logger.LogInformation("成功获取系统路由详情，ID: {RouteId}", request.Id);
            return Result<SysRoute>.SuccessResult(route);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取系统路由详情失败，ID: {RouteId}", request.Id);
            return Result<SysRoute>.FailResult("获取路由失败");
        }
    }
}
