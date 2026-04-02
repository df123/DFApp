using System;
using System.Threading.Tasks;
using DFApp.Web.Data;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using Microsoft.AspNetCore.Mvc;

namespace DFApp.Web.Controllers;

/// <summary>
/// 控制器基类，提供通用的控制器功能
/// </summary>
[ApiController]
[Route("api/app/[controller]")]
public abstract class DFAppControllerBase : ControllerBase
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
    protected DFAppControllerBase(ICurrentUser currentUser, IPermissionChecker permissionChecker)
    {
        CurrentUser = currentUser;
        PermissionChecker = permissionChecker;
    }

    /// <summary>
    /// 返回成功的响应
    /// </summary>
    /// <param name="data">响应数据</param>
    /// <returns>成功响应</returns>
    protected IActionResult Success(object? data = null)
    {
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "操作成功",
            Data = data
        });
    }

    /// <summary>
    /// 返回成功的响应（带消息）
    /// </summary>
    /// <param name="message">响应消息</param>
    /// <param name="data">响应数据</param>
    /// <returns>成功响应</returns>
    protected IActionResult Success(string message, object? data = null)
    {
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = message,
            Data = data
        });
    }

    /// <summary>
    /// 返回失败的响应
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <returns>失败响应</returns>
    protected IActionResult Fail(string message)
    {
        return BadRequest(new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Data = null
        });
    }

    /// <summary>
    /// 返回失败的响应（带错误代码）
    /// </summary>
    /// <param name="code">错误代码</param>
    /// <param name="message">错误消息</param>
    /// <returns>失败响应</returns>
    protected IActionResult Fail(string code, string message)
    {
        return BadRequest(new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Code = code,
            Data = null
        });
    }

    /// <summary>
    /// 返回未找到的响应
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <returns>未找到响应</returns>
    protected IActionResult NotFound(string message)
    {
        return NotFound(new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Data = null
        });
    }

    /// <summary>
    /// 抛出业务异常
    /// </summary>
    /// <param name="message">错误消息</param>
    protected void ThrowBusinessException(string message)
    {
        throw new BusinessException(message);
    }

    /// <summary>
    /// 抛出业务异常（带错误代码）
    /// </summary>
    /// <param name="code">错误代码</param>
    /// <param name="message">错误消息</param>
    protected void ThrowBusinessException(string code, string message)
    {
        throw new BusinessException(code, message);
    }

    /// <summary>
    /// 抛出未找到异常
    /// </summary>
    /// <param name="resourceType">资源类型</param>
    /// <param name="resourceId">资源 ID</param>
    protected void ThrowNotFoundException(string resourceType, object resourceId)
    {
        throw new NotFoundException(resourceType, resourceId);
    }

    /// <summary>
    /// 抛出验证异常
    /// </summary>
    /// <param name="propertyName">属性名称</param>
    /// <param name="errorMessage">错误消息</param>
    protected void ThrowValidationException(string propertyName, string errorMessage)
    {
        throw new ValidationException(propertyName, errorMessage);
    }

    /// <summary>
    /// 检查当前用户是否拥有指定权限
    /// </summary>
    /// <param name="permissionName">权限名称</param>
    /// <returns>如果拥有权限返回 true，否则返回 false</returns>
    protected async Task<bool> HasPermissionAsync(string permissionName)
    {
        return await PermissionChecker.IsGrantedAsync(permissionName);
    }

    /// <summary>
    /// 检查当前用户是否拥有指定权限，如果没有则抛出异常
    /// </summary>
    /// <param name="permissionName">权限名称</param>
    protected async Task CheckPermissionAsync(string permissionName)
    {
        if (!await HasPermissionAsync(permissionName))
        {
            throw new UnauthorizedAccessException($"您没有权限执行此操作，需要权限: {permissionName}");
        }
    }
}
