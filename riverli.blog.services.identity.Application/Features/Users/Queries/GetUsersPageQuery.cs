using MediatR;
using riverli.blog.services.identity.Application.DTOs.Users;
using RiverLi.DDD.Core.Application.Common.Models;

namespace riverli.blog.services.identity.Application.Features.Users.Queries;

public record GetUsersPageQuery(int PageIndex, int PageSize, string? Keyword) : IRequest<PagedResult<UserDto>>;
