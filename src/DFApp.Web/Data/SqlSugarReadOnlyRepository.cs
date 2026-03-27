using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SqlSugar;

namespace DFApp.Web.Data;

/// <summary>
/// SqlSugar 只读仓储实现
/// </summary>
/// <typeparam name="T">实体类型</typeparam>
/// <typeparam name="TKey">主键类型</typeparam>
public class SqlSugarReadOnlyRepository<T, TKey> : ISqlSugarReadOnlyRepository<T, TKey> where T : class, new()
{
    private readonly ISqlSugarClient _db;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db">SqlSugar 客户端</param>
    public SqlSugarReadOnlyRepository(ISqlSugarClient db)
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
    /// 分页查询（带排序）
    /// </summary>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="orderByExpression">排序表达式</param>
    /// <param name="orderByType">排序类型（升序或降序）</param>
    /// <returns>分页结果</returns>
    public async Task<(List<T> Items, int TotalCount)> GetPagedListAsync(int pageIndex, int pageSize, Expression<Func<T, object>> orderByExpression, OrderByType orderByType = OrderByType.Asc)
    {
        RefAsync<int> totalCount = 0;
        var query = _db.Queryable<T>();
        if (orderByType == OrderByType.Asc)
        {
            query = query.OrderBy(orderByExpression, OrderByType.Asc);
        }
        else
        {
            query = query.OrderBy(orderByExpression, OrderByType.Desc);
        }
        var items = await query.ToPageListAsync(pageIndex, pageSize, totalCount);
        return (items, totalCount.Value);
    }

    /// <summary>
    /// 根据条件分页查询（带排序）
    /// </summary>
    /// <param name="expression">查询条件</param>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="orderByExpression">排序表达式</param>
    /// <param name="orderByType">排序类型（升序或降序）</param>
    /// <returns>分页结果</returns>
    public async Task<(List<T> Items, int TotalCount)> GetPagedListAsync(Expression<Func<T, bool>> expression, int pageIndex, int pageSize, Expression<Func<T, object>> orderByExpression, OrderByType orderByType = OrderByType.Asc)
    {
        RefAsync<int> totalCount = 0;
        var query = _db.Queryable<T>().Where(expression);
        if (orderByType == OrderByType.Asc)
        {
            query = query.OrderBy(orderByExpression, OrderByType.Asc);
        }
        else
        {
            query = query.OrderBy(orderByExpression, OrderByType.Desc);
        }
        var items = await query.ToPageListAsync(pageIndex, pageSize, totalCount);
        return (items, totalCount.Value);
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
}
