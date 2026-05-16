using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.RouteGroups.Commands;

public class BatchUpdateRouteGroupStatusCommandHandler : IRequestHandler<BatchUpdateRouteGroupStatusCommand, Result<int>>
{
    private readonly IdentityServiceDbContext _context;
    private readonly ILogger<BatchUpdateRouteGroupStatusCommandHandler> _logger;

    public BatchUpdateRouteGroupStatusCommandHandler(
        IdentityServiceDbContext context,
        ILogger<BatchUpdateRouteGroupStatusCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(BatchUpdateRouteGroupStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("开始批量更新路由分组状态，数量：{Count}, 启用状态：{IsEnabled}", 
                request.RouteGroupIds?.Count ?? 0, request.IsEnabled);
            
            if (request.RouteGroupIds == null || !request.RouteGroupIds.Any())
            {
                _logger.LogWarning("未选择要更新的路由分组");
                return Result<int>.FailResult("请选择要更新的路由分组");
            }

            var routeGroups = await _context.RouteGroups
                .Where(rg => request.RouteGroupIds.Contains(rg.Id))
                .ToListAsync(cancellationToken);

            if (!routeGroups.Any())
            {
                _logger.LogWarning("未找到要更新的路由分组");
                return Result<int>.FailResult("未找到要更新的路由分组");
            }

            foreach (var routeGroup in routeGroups)
            {
                routeGroup.IsEnabled = request.IsEnabled;
            }

            var updatedCount = await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("成功更新{UpdatedCount}个路由分组的状态", updatedCount);
            return Result<int>.SuccessResult(updatedCount, $"成功更新{updatedCount}个路由分组的状态");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量更新路由分组状态失败");
            return Result<int>.FailResult("批量更新路由分组状态失败");
        }
    }
}
