using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Roles.Queries;

public class UpdateUserRolesCommandHandler : IRequestHandler<UpdateUserRolesCommand, Result<bool>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UpdateUserRolesCommandHandler> _logger;

        public UpdateUserRolesCommandHandler(
            UserManager<ApplicationUser> userManager,
            ILogger<UpdateUserRolesCommandHandler> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("开始更新用户角色，用户 ID: {UserId}", request.UserId);
                
                // 查找用户
                var user = await _userManager.FindByIdAsync(request.UserId.ToString());
                if (user == null)
                {
                    _logger.LogWarning("用户不存在，ID: {UserId}", request.UserId);
                    return Result<bool>.FailResult("用户不存在");
                }

                // 获取用户当前的角色
                var currentRoles = await _userManager.GetRolesAsync(user);

                // 移除用户当前不在新列表中的角色
                var rolesToRemove = currentRoles.Except(request.RoleNames).ToList();
                if (rolesToRemove.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    if (!removeResult.Succeeded)
                    {
                        // 处理移除角色失败的情况
                        var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                        _logger.LogWarning("移除旧角色失败：{Errors}", errors);
                        return Result<bool>.FailResult($"移除旧角色失败：{errors}");
                    }
                    _logger.LogDebug("成功移除用户的角色：{Roles}", string.Join(", ", rolesToRemove));
                }

                // 添加用户不在当前列表中的新角色
                var rolesToAdd = request.RoleNames.Except(currentRoles).ToList();
                if (rolesToAdd.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                    if (!addResult.Succeeded)
                    {
                        // 处理添加角色失败的情况
                        var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                        _logger.LogWarning("添加新角色失败：{Errors}", errors);
                        return Result<bool>.FailResult($"添加新角色失败：{errors}");
                    }
                    _logger.LogDebug("成功添加用户的角色：{Roles}", string.Join(", ", rolesToAdd));
                }

                _logger.LogInformation("成功更新用户角色，用户 ID: {UserId}", request.UserId);
                return Result<bool>.SuccessResult(true, "角色更新成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户角色失败，用户 ID: {UserId}", request.UserId);
                return Result<bool>.FailResult("角色更新失败");
            }
        }
    }
