using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DFApp.Web.Data;
using DFApp.Web.Domain;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;

namespace DFApp.Web.Services;

/// <summary>
/// CRUD 服务基类，提供标准的 CRUD 操作
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
/// <typeparam name="TKey">主键类型</typeparam>
/// <typeparam name="TGetOutputDto">获取输出 DTO 类型</typeparam>
/// <typeparam name="TCreateInputDto">创建输入 DTO 类型</typeparam>
/// <typeparam name="TUpdateInputDto">更新输入 DTO 类型</typeparam>
public abstract class CrudServiceBase<TEntity, TKey, TGetOutputDto, TCreateInputDto, TUpdateInputDto> : AppServiceBase
    where TEntity : class, IEntity<TKey>, new()
{
    /// <summary>
    /// 仓储接口
    /// </summary>
    protected ISqlSugarRepository<TEntity, TKey> Repository { get; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    /// <param name="repository">仓储接口</param>
    protected CrudServiceBase(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<TEntity, TKey> repository)
        : base(currentUser, permissionChecker)
    {
        Repository = repository;
    }

    /// <summary>
    /// 根据 ID 获取实体
    /// </summary>
    /// <param name="id">主键 ID</param>
    /// <returns>输出 DTO</returns>
    public virtual async Task<TGetOutputDto> GetAsync(TKey id)
    {
        var entity = await Repository.GetByIdAsync(id);
        EnsureEntityExists(entity, id);
        return await MapToGetOutputDtoAsync(entity);
    }

    /// <summary>
    /// 获取所有实体列表
    /// </summary>
    /// <returns>输出 DTO 列表</returns>
    public virtual async Task<List<TGetOutputDto>> GetListAsync()
    {
        var entities = await Repository.GetListAsync();
        return await MapToGetOutputDtoAsync(entities);
    }

    /// <summary>
    /// 根据条件获取实体列表
    /// </summary>
    /// <param name="expression">查询条件</param>
    /// <returns>输出 DTO 列表</returns>
    public virtual async Task<List<TGetOutputDto>> GetListAsync(Expression<Func<TEntity, bool>> expression)
    {
        var entities = await Repository.GetListAsync(expression);
        return await MapToGetOutputDtoAsync(entities);
    }

    /// <summary>
    /// 分页查询
    /// </summary>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>分页结果</returns>
    public virtual async Task<(List<TGetOutputDto> Items, int TotalCount)> GetPagedListAsync(int pageIndex, int pageSize)
    {
        var (items, totalCount) = await Repository.GetPagedListAsync(pageIndex, pageSize);
        var dtos = await MapToGetOutputDtoAsync(items);
        return (dtos, totalCount);
    }

    /// <summary>
    /// 根据条件分页查询
    /// </summary>
    /// <param name="expression">查询条件</param>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>分页结果</returns>
    public virtual async Task<(List<TGetOutputDto> Items, int TotalCount)> GetPagedListAsync(
        Expression<Func<TEntity, bool>> expression,
        int pageIndex,
        int pageSize)
    {
        var (items, totalCount) = await Repository.GetPagedListAsync(expression, pageIndex, pageSize);
        var dtos = await MapToGetOutputDtoAsync(items);
        return (dtos, totalCount);
    }

    /// <summary>
    /// 创建实体
    /// </summary>
    /// <param name="input">创建输入 DTO</param>
    /// <returns>输出 DTO</returns>
    public virtual async Task<TGetOutputDto> CreateAsync(TCreateInputDto input)
    {
        var entity = await MapToEntityAsync(input);
        await Repository.InsertAsync(entity);
        return await MapToGetOutputDtoAsync(entity);
    }

    /// <summary>
    /// 批量创建实体
    /// </summary>
    /// <param name="inputs">创建输入 DTO 列表</param>
    /// <returns>输出 DTO 列表</returns>
    public virtual async Task<List<TGetOutputDto>> CreateAsync(List<TCreateInputDto> inputs)
    {
        var entities = new List<TEntity>();
        foreach (var input in inputs)
        {
            var entity = await MapToEntityAsync(input);
            entities.Add(entity);
        }

        await Repository.InsertAsync(entities);
        return await MapToGetOutputDtoAsync(entities);
    }

    /// <summary>
    /// 更新实体
    /// </summary>
    /// <param name="id">主键 ID</param>
    /// <param name="input">更新输入 DTO</param>
    /// <returns>输出 DTO</returns>
    public virtual async Task<TGetOutputDto> UpdateAsync(TKey id, TUpdateInputDto input)
    {
        var entity = await Repository.GetByIdAsync(id);
        EnsureEntityExists(entity, id);

        await MapToEntityAsync(input, entity);
        await Repository.UpdateAsync(entity);

        return await MapToGetOutputDtoAsync(entity);
    }

    /// <summary>
    /// 删除实体
    /// </summary>
    /// <param name="id">主键 ID</param>
    public virtual async Task DeleteAsync(TKey id)
    {
        var entity = await Repository.GetByIdAsync(id);
        EnsureEntityExists(entity, id);
        await Repository.DeleteAsync(id);
    }

    /// <summary>
    /// 批量删除实体
    /// </summary>
    /// <param name="ids">主键 ID 列表</param>
    public virtual async Task DeleteAsync(List<TKey> ids)
    {
        foreach (var id in ids)
        {
            await Repository.DeleteAsync(id);
        }
    }

    /// <summary>
    /// 将实体映射为输出 DTO
    /// </summary>
    /// <param name="entity">实体</param>
    /// <returns>输出 DTO</returns>
    protected virtual Task<TGetOutputDto> MapToGetOutputDtoAsync(TEntity entity)
    {
        return Task.FromResult(MapToGetOutputDto(entity));
    }

    /// <summary>
    /// 将实体列表映射为输出 DTO 列表
    /// </summary>
    /// <param name="entities">实体列表</param>
    /// <returns>输出 DTO 列表</returns>
    protected virtual Task<List<TGetOutputDto>> MapToGetOutputDtoAsync(List<TEntity> entities)
    {
        var dtos = entities.Select(MapToGetOutputDto).ToList();
        return Task.FromResult(dtos);
    }

    /// <summary>
    /// 将创建输入 DTO 映射为实体
    /// </summary>
    /// <param name="input">创建输入 DTO</param>
    /// <returns>实体</returns>
    protected virtual Task<TEntity> MapToEntityAsync(TCreateInputDto input)
    {
        return Task.FromResult(MapToEntity(input));
    }

    /// <summary>
    /// 将更新输入 DTO 映射到现有实体
    /// </summary>
    /// <param name="input">更新输入 DTO</param>
    /// <param name="entity">实体</param>
    protected virtual Task MapToEntityAsync(TUpdateInputDto input, TEntity entity)
    {
        MapToEntity(input, entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 将实体映射为输出 DTO（同步方法，子类应重写此方法或 MapToGetOutputDtoAsync）
    /// </summary>
    /// <param name="entity">实体</param>
    /// <returns>输出 DTO</returns>
    protected virtual TGetOutputDto MapToGetOutputDto(TEntity entity)
    {
        throw new NotImplementedException(
            $"请重写 {nameof(MapToGetOutputDto)} 或 {nameof(MapToGetOutputDtoAsync)} 方法以实现实体到 DTO 的映射。" +
            "建议使用 Mapperly 的 [Mapper] 特性创建映射器类。");
    }

    /// <summary>
    /// 将创建输入 DTO 映射为实体（同步方法，子类应重写此方法或 MapToEntityAsync）
    /// </summary>
    /// <param name="input">创建输入 DTO</param>
    /// <returns>实体</returns>
    protected virtual TEntity MapToEntity(TCreateInputDto input)
    {
        throw new NotImplementedException(
            $"请重写 {nameof(MapToEntity)} 或 {nameof(MapToEntityAsync)} 方法以实现 DTO 到实体的映射。" +
            "建议使用 Mapperly 的 [Mapper] 特性创建映射器类。");
    }

    /// <summary>
    /// 将更新输入 DTO 映射到现有实体（同步方法，子类应重写此方法或 MapToEntityAsync）
    /// </summary>
    /// <param name="input">更新输入 DTO</param>
    /// <param name="entity">实体</param>
    protected virtual void MapToEntity(TUpdateInputDto input, TEntity entity)
    {
        throw new NotImplementedException(
            $"请重写 {nameof(MapToEntity)} 或 {nameof(MapToEntityAsync)} 方法以实现 DTO 到实体的映射。" +
            "建议使用 Mapperly 的 [Mapper] 特性创建映射器类。");
    }
}
