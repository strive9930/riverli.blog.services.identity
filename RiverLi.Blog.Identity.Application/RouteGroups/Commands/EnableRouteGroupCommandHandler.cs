using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.RouteGroups.Commands;

public class EnableRouteGroupCommandHandler : IRequestHandler<EnableRouteGroupCommand, Result<string>>
{
    private readonly IdentityServiceDbContext _context;
    private readonly ILogger<EnableRouteGroupCommandHandler> _logger;

    public EnableRouteGroupCommandHandler(
        IdentityServiceDbContext context,
        ILogger<EnableRouteGroupCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(EnableRouteGroupCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("开始启用路由分组，ID: {RouteGroupId}", request.Id);
            
            var routeGroup = await _context.RouteGroups.FindAsync(request.Id);
            if (routeGroup == null)
            {
                _logger.LogWarning("路由分组不存在，ID: {RouteGroupId}", request.Id);
                return Result<string>.FailResult("路由分组不存在");
            }

            routeGroup.IsEnabled = true;
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("成功启用路由分组，ID: {RouteGroupId}", request.Id);
            return Result<string>.SuccessResult("路由分组已启用");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启用路由分组失败，ID: {RouteGroupId}", request.Id);
            return Result<string>.FailResult("启用路由分组失败");
        }
    }
}
