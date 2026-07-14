using MediatR;
using riverli.blog.services.identity.Application.DTOs.ApiResources;

namespace riverli.blog.services.identity.Application.Features.ApiResources.Queries;

// 1. 定义查询命令
public record GetApiResourceTreeQuery() : IRequest<List<ApiResourceTreeNodeDto>>;