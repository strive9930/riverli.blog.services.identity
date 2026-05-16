using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.SysRoutes.Commands;

public class BatchDeleteSysRoutesCommandHandler : IRequestHandler<BatchDeleteSysRoutesCommand, Result<int>>
{
    private readonly IdentityServiceDbContext _context;
    private readonly ILogger<BatchDeleteSysRoutesCommandHandler> _logger;

    public BatchDeleteSysRoutesCommandHandler(
        IdentityServiceDbContext context,
        ILogger<BatchDeleteSysRoutesCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(BatchDeleteSysRoutesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("开始批量删除系统路由，数量：{Count}", request.RouteIds.Count);
            
            var routes = await _context.SysRoutes
                .Where(r => request.RouteIds.Contains(r.Id))
                .ToListAsync(cancellationToken);

            if (!routes.Any())
            {
                _logger.LogWarning("未找到要删除的系统路由");
                return Result<int>.FailResult("未找到要删除的路由");
            }

            _context.SysRoutes.RemoveRange(routes);
            var deletedCount = await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("成功删除{DeletedCount}条系统路由", deletedCount);
            return Result<int>.SuccessResult(deletedCount, $"成功删除{deletedCount}条路由");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量删除系统路由失败");
            return Result<int>.FailResult("批量删除路由失败");
        }
    }
}
