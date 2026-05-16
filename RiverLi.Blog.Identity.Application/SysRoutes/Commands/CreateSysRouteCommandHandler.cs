using MediatR;
using Microsoft.Extensions.Logging;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.SysRoutes.Commands;

public class CreateSysRouteCommandHandler : IRequestHandler<CreateSysRouteCommand, Result<Guid>>
{
    private readonly IdentityServiceDbContext _context;
    private readonly ILogger<CreateSysRouteCommandHandler> _logger;

    public CreateSysRouteCommandHandler(
        IdentityServiceDbContext context,
        ILogger<CreateSysRouteCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateSysRouteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("开始创建系统路由，Path: {Path}, Method: {Method}", request.Path, request.Method);
            
            var newRoute = new SysRoute
            {
                Id = Guid.NewGuid(),
                Path = request.Path,
                Method = request.Method.ToUpper(),
                RequiredPermission = request.RequiredPermission,
                IsEnabled = request.IsEnabled,
                ServiceName = request.ServiceName,
                RouteGroupId = request.RouteGroupId,
                FrontendRouteId = request.FrontendRouteId
            };

            _context.SysRoutes.Add(newRoute);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("成功创建系统路由，ID: {RouteId}", newRoute.Id);
            return Result<Guid>.SuccessResult(newRoute.Id, "路由创建成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建系统路由失败，Path: {Path}", request.Path);
            return Result<Guid>.FailResult("路由创建失败");
        }
    }
}
