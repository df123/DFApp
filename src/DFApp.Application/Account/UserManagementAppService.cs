using System;
using System.Linq;
using System.Threading.Tasks;
using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace DFApp.Account;

/// <summary>
/// 用户管理应用服务
/// </summary>
[Authorize(DFAppPermissions.UserManagement.Default)]
public class UserManagementAppService : ApplicationService, IUserManagementAppService
{
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UserManagementAppService(
        IRepository<IdentityUser, Guid> userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// 获取用户列表
    /// </summary>
    [Authorize(DFAppPermissions.UserManagement.Default)]
    public async Task<PagedResultDto<UserDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _userRepository.GetQueryableAsync();

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var users = await AsyncExecuter.ToListAsync(
            queryable
                .OrderByDescending(u => u.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        return new PagedResultDto<UserDto>(
            totalCount,
            users.Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                IsActive = u.IsActive,
                CreationTime = u.CreationTime,
                LastModificationTime = u.LastModificationTime
            }).ToList());
    }

    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    [Authorize(DFAppPermissions.UserManagement.Default)]
    public async Task<UserDto> GetAsync(Guid id)
    {
        var user = await _userRepository.GetAsync(id);
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            IsActive = user.IsActive,
            CreationTime = user.CreationTime,
            LastModificationTime = user.LastModificationTime
        };
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    [Authorize(DFAppPermissions.UserManagement.Create)]
    public async Task<UserDto> CreateAsync(CreateUserDto input)
    {
        // 检查用户名是否已存在
        var queryable = await _userRepository.GetQueryableAsync();
        var existingUser = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(u => u.UserName == input.UserName));

        if (existingUser != null)
        {
            throw new UserFriendlyException("用户名已存在");
        }

        // 检查邮箱是否已存在
        var existingEmailUser = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(u => u.Email == input.Email));
        if (existingEmailUser != null)
        {
            throw new UserFriendlyException("邮箱已被使用");
        }

        // 创建新用户
        var user = new IdentityUser(Guid.NewGuid(), input.UserName, input.Email);

        var isActiveProperty = typeof(IdentityUser).GetProperty("IsActive");
        isActiveProperty?.SetValue(user, input.IsActive);

        var passwordHash = _passwordHasher.HashPassword(input.Password);
        var passwordHashProperty = typeof(IdentityUser).GetProperty("PasswordHash");
        passwordHashProperty?.SetValue(user, passwordHash);

        await _userRepository.InsertAsync(user);

        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            IsActive = user.IsActive,
            CreationTime = user.CreationTime,
            LastModificationTime = user.LastModificationTime
        };
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    [Authorize(DFAppPermissions.UserManagement.Update)]
    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto input)
    {
        var user = await _userRepository.GetAsync(id);

        // 检查用户名是否已被其他用户使用
        var queryable = await _userRepository.GetQueryableAsync();
        var existingUser = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(u => u.UserName == input.UserName && u.Id != id));

        if (existingUser != null)
        {
            throw new UserFriendlyException("用户名已被其他用户使用");
        }

        // 检查邮箱是否已被其他用户使用
        var existingEmailUser = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(u => u.Email == input.Email && u.Id != id));
        if (existingEmailUser != null)
        {
            throw new UserFriendlyException("邮箱已被其他用户使用");
        }

        var userNameProperty = typeof(IdentityUser).GetProperty("UserName");
        userNameProperty?.SetValue(user, input.UserName);

        var emailProperty = typeof(IdentityUser).GetProperty("Email");
        emailProperty?.SetValue(user, input.Email);

        var isActiveProperty = typeof(IdentityUser).GetProperty("IsActive");
        isActiveProperty?.SetValue(user, input.IsActive);

        await _userRepository.UpdateAsync(user);

        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            IsActive = user.IsActive,
            CreationTime = user.CreationTime,
            LastModificationTime = user.LastModificationTime
        };
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    [Authorize(DFAppPermissions.UserManagement.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        // 防止删除当前登录用户
        var currentUserId = CurrentUser.Id;
        if (currentUserId == id)
        {
            throw new UserFriendlyException("不能删除当前登录用户");
        }

        await _userRepository.DeleteAsync(id);
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    [Authorize(DFAppPermissions.UserManagement.ChangePassword)]
    public async Task ChangePasswordAsync(ChangePasswordDto input)
    {
        var user = await _userRepository.GetAsync(input.UserId);

        var newPasswordHash = _passwordHasher.HashPassword(input.NewPassword);
        var passwordHashProperty = typeof(IdentityUser).GetProperty("PasswordHash");
        passwordHashProperty?.SetValue(user, newPasswordHash);

        await _userRepository.UpdateAsync(user);
    }
}
