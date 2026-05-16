using MediatR;
using Microsoft.Extensions.Logging;

namespace RiverLi.Blog.Identity.Application.Common.Behaviors;

/// <summary>
/// 全局异常处理管道行为
/// 统一捕获并记录所有 Command/Query 处理过程中的异常
/// </summary>
public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;

    public ExceptionHandlingBehavior(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;
            
            _logger.LogError(ex, 
                "处理请求 {RequestName} 时发生异常。错误信息：{ErrorMessage}", 
                requestName, ex.Message);
            
            // 重新抛出异常，让全局异常中间件处理
            // 或者根据 TResponse 类型返回统一的错误结果
            throw;
        }
    }
}
