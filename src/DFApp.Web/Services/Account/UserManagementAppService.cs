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
    private readonly ISqlSugarReadOnlyRepository<DFApp.Identity.UserRole, Guid> _userRoleReadOnlyRepository;
    private readonly ISqlSugarReadOnlyRepository<DFApp.Identity.Role, Guid> _roleReadOnlyRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UserManagementAppService> _logger;

    public UserManagementAppService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<User, Guid> userRepository,
        ISqlSugarReadOnlyRepository<DFApp.Identity.UserRole, Guid> userRoleReadOnlyRepository,
        ISqlSugarReadOnlyRepository<DFApp.Identity.Role, Guid> roleReadOnlyRepository,
        IPasswordHasher passwordHasher,
        ILogger<UserManagementAppService> logger)
        : base(currentUser, permissionChecker)
    {
        _userRepository = userRepository;
        _userRoleReadOnlyRepository = userRoleReadOnlyRepository;
        _roleReadOnlyRepository = roleReadOnlyRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    /// <summary>
    /// 判断目标用户是否持有 admin 角色（用于保护管理员账户不被非管理员修改/删除/重置密码）
    /// </summary>
    private async Task<bool> UserHasAdminRoleAsync(Guid userId)
    {
        var userIdUpper = userId.ToString().ToUpperInvariant();
        var userRoles = await _userRoleReadOnlyRepository.GetQueryable()
            .Where("UPPER(UserId) = @UserId", new { UserId = userIdUpper })
            .ToListAsync();
        if (userRoles.Count == 0)
        {
            return false;
        }

        var adminRole = await _roleReadOnlyRepository.GetQueryable()
            .Where(r => r.Name == AdminRoleName)
            .FirstAsync();
        if (adminRole == null)
        {
            return false;
        }

        var adminRoleId = adminRole.Id.ToString();
        return userRoles.Any(ur => string.Equals(ur.RoleId.ToString(), adminRoleId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 校验对目标用户执行敏感操作（改密/更新/删除）的权限：
    /// 目标为 admin 角色用户时，调用者必须持有 admin 角色
    /// </summary>
    private async Task EnsureTargetUserModifiableAsync(Guid targetUserId)
    {
        if (await IsAdminUserAsync())
        {
            return;
        }

        if (await UserHasAdminRoleAsync(targetUserId))
        {
            throw new BusinessException("不能对管理员账户执行该操作：仅限 admin 角色");
        }
    }

    /// <summary>
    /// 获取用户列表
    /// </summary>
    public async Task<PagedResultDto<UserDto>> GetListAsync(GetUserListDto input)
    {
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
        var user = await _userRepository.GetByIdAsync(id);
        EnsureEntityExists(user, id);
        await EnsureTargetUserModifiableAsync(id);

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
        // 防止删除当前登录用户
        if (CurrentUserId == id)
        {
            throw new BusinessException("不能删除当前登录用户");
        }

        await EnsureTargetUserModifiableAsync(id);
        await _userRepository.DeleteAsync(id);
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    public async Task ChangePasswordAsync(ChangePasswordDto input)
    {
        var user = await _userRepository.GetByIdAsync(input.UserId);
        EnsureEntityExists(user, input.UserId);

        var isSelf = CurrentUserId.HasValue
            && string.Equals(input.UserId.ToString(), CurrentUserId.Value.ToString(), StringComparison.OrdinalIgnoreCase);

        if (isSelf)
        {
            // 修改本人密码必须校验原密码
            if (string.IsNullOrWhiteSpace(input.CurrentPassword)
                || !_passwordHasher.VerifyPassword(user.PasswordHash, input.CurrentPassword))
            {
                throw new BusinessException("原密码不正确");
            }
        }
        else
        {
            // 重置他人密码：目标为管理员账户时仅限 admin 角色
            await EnsureTargetUserModifiableAsync(input.UserId);
        }

        user.PasswordHash = _passwordHasher.HashPassword(input.NewPassword);
        await _userRepository.UpdateAsync(user);
        _logger.LogInformation("修改用户密码：UserId={UserId}, Operator={Operator}, IsSelf={IsSelf}",
            input.UserId, CurrentUserName ?? "unknown", isSelf);
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
