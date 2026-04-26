using System;
using System.Threading.Tasks;
using DFApp.Web.Permissions;
using DFApp.Web.Infrastructure;

namespace DFApp.Web.Services;

/// <summary>
/// 应用服务基类，提供通用的应用服务功能
/// </summary>
public abstract class AppServiceBase
{
    /// <summary>
    /// 当前用户
    /// </summary>
    protected ICurrentUser CurrentUser { get; }

    /// <summary>
    /// 权限检查器
    /// </summary>
    protected IPermissionChecker PermissionChecker { get; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    protected AppServiceBase(ICurrentUser currentUser, IPermissionChecker permissionChecker)
    {
        CurrentUser = currentUser;
        PermissionChecker = permissionChecker;
    }

    /// <summary>
    /// 获取当前用户 ID
    /// </summary>
    protected Guid? CurrentUserId => CurrentUser.Id;

    /// <summary>
    /// 获取当前用户名
    /// </summary>
    protected string? CurrentUserName => CurrentUser.UserName;

    /// <summary>
    /// 检查当前用户是否拥有指定权限
    /// </summary>
    /// <param name="permissionName">权限名称</param>
    /// <returns>如果拥有权限返回 true，否则返回 false</returns>
    protected Task<bool> IsGrantedAsync(string permissionName)
    {
        return PermissionChecker.IsGrantedAsync(permissionName);
    }

    /// <summary>
    /// 检查当前用户是否拥有指定权限，如果没有权限则抛出异常
    /// </summary>
    /// <param name="permissionName">权限名称</param>
    /// <exception cref="BusinessException">当用户没有权限时抛出</exception>
    protected async Task CheckPermissionAsync(string permissionName)
    {
        if (!await IsGrantedAsync(permissionName))
        {
            throw new BusinessException($"用户没有权限：{permissionName}");
        }
    }

    /// <summary>
    /// 确保当前用户已登录，如果未登录则抛出异常
    /// </summary>
    /// <exception cref="BusinessException">当用户未登录时抛出</exception>
    protected void EnsureLoggedIn()
    {
        if (!CurrentUserId.HasValue)
        {
            throw new BusinessException("用户未登录");
        }
    }

    /// <summary>
    /// 检查实体是否存在，如果不存在则抛出异常
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TKey">主键类型</typeparam>
    /// <param name="entity">实体</param>
    /// <param name="id">主键 ID</param>
    /// <exception cref="NotFoundException">当实体不存在时抛出</exception>
    protected void EnsureEntityExists<TEntity, TKey>(TEntity? entity, TKey id) where TEntity : class
    {
        if (entity == null)
        {
            throw new NotFoundException($"{typeof(TEntity).Name} 不存在，ID：{id}");
        }
    }

    /// <summary>
    /// 检查实体是否存在，如果不存在则抛出异常
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="entity">实体</param>
    /// <param name="message">自定义错误消息</param>
    /// <exception cref="NotFoundException">当实体不存在时抛出</exception>
    protected void EnsureEntityExists<TEntity>(TEntity? entity, string? message = null) where TEntity : class
    {
        if (entity == null)
        {
            throw new NotFoundException(message ?? $"{typeof(TEntity).Name} 不存在");
        }
    }
}
