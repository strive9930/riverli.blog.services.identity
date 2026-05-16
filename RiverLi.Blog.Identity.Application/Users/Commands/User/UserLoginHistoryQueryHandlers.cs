using MediatR;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Users.Commands.User;

/// <summary>
/// 获取用户登录历史查询处理器
/// </summary>
public class GetUserLoginHistoryQueryHandler : IRequestHandler<GetUserLoginHistoryQuery, UserLoginHistoryResult>
{
    private readonly IdentityServiceDbContext _context;

    public GetUserLoginHistoryQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<UserLoginHistoryResult> Handle(GetUserLoginHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.UserLoginHistories
                .Include(ulh => ulh.User)
                .AsQueryable();

            // 应用过滤条件
            if (request.UserId.HasValue)
            {
                query = query.Where(ulh => ulh.UserId == request.UserId.Value);
            }

            if (!string.IsNullOrEmpty(request.IpAddress))
            {
                query = query.Where(ulh => ulh.IpAddress.Contains(request.IpAddress));
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                query = query.Where(ulh => ulh.Status == request.Status);
            }

            if (request.StartTime.HasValue)
            {
                query = query.Where(ulh => ulh.LoginTime >= request.StartTime.Value);
            }

            if (request.EndTime.HasValue)
            {
                query = query.Where(ulh => ulh.LoginTime <= request.EndTime.Value);
            }

            // 获取总数
            var totalCount = await query.CountAsync(cancellationToken);

            // 分页查询
            var histories = await query
                .OrderByDescending(ulh => ulh.LoginTime)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(ulh => new UserLoginHistoryDto
                {
                    Id = ulh.Id,
                    UserId = ulh.UserId,
                    UserEmail = ulh.User.Email ?? string.Empty,
                    UserNickName = ulh.User.NickName,
                    LoginTime = ulh.LoginTime,
                    IpAddress = ulh.IpAddress,
                    Location = ulh.Location,
                    DeviceInfo = ulh.DeviceInfo,
                    BrowserInfo = ulh.BrowserInfo,
                    OperatingSystem = ulh.OperatingSystem,
                    Status = ulh.Status,
                    FailureReason = ulh.FailureReason,
                    IsFirstLogin = ulh.IsFirstLogin
                })
                .ToListAsync(cancellationToken);

            return new UserLoginHistoryResult(totalCount, request.PageIndex, request.PageSize, histories);
        }
        catch (Exception ex)
        {
            throw new Exception($"获取用户登录历史失败: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// 获取用户最近登录历史查询处理器
/// </summary>
public class GetUserRecentLoginHistoryQueryHandler : IRequestHandler<GetUserRecentLoginHistoryQuery, Result<List<UserLoginHistoryDto>>>
{
    private readonly IdentityServiceDbContext _context;

    public GetUserRecentLoginHistoryQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<UserLoginHistoryDto>>> Handle(GetUserRecentLoginHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var histories = await _context.UserLoginHistories
                .Include(ulh => ulh.User)
                .Where(ulh => ulh.UserId == request.UserId)
                .OrderByDescending(ulh => ulh.LoginTime)
                .Take(request.Count)
                .Select(ulh => new UserLoginHistoryDto
                {
                    Id = ulh.Id,
                    UserId = ulh.UserId,
                    UserEmail = ulh.User.Email ?? string.Empty,
                    UserNickName = ulh.User.NickName,
                    LoginTime = ulh.LoginTime,
                    IpAddress = ulh.IpAddress,
                    Location = ulh.Location,
                    DeviceInfo = ulh.DeviceInfo,
                    BrowserInfo = ulh.BrowserInfo,
                    OperatingSystem = ulh.OperatingSystem,
                    Status = ulh.Status,
                    FailureReason = ulh.FailureReason,
                    IsFirstLogin = ulh.IsFirstLogin
                })
                .ToListAsync(cancellationToken);

            return Result<List<UserLoginHistoryDto>>.SuccessResult(histories);
        }
        catch (Exception ex)
        {
            return Result<List<UserLoginHistoryDto>>.FailResult($"获取用户最近登录历史失败: {ex.Message}");
        }
    }
}

/// <summary>
/// 获取用户登录统计信息查询处理器
/// </summary>
public class GetUserLoginStatisticsQueryHandler : IRequestHandler<GetUserLoginStatisticsQuery, Result<UserLoginStatisticsDto>>
{
    private readonly IdentityServiceDbContext _context;

    public GetUserLoginStatisticsQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UserLoginStatisticsDto>> Handle(GetUserLoginStatisticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userHistories = await _context.UserLoginHistories
                .Where(ulh => ulh.UserId == request.UserId)
                .ToListAsync(cancellationToken);

            if (!userHistories.Any())
            {
                return Result<UserLoginStatisticsDto>.SuccessResult(new UserLoginStatisticsDto());
            }

            var statistics = new UserLoginStatisticsDto
            {
                TotalLoginCount = userHistories.Count,
                SuccessfulLoginCount = userHistories.Count(h => h.Status == "Success"),
                FailedLoginCount = userHistories.Count(h => h.Status == "Failed"),
                FirstLoginTime = userHistories.Min(h => h.LoginTime),
                LastLoginTime = userHistories.Max(h => h.LoginTime),
                Recent7DaysLoginCount = userHistories.Count(h => h.LoginTime >= DateTime.UtcNow.AddDays(-7)),
                Recent30DaysLoginCount = userHistories.Count(h => h.LoginTime >= DateTime.UtcNow.AddDays(-30))
            };

            // 获取常用IP地址
            statistics.FrequentIpAddresses = userHistories
                .GroupBy(h => h.IpAddress)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToList();

            // 获取常用设备
            statistics.FrequentDevices = userHistories
                .GroupBy(h => h.DeviceInfo)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToList();

            return Result<UserLoginStatisticsDto>.SuccessResult(statistics);
        }
        catch (Exception ex)
        {
            return Result<UserLoginStatisticsDto>.FailResult($"获取用户登录统计信息失败: {ex.Message}");
        }
    }
}