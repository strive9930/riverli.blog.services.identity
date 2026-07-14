using MediatR;
using riverli.blog.services.identity.Application.DTOs.Roles;
using RiverLi.DDD.Core.Application.Common.Models;

namespace riverli.blog.services.identity.Application.Features.Roles.Queries;

public record GetRolesPageQuery(int PageIndex, int PageSize, string? Keyword) : IRequest<PagedResult<RoleDto>>;
