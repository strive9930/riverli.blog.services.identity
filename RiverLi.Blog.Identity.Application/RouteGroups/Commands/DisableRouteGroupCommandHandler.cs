using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.RouteGroups.Commands;

public class DisableRouteGroupCommandHandler : IRequestHandler<DisableRouteGroupCommand, Result<string>>
{
    private readonly IdentityServiceDbContext _context;
    private readonly ILogger<DisableRouteGroupCommandHandler> _logger;

    public DisableRouteGroupCommandHandler(
        IdentityServiceDbContext context,
        ILogger<DisableRouteGroupCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(DisableRouteGroupCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("开始禁用路由分组，ID: {RouteGroupId}", request.Id);
            
            var routeGroup = await _context.RouteGroups.FindAsync(request.Id);
            if (routeGroup == null)
            {
                _logger.LogWarning("路由分组不存在，ID: {RouteGroupId}", request.Id);
                return Result<string>.FailResult("路由分组不存在");
            }

            routeGroup.IsEnabled = false;
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("成功禁用路由分组，ID: {RouteGroupId}", request.Id);
            return Result<string>.SuccessResult("路由分组已禁用");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "禁用路由分组失败，ID: {RouteGroupId}", request.Id);
            return Result<string>.FailResult("禁用路由分组失败");
        }
    }
}
