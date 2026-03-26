using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SqlSugar;

namespace DFApp.Web.Data;

/// <summary>
/// SqlSugar 仓储接口
/// </summary>
/// <typeparam name="T">实体类型</typeparam>
/// <typeparam name="TKey">主键类型</typeparam>
public interface ISqlSugarRepository<T, TKey> where T : class, new()
{
    /// <summary>
    /// 根据 ID 获取实体
    /// </summary>
    /// <param name="id">主键 ID</param>
    /// <returns>实体</returns>
    Task<T?> GetByIdAsync(TKey id);

    /// <summary>
    /// 根据条件获取单个实体
    /// </summary>
    /// <param name="expression">查询条件</param>
    /// <returns>实体</returns>
    Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> expression);

    /// <summary>
    /// 获取所有实体列表
    /// </summary>
    /// <returns>实体列表</returns>
    Task<List<T>> GetListAsync();

    /// <summary>
    /// 根据条件获取实体列表
    /// </summary>
    /// <param name="expression">查询条件</param>
    /// <returns>实体列表</returns>
    Task<List<T>> GetListAsync(Expression<Func<T, bool>> expression);

    /// <summary>
    /// 分页查询
    /// </summary>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>分页结果</returns>
    Task<(List<T> Items, int TotalCount)> GetPagedListAsync(int pageIndex, int pageSize);

    /// <summary>
    /// 根据条件分页查询
    /// </summary>
    /// <param name="expression">查询条件</param>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>分页结果</returns>
    Task<(List<T> Items, int TotalCount)> GetPagedListAsync(Expression<Func<T, bool>> expression, int pageIndex, int pageSize);

    /// <summary>
    /// 插入实体
    /// </summary>
    /// <param name="entity">实体</param>
    /// <returns>插入的行数</returns>
    Task<int> InsertAsync(T entity);

    /// <summary>
    /// 批量插入实体
    /// </summary>
    /// <param name="entities">实体列表</param>
    /// <returns>插入的行数</returns>
    Task<int> InsertAsync(List<T> entities);

    /// <summary>
    /// 更新实体
    /// </summary>
    /// <param name="entity">实体</param>
    /// <returns>更新的行数</returns>
    Task<int> UpdateAsync(T entity);

    /// <summary>
    /// 批量更新实体
    /// </summary>
    /// <param name="entities">实体列表</param>
    /// <returns>更新的行数</returns>
    Task<int> UpdateAsync(List<T> entities);

    /// <summary>
    /// 根据条件更新实体
    /// </summary>
    /// <param name="expression">更新条件</param>
    /// <param name="entity">更新内容</param>
    /// <returns>更新的行数</returns>
    Task<int> UpdateAsync(Expression<Func<T, bool>> expression, T entity);

    /// <summary>
    /// 删除实体
    /// </summary>
    /// <param name="entity">实体</param>
    /// <returns>删除的行数</returns>
    Task<int> DeleteAsync(T entity);

    /// <summary>
    /// 根据 ID 删除实体
    /// </summary>
    /// <param name="id">主键 ID</param>
    /// <returns>删除的行数</returns>
    Task<int> DeleteAsync(TKey id);

    /// <summary>
    /// 批量删除实体
    /// </summary>
    /// <param name="entities">实体列表</param>
    /// <returns>删除的行数</returns>
    Task<int> DeleteAsync(List<T> entities);

    /// <summary>
    /// 根据条件删除实体
    /// </summary>
    /// <param name="expression">删除条件</param>
    /// <returns>删除的行数</returns>
    Task<int> DeleteAsync(Expression<Func<T, bool>> expression);

    /// <summary>
    /// 软删除实体
    /// </summary>
    /// <param name="entity">实体</param>
    /// <returns>删除的行数</returns>
    Task<int> SoftDeleteAsync(T entity);

    /// <summary>
    /// 根据 ID 软删除实体
    /// </summary>
    /// <param name="id">主键 ID</param>
    /// <returns>删除的行数</returns>
    Task<int> SoftDeleteAsync(TKey id);

    /// <summary>
    /// 批量软删除实体
    /// </summary>
    /// <param name="entities">实体列表</param>
    /// <returns>删除的行数</returns>
    Task<int> SoftDeleteAsync(List<T> entities);

    /// <summary>
    /// 根据条件软删除实体
    /// </summary>
    /// <param name="expression">删除条件</param>
    /// <returns>删除的行数</returns>
    Task<int> SoftDeleteAsync(Expression<Func<T, bool>> expression);

    /// <summary>
    /// 获取可查询对象
    /// </summary>
    /// <returns>可查询对象</returns>
    ISugarQueryable<T> GetQueryable();

    /// <summary>
    /// 获取可查询对象（带条件）
    /// </summary>
    /// <param name="expression">查询条件</param>
    /// <returns>可查询对象</returns>
    ISugarQueryable<T> GetQueryable(Expression<Func<T, bool>> expression);

    /// <summary>
    /// 统计数量
    /// </summary>
    /// <returns>数量</returns>
    Task<int> CountAsync();

    /// <summary>
    /// 根据条件统计数量
    /// </summary>
    /// <param name="expression">查询条件</param>
    /// <returns>数量</returns>
    Task<int> CountAsync(Expression<Func<T, bool>> expression);

    /// <summary>
    /// 判断是否存在
    /// </summary>
    /// <param name="expression">查询条件</param>
    /// <returns>是否存在</returns>
    Task<bool> AnyAsync(Expression<Func<T, bool>> expression);

    /// <summary>
    /// 开始事务
    /// </summary>
    void BeginTran();

    /// <summary>
    /// 提交事务
    /// </summary>
    void CommitTran();

    /// <summary>
    /// 回滚事务
    /// </summary>
    void RollbackTran();
}
