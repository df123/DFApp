using System;
using System.Linq;
using System.Threading.Tasks;
using User = DFApp.Account.User;
using DFApp.Web.Data;
using DFApp.Web.DTOs.Account;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using DFApp.Web.DTOs;
using Microsoft.Extensions.Logging;

namespace DFApp.Web.Services.Account;

/// <summary>
/// 用户管理应用服务
/// </summary>
public class UserManagementAppService : AppServiceBase
{
    private readonly ISqlSugarRepository<User, Guid> _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UserManagementAppService> _logger;

    public UserManagementAppService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<User, Guid> userRepository,
        IPasswordHasher passwordHasher,
        ILogger<UserManagementAppService> logger)
        : base(currentUser, permissionChecker)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    /// <summary>
    /// 获取用户列表
    /// </summary>
    public async Task<PagedResultDto<UserDto>> GetListAsync(GetUserListDto input)
    {
        await CheckPermissionAsync(DFAppPermissions.UserManagement.Default);

        var queryable = _userRepository.GetQueryable();

        var totalCount = await queryable.CountAsync();

        var users = await queryable
            .OrderByDescending(u => u.CreationTime)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToListAsync();

        // TODO: 使用 Mapperly 映射，暂保留手动映射
        var dtos = users.Select(u => new UserDto
        {
            Id = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            IsActive = u.IsActive,
            CreationTime = u.CreationTime,
            LastModificationTime = u.LastModificationTime
        }).ToList();

        return new PagedResultDto<UserDto>(totalCount, dtos);
    }

    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    public async Task<UserDto> GetAsync(Guid id)
    {
        await CheckPermissionAsync(DFAppPermissions.UserManagement.Default);

        var user = await _userRepository.GetByIdAsync(id);
        EnsureEntityExists(user, id);

        // TODO: 使用 Mapperly 映射，暂保留手动映射
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            IsActive = user.IsActive,
            CreationTime = user.CreationTime,
            LastModificationTime = user.LastModificationTime
        };
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    public async Task<UserDto> CreateAsync(CreateUserDto input)
    {
        await CheckPermissionAsync(DFAppPermissions.UserManagement.Create);

        // 检查用户名是否已存在
        var existingUser = await _userRepository.GetFirstOrDefaultAsync(u => u.UserName == input.UserName);
        if (existingUser != null)
        {
            throw new BusinessException("用户名已存在");
        }

        // 检查邮箱是否已存在
        var existingEmailUser = await _userRepository.GetFirstOrDefaultAsync(u => u.Email == input.Email);
        if (existingEmailUser != null)
        {
            throw new BusinessException("邮箱已被使用");
        }

        // 创建新用户
        var user = new User(Guid.NewGuid(), input.UserName, input.Email)
        {
            IsActive = input.IsActive,
            PasswordHash = _passwordHasher.HashPassword(input.Password)
        };

        await _userRepository.InsertAsync(user);

        // TODO: 使用 Mapperly 映射，暂保留手动映射
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            IsActive = user.IsActive,
            CreationTime = user.CreationTime,
            LastModificationTime = user.LastModificationTime
        };
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto input)
    {
        await CheckPermissionAsync(DFAppPermissions.UserManagement.Update);

        var user = await _userRepository.GetByIdAsync(id);
        EnsureEntityExists(user, id);

        // 检查用户名是否已被其他用户使用
        var existingUser = await _userRepository.GetFirstOrDefaultAsync(u => u.UserName == input.UserName && u.Id != id);
        if (existingUser != null)
        {
            throw new BusinessException("用户名已被其他用户使用");
        }

        // 检查邮箱是否已被其他用户使用
        var existingEmailUser = await _userRepository.GetFirstOrDefaultAsync(u => u.Email == input.Email && u.Id != id);
        if (existingEmailUser != null)
        {
            throw new BusinessException("邮箱已被其他用户使用");
        }

        user.UserName = input.UserName;
        user.Email = input.Email;
        user.IsActive = input.IsActive;

        await _userRepository.UpdateAsync(user);

        // TODO: 使用 Mapperly 映射，暂保留手动映射
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            IsActive = user.IsActive,
            CreationTime = user.CreationTime,
            LastModificationTime = user.LastModificationTime
        };
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        await CheckPermissionAsync(DFAppPermissions.UserManagement.Delete);

        // 防止删除当前登录用户
        if (CurrentUserId == id)
        {
            throw new BusinessException("不能删除当前登录用户");
        }

        await _userRepository.DeleteAsync(id);
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    public async Task ChangePasswordAsync(ChangePasswordDto input)
    {
        await CheckPermissionAsync(DFAppPermissions.UserManagement.ChangePassword);

        var user = await _userRepository.GetByIdAsync(input.UserId);
        EnsureEntityExists(user, input.UserId);

        user.PasswordHash = _passwordHasher.HashPassword(input.NewPassword);
        await _userRepository.UpdateAsync(user);
    }
}

/// <summary>
/// 获取用户列表请求 DTO
/// </summary>
public class GetUserListDto
{
    /// <summary>
    /// 跳过数量
    /// </summary>
    public int SkipCount { get; set; } = 0;

    /// <summary>
    /// 最大返回数量
    /// </summary>
    public int MaxResultCount { get; set; } = 10;
}
