// 路径: Application/Features/Users/Queries/LoginQueryHandler.cs
using MediatR;
using Microsoft.AspNetCore.Identity;
using riverli.blog.services.identity.Application.DTOs;
using riverli.blog.services.identity.Application.Interfaces;
using riverli.blog.services.identity.Domain.Entities;
using System.Security.Authentication;
using riverli.blog.services.identity.Application.DTOs.Auth;
using RiverLi.DDD.Core.Application.Common.Models;
using RiverLi.DDD.Core.Domain.Repositories;
using System.Linq.Expressions;

namespace riverli.blog.services.identity.Application.Features.Users.Queries;

/// <summary>
/// 登录查询处理器 (这才是真正干活的地方)
/// 它必须实现 IRequestHandler<请求类型, 返回类型>
/// </summary>
public class LoginQueryHandler : IRequestHandler<LoginQuery, Result<LoginResponseDto>>
{
    private readonly IRepository<AppUser, Guid> _repository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtProvider _jwtProvider;
    private readonly IMenuRepository _menuRepository;

    // 通过构造函数注入
    public LoginQueryHandler(IRepository<AppUser, Guid> repository, UserManager<AppUser> userManager, IJwtProvider jwtProvider, IMenuRepository menuRepository)
    {
        _repository = repository;
        _userManager = userManager;
        _jwtProvider = jwtProvider;
        _menuRepository = menuRepository;
    }

    // 当 Controller 调用 _mediator.Send() 时，MediatR 会自动执行这个 Handle 方法
    public async Task<Result<LoginResponseDto>> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        // 1. 根据传入的 Username 去数据库查询用户
        var user = await _repository.SingleOrDefaultAsync(u => u.UserName == request.Username, cancellationToken);
        if (user == null)
        {
            throw new InvalidCredentialException("用户名或密码错误"); 
        }

        // 2. 验证密码哈希是否匹配
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            throw new InvalidCredentialException("用户名或密码错误");
        }

        // 3. 检查用户是否被管理员封禁 (IsActive 字段)
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("该账号已被禁用，请联系管理员");
        }

        // 4. 获取该用户的所有角色 (Identity 自动去 sys_user_roles 和 sys_roles 表里查)
        var roles = await _userManager.GetRolesAsync(user);

        // 【调用仓储】获取细粒度权限字符，不再直接碰 _context！
        var permissions = await _menuRepository.GetUserPermissionsAsync(user.Id, cancellationToken);
        
        // 5. 生成包含用户信息和权限的 JWT Token
        var token = _jwtProvider.GenerateToken(user, roles,permissions);

        // 6. 返回组装好的 DTO 数据给 Controller
        return Result<LoginResponseDto>.SuccessResult(new LoginResponseDto(
            Token: token,
            Username: user.UserName!,
            RealName: user.RealName,
            Roles: roles
        ));
    }
}