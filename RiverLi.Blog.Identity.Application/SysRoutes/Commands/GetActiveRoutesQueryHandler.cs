using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.SysRoutes.Commands;

public class GetActiveRoutesQueryHandler : IRequestHandler<GetActiveRoutesQuery, Result<List<SysRoute>>>
{
    private readonly IdentityServiceDbContext _context;
    private readonly ILogger<GetActiveRoutesQueryHandler> _logger;

    public GetActiveRoutesQueryHandler(
        IdentityServiceDbContext context,
        ILogger<GetActiveRoutesQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<SysRoute>>> Handle(GetActiveRoutesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("查询活跃的系统路由");
            
            var routes = await _context.SysRoutes
                .Where(x => x.IsEnabled)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("成功获取{Count}条活跃系统路由", routes.Count);
            return Result<List<SysRoute>>.SuccessResult(routes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取活跃系统路由失败");
            return Result<List<SysRoute>>.FailResult("获取路由失败");
        }
    }
}
