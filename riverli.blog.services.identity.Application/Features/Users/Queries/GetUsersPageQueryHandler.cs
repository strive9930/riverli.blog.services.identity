using System.Linq.Expressions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using riverli.blog.services.identity.Application.DTOs.Users;
using riverli.blog.services.identity.Domain.Entities;
using RiverLi.DDD.Core.Application.Common.Models;
using RiverLi.DDD.Core.Domain.Repositories;

namespace riverli.blog.services.identity.Application.Features.Users.Queries;

public class GetUsersPageQueryHandler : IRequestHandler<GetUsersPageQuery, PagedResult<UserDto>>
{
    private readonly IRepository<AppUser, Guid> _repository;
    public GetUsersPageQueryHandler(IRepository<AppUser, Guid> repository) => _repository = repository;

    public async Task<PagedResult<UserDto>> Handle(GetUsersPageQuery request, CancellationToken cancellationToken)
    {
        // 构建可选的过滤表达式
        Expression<Func<AppUser, bool>>? filter = null;
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            filter = u => u.UserName!.Contains(request.Keyword) || u.RealName.Contains(request.Keyword);
        }

        // 使用仓储内置分页（自动处理排序、分页、计数）
        var pagedResult = await _repository.GetPagedAsync(
            new PagedQuery { PageIndex = request.PageIndex, PageSize = request.PageSize },
            filter,
            cancellationToken);

        // 将实体映射为 DTO
        var userDtos = pagedResult.Data.Select(u => new UserDto(
            u.Id, u.UserName!, u.RealName, u.Email!, u.IsActive, u.CreatedAt)).ToList();

        return PagedResult<UserDto>.SuccessResult(userDtos, pagedResult.TotalCount, request.PageIndex, request.PageSize);
    }
}
