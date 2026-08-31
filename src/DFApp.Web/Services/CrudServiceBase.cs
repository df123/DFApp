using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DFApp.Web.Data;
using DFApp.Web.Domain;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using SqlSugar;

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
    /// 是否对单条/列表操作强制对象级所有权校验（防止越权访问他人记录）。
    /// 默认关闭；需要按创建者隔离数据的模块（如文件上传）应重写为 true。
    /// 拥有用户管理权限的账号视为管理员，可访问全部记录（含缺失创建者的历史记录）。
    /// </summary>
    protected virtual bool RequireOwnerCheck => false;

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
    /// 校验当前用户对指定记录的访问权：创建者本人或管理员，否则抛出业务异常
    /// </summary>
    /// <param name="entity">目标实体</param>
    protected async Task EnsureOwnerAsync(TEntity entity)
    {
        if (!RequireOwnerCheck)
        {
            return;
        }

        if (entity is not ICreatorId creatorEntity)
        {
            throw new InvalidOperationException(
                $"{typeof(TEntity).Name} 未实现 {nameof(ICreatorId)}，无法启用所有权校验");
        }

        if (await IsPrivilegedUserAsync())
        {
            return;
        }

        if (creatorEntity.CreatorId is not null &&
            CurrentUser.Id is not null &&
            creatorEntity.CreatorId == CurrentUser.Id)
        {
            return;
        }

        throw new BusinessException("无权访问该记录");
    }

    /// <summary>
    /// 当启用所有权校验且用户非管理员时，返回按创建者过滤的查询条件；否则返回 null。
    /// 过滤条件直接引用实体接口属性，保证可被查询提供器翻译。
    /// </summary>
    protected async Task<Expression<Func<TEntity, bool>>?> BuildOwnerFilterAsync()
    {
        if (!RequireOwnerCheck || !typeof(ICreatorId).IsAssignableFrom(typeof(TEntity)))
        {
            return null;
        }

        if (await IsPrivilegedUserAsync())
        {
            return null;
        }

        return BuildOwnerFilterForCurrentUserId();
    }

    /// <summary>
    /// 为自定义查询附加所有权过滤（供绕过基类方法的派生类查询使用）
    /// </summary>
    protected async Task<ISugarQueryable<TEntity>> ApplyOwnerFilterAsync(ISugarQueryable<TEntity> query)
    {
        var filter = await BuildOwnerFilterAsync();
        return filter is null ? query : query.Where(filter);
    }

    /// <summary>
    /// 为已加载到内存的实体列表过滤所有权（供全量加载后内存处理的派生类使用）
    /// </summary>
    protected async Task<List<TEntity>> FilterOwnedAsync(List<TEntity> entities)
    {
        var filter = await BuildOwnerFilterAsync();
        if (filter is null)
        {
            return entities;
        }

        var predicate = filter.Compile();
        return entities.Where(predicate).ToList();
    }

    /// <summary>
    /// 构造"仅当前用户创建"的过滤条件（未登录时恒为 false）
    /// </summary>
    private Expression<Func<TEntity, bool>>? BuildOwnerFilterForCurrentUserId()
    {
        var userId = CurrentUser.Id;
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        var property = Expression.Property(
            Expression.Convert(parameter, typeof(ICreatorId)),
            nameof(ICreatorId.CreatorId));

        Expression body = userId is null
            ? Expression.Constant(false)
            : Expression.Equal(property, Expression.Constant(userId, typeof(Guid?)));

        return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
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
        await EnsureOwnerAsync(entity);
        return await MapToGetOutputDtoAsync(entity);
    }

    /// <summary>
    /// 获取所有实体列表
    /// </summary>
    /// <returns>输出 DTO 列表</returns>
    public virtual async Task<List<TGetOutputDto>> GetListAsync()
    {
        var ownerFilter = await BuildOwnerFilterAsync();
        var entities = ownerFilter is null
            ? await Repository.GetListAsync()
            : await Repository.GetListAsync(ownerFilter);
        return await MapToGetOutputDtoAsync(entities);
    }

    /// <summary>
    /// 根据条件获取实体列表
    /// </summary>
    /// <param name="expression">查询条件</param>
    /// <returns>输出 DTO 列表</returns>
    public virtual async Task<List<TGetOutputDto>> GetListAsync(Expression<Func<TEntity, bool>> expression)
    {
        var ownerFilter = await BuildOwnerFilterAsync();
        var combined = ownerFilter is null ? expression : CombineExpressions(expression, ownerFilter);
        var entities = await Repository.GetListAsync(combined);
        return await MapToGetOutputDtoAsync(entities);
    }

    /// <summary>
    /// 分页查询（默认按创建时间倒序）
    /// </summary>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>分页结果</returns>
    public virtual async Task<(List<TGetOutputDto> Items, int TotalCount)> GetPagedListAsync(int pageIndex, int pageSize)
    {
        var ownerFilter = await BuildOwnerFilterAsync();
        if (ownerFilter is not null)
        {
            return await GetPagedListAsync(ownerFilter, pageIndex, pageSize);
        }

        if (HasCreationTimeProperty())
        {
            var (items, totalCount) = await Repository.GetPagedListAsync(
                pageIndex, pageSize,
                BuildCreationTimeOrderExpression(),
                OrderByType.Desc);
            var dtos = await MapToGetOutputDtoAsync(items);
            return (dtos, totalCount);
        }

        var (defaultItems, defaultTotalCount) = await Repository.GetPagedListAsync(pageIndex, pageSize);
        var defaultDtos = await MapToGetOutputDtoAsync(defaultItems);
        return (defaultDtos, defaultTotalCount);
    }

    /// <summary>
    /// 根据条件分页查询（默认按创建时间倒序）
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
        var ownerFilter = await BuildOwnerFilterAsync();
        if (ownerFilter is not null)
        {
            expression = CombineExpressions(expression, ownerFilter);
        }

        if (HasCreationTimeProperty())
        {
            var (items, totalCount) = await Repository.GetPagedListAsync(
                expression, pageIndex, pageSize,
                BuildCreationTimeOrderExpression(),
                OrderByType.Desc);
            var dtos = await MapToGetOutputDtoAsync(items);
            return (dtos, totalCount);
        }

        var (defaultItems, defaultTotalCount) = await Repository.GetPagedListAsync(expression, pageIndex, pageSize);
        var defaultDtos = await MapToGetOutputDtoAsync(defaultItems);
        return (defaultDtos, defaultTotalCount);
    }

    /// <summary>
    /// 合并两个查询条件（统一参数表达式，保证可翻译）
    /// </summary>
    private static Expression<Func<TEntity, bool>> CombineExpressions(
        Expression<Func<TEntity, bool>> first,
        Expression<Func<TEntity, bool>> second)
    {
        var parameter = first.Parameters[0];
        var replacedBody = new ReplaceParameterVisitor(second.Parameters[0], parameter).Visit(second.Body);
        var body = Expression.AndAlso(first.Body, replacedBody!);
        return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
    }

    /// <summary>
    /// 表达式参数替换访问器
    /// </summary>
    private sealed class ReplaceParameterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _from;
        private readonly ParameterExpression _to;

        public ReplaceParameterVisitor(ParameterExpression from, ParameterExpression to)
        {
            _from = from;
            _to = to;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _from ? _to : node;
        }
    }

    /// <summary>
    /// 检查实体是否有 CreationTime 属性
    /// </summary>
    private static bool HasCreationTimeProperty()
    {
        return typeof(TEntity).GetProperty("CreationTime") != null;
    }

    /// <summary>
    /// 构建 CreationTime 排序表达式
    /// </summary>
    private static Expression<Func<TEntity, object>> BuildCreationTimeOrderExpression()
    {
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        var property = Expression.PropertyOrField(parameter, "CreationTime");
        var converted = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<TEntity, object>>(converted, parameter);
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
        await EnsureOwnerAsync(entity);

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
        await EnsureOwnerAsync(entity);
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
            var entity = await Repository.GetByIdAsync(id);
            EnsureEntityExists(entity, id);
            await EnsureOwnerAsync(entity);
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
