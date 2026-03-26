using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SqlSugar;

namespace DFApp.Web.Data;

/// <summary>
/// SqlSugar 仓储实现
/// </summary>
/// <typeparam name="T">实体类型</typeparam>
/// <typeparam name="TKey">主键类型</typeparam>
public class SqlSugarRepository<T, TKey> : ISqlSugarRepository<T, TKey> where T : class, new()
{
    private readonly ISqlSugarClient _db;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db">SqlSugar 客户端</param>
    public SqlSugarRepository(ISqlSugarClient db)
    {
        _db = db;
    }

    /// <summary>
    /// 根据 ID 获取实体
    /// </summary>
    /// <param name="id">主键 ID</param>
    /// <returns>实体</returns>
    public async Task<T?> GetByIdAsync(TKey id)
    {
        return await _db.Queryable<T>().In(id).FirstAsync();
    }

    /// <summary>
    /// 根据条件获取单个实体
    /// </summary>
    /// <param name="expression">查询条件</param>
    /// <returns>实体</returns>
    public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> expression)
    {
        return await _db.Queryable<T>().FirstAsync(expression);
    }

    /// <summary>
    /// 获取所有实体列表
    /// </summary>
    /// <returns>实体列表</returns>
    public async Task<List<T>> GetListAsync()
    {
        return await _db.Queryable<T>().ToListAsync();
    }

    /// <summary>
    /// 根据条件获取实体列表
    /// </summary>
    /// <param name="expression">查询条件</param>
    /// <returns>实体列表</returns>
    public async Task<List<T>> GetListAsync(Expression<Func<T, bool>> expression)
    {
        return await _db.Queryable<T>().Where(expression).ToListAsync();
    }

    /// <summary>
    /// 分页查询
    /// </summary>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>分页结果</returns>
    public async Task<(List<T> Items, int TotalCount)> GetPagedListAsync(int pageIndex, int pageSize)
    {
        RefAsync<int> totalCount = 0;
        var items = await _db.Queryable<T>()
            .ToPageListAsync(pageIndex, pageSize, totalCount);
        return (items, totalCount.Value);
    }

    /// <summary>
    /// 根据条件分页查询
    /// </summary>
    /// <param name="expression">查询条件</param>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>分页结果</returns>
    public async Task<(List<T> Items, int TotalCount)> GetPagedListAsync(Expression<Func<T, bool>> expression, int pageIndex, int pageSize)
    {
        RefAsync<int> totalCount = 0;
        var items = await _db.Queryable<T>()
            .Where(expression)
            .ToPageListAsync(pageIndex, pageSize, totalCount);
        return (items, totalCount.Value);
    }

    /// <summary>
    /// 插入实体
    /// </summary>
    /// <param name="entity">实体</param>
    /// <returns>插入的行数</returns>
    public async Task<int> InsertAsync(T entity)
    {
        return await _db.Insertable(entity).ExecuteCommandAsync();
    }

    /// <summary>
    /// 批量插入实体
    /// </summary>
    /// <param name="entities">实体列表</param>
    /// <returns>插入的行数</returns>
    public async Task<int> InsertAsync(List<T> entities)
    {
        return await _db.Insertable(entities).ExecuteCommandAsync();
    }

    /// <summary>
    /// 更新实体
    /// </summary>
    /// <param name="entity">实体</param>
    /// <returns>更新的行数</returns>
    public async Task<int> UpdateAsync(T entity)
    {
        return await _db.Updateable(entity).ExecuteCommandAsync();
    }

    /// <summary>
    /// 批量更新实体
    /// </summary>
    /// <param name="entities">实体列表</param>
    /// <returns>更新的行数</returns>
    public async Task<int> UpdateAsync(List<T> entities)
    {
        return await _db.Updateable(entities).ExecuteCommandAsync();
    }

    /// <summary>
    /// 根据条件更新实体
    /// </summary>
    /// <param name="expression">更新条件</param>
    /// <param name="entity">更新内容</param>
    /// <returns>更新的行数</returns>
    public async Task<int> UpdateAsync(Expression<Func<T, bool>> expression, T entity)
    {
        return await _db.Updateable(entity).Where(expression).ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除实体
    /// </summary>
    /// <param name="entity">实体</param>
    /// <returns>删除的行数</returns>
    public async Task<int> DeleteAsync(T entity)
    {
        return await _db.Deleteable(entity).ExecuteCommandAsync();
    }

    /// <summary>
    /// 根据 ID 删除实体
    /// </summary>
    /// <param name="id">主键 ID</param>
    /// <returns>删除的行数</returns>
    public async Task<int> DeleteAsync(TKey id)
    {
        return await _db.Deleteable<T>().In(id).ExecuteCommandAsync();
    }

    /// <summary>
    /// 批量删除实体
    /// </summary>
    /// <param name="entities">实体列表</param>
    /// <returns>删除的行数</returns>
    public async Task<int> DeleteAsync(List<T> entities)
    {
        return await _db.Deleteable(entities).ExecuteCommandAsync();
    }

    /// <summary>
    /// 根据条件删除实体
    /// </summary>
    /// <param name="expression">删除条件</param>
    /// <returns>删除的行数</returns>
    public async Task<int> DeleteAsync(Expression<Func<T, bool>> expression)
    {
        return await _db.Deleteable<T>().Where(expression).ExecuteCommandAsync();
    }

    /// <summary>
    /// 软删除实体
    /// </summary>
    /// <param name="entity">实体</param>
    /// <returns>删除的行数</returns>
    public async Task<int> SoftDeleteAsync(T entity)
    {
        return await _db.Updateable(entity).ExecuteCommandAsync();
    }

    /// <summary>
    /// 根据 ID 软删除实体
    /// </summary>
    /// <param name="id">主键 ID</param>
    /// <returns>删除的行数</returns>
    public async Task<int> SoftDeleteAsync(TKey id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
        {
            return 0;
        }
        return await SoftDeleteAsync(entity);
    }

    /// <summary>
    /// 批量软删除实体
    /// </summary>
    /// <param name="entities">实体列表</param>
    /// <returns>删除的行数</returns>
    public async Task<int> SoftDeleteAsync(List<T> entities)
    {
        return await _db.Updateable(entities).ExecuteCommandAsync();
    }

    /// <summary>
    /// 根据条件软删除实体
    /// </summary>
    /// <param name="expression">删除条件</param>
    /// <returns>删除的行数</returns>
    public async Task<int> SoftDeleteAsync(Expression<Func<T, bool>> expression)
    {
        var entities = await GetListAsync(expression);
        return await SoftDeleteAsync(entities);
    }

    /// <summary>
    /// 获取可查询对象
    /// </summary>
    /// <returns>可查询对象</returns>
    public ISugarQueryable<T> GetQueryable()
    {
        return _db.Queryable<T>();
    }

    /// <summary>
    /// 获取可查询对象（带条件）
    /// </summary>
    /// <param name="expression">查询条件</param>
    /// <returns>可查询对象</returns>
    public ISugarQueryable<T> GetQueryable(Expression<Func<T, bool>> expression)
    {
        return _db.Queryable<T>().Where(expression);
    }

    /// <summary>
    /// 统计数量
    /// </summary>
    /// <returns>数量</returns>
    public async Task<int> CountAsync()
    {
        return await _db.Queryable<T>().CountAsync();
    }

    /// <summary>
    /// 根据条件统计数量
    /// </summary>
    /// <param name="expression">查询条件</param>
    /// <returns>数量</returns>
    public async Task<int> CountAsync(Expression<Func<T, bool>> expression)
    {
        return await _db.Queryable<T>().Where(expression).CountAsync();
    }

    /// <summary>
    /// 判断是否存在
    /// </summary>
    /// <param name="expression">查询条件</param>
    /// <returns>是否存在</returns>
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> expression)
    {
        return await _db.Queryable<T>().AnyAsync(expression);
    }

    /// <summary>
    /// 开始事务
    /// </summary>
    public void BeginTran()
    {
        _db.Ado.BeginTran();
    }

    /// <summary>
    /// 提交事务
    /// </summary>
    public void CommitTran()
    {
        _db.Ado.CommitTran();
    }

    /// <summary>
    /// 回滚事务
    /// </summary>
    public void RollbackTran()
    {
        _db.Ado.RollbackTran();
    }
}
