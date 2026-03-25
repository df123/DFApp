using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DFApp.Account;

/// <summary>
/// 用户管理应用服务接口
/// </summary>
public interface IUserManagementAppService : IApplicationService
{
    /// <summary>
    /// 获取用户列表
    /// </summary>
    /// <param name="input">分页请求</param>
    /// <returns>用户列表</returns>
    Task<PagedResultDto<UserDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <returns>用户信息</returns>
    Task<UserDto> GetAsync(Guid id);

    /// <summary>
    /// 创建用户
    /// </summary>
    /// <param name="input">创建用户请求</param>
    /// <returns>用户信息</returns>
    Task<UserDto> CreateAsync(CreateUserDto input);

    /// <summary>
    /// 更新用户
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <param name="input">更新用户请求</param>
    /// <returns>用户信息</returns>
    Task<UserDto> UpdateAsync(Guid id, UpdateUserDto input);

    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="id">用户ID</param>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="input">修改密码请求</param>
    Task ChangePasswordAsync(ChangePasswordDto input);
}
