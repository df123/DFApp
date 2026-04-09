using System;
using System.Linq;
using System.Reflection;
using DFApp.Web.Domain;
using DFApp.Web.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace DFApp.Web.Data;

/// <summary>
/// SqlSugar 配置类，提供数据库连接和自动化功能
/// </summary>
public class SqlSugarConfig
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="httpContextAccessor">HTTP 上下文访问器，用于从请求作用域获取服务</param>
    /// <param name="configuration">配置</param>
    public SqlSugarConfig(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    /// <summary>
    /// 创建并配置 SqlSugar 客户端
    /// </summary>
    /// <returns>配置好的 SqlSugar 客户端</returns>
    public ISqlSugarClient CreateClient()
    {
        var connectionString = _configuration.GetConnectionString("Default");
        var db = new SqlSugarClient(new ConnectionConfig()
        {
            ConnectionString = connectionString,
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
        });

        // 配置 AOP 自动填充审计字段
        ConfigureAop(db);

        // 配置全局软删除过滤器
        ConfigureSoftDeleteFilter(db);

        return db;
    }

    /// <summary>
    /// 配置 AOP 自动填充审计字段
    /// </summary>
    /// <param name="db">SqlSugar 客户端</param>
    private void ConfigureAop(ISqlSugarClient db)
    {
        db.Aop.DataExecuting = (oldValue, entityInfo) =>
        {
            // 插入操作
            if (entityInfo.OperationType == DataFilterType.InsertByObject)
            {
                // 设置创建时间
                if (entityInfo.PropertyName == nameof(IHasCreationTime.CreationTime) && entityInfo.EntityValue is IHasCreationTime creationTimeEntity)
                {
                    if (creationTimeEntity.CreationTime == default)
                    {
                        creationTimeEntity.CreationTime = DateTime.Now;
                    }
                }

                // 设置最后修改时间
                if (entityInfo.PropertyName == nameof(IHasModificationTime.LastModificationTime) && entityInfo.EntityValue is IHasModificationTime modificationTimeEntity)
                {
                    modificationTimeEntity.LastModificationTime = DateTime.Now;
                }

                // 设置创建者 ID
                if (entityInfo.PropertyName == nameof(ICreatorId.CreatorId) && entityInfo.EntityValue is ICreatorId creatorIdEntity)
                {
                    if (creatorIdEntity.CreatorId == null)
                    {
                        var currentUser = GetCurrentUser();
                        if (currentUser != null && currentUser.Id.HasValue)
                        {
                            creatorIdEntity.CreatorId = currentUser.Id.Value;
                        }
                    }
                }

                // 设置最后修改者 ID
                if (entityInfo.PropertyName == nameof(IModifierId.LastModifierId) && entityInfo.EntityValue is IModifierId modifierIdEntity)
                {
                    var currentUser = GetCurrentUser();
                    if (currentUser != null && currentUser.Id.HasValue)
                    {
                        modifierIdEntity.LastModifierId = currentUser.Id.Value;
                    }
                }

                // 设置并发标记
                if (entityInfo.PropertyName == "ConcurrencyStamp" && entityInfo.EntityValue != null)
                {
                    var property = entityInfo.EntityValue.GetType().GetProperty("ConcurrencyStamp");
                    if (property != null && property.GetValue(entityInfo.EntityValue) == null)
                    {
                        property.SetValue(entityInfo.EntityValue, Guid.NewGuid().ToString());
                    }
                }
            }

            // 更新操作
            if (entityInfo.OperationType == DataFilterType.UpdateByObject)
            {
                // 设置最后修改时间
                if (entityInfo.PropertyName == nameof(IHasModificationTime.LastModificationTime) && entityInfo.EntityValue is IHasModificationTime modificationTimeEntity)
                {
                    modificationTimeEntity.LastModificationTime = DateTime.Now;
                }

                // 设置最后修改者 ID
                if (entityInfo.PropertyName == nameof(IModifierId.LastModifierId) && entityInfo.EntityValue is IModifierId modifierIdEntity)
                {
                    var currentUser = GetCurrentUser();
                    if (currentUser != null && currentUser.Id.HasValue)
                    {
                        modifierIdEntity.LastModifierId = currentUser.Id.Value;
                    }
                }

                // 更新并发标记
                if (entityInfo.PropertyName == "ConcurrencyStamp" && entityInfo.EntityValue != null)
                {
                    var property = entityInfo.EntityValue.GetType().GetProperty("ConcurrencyStamp");
                    if (property != null)
                    {
                        property.SetValue(entityInfo.EntityValue, Guid.NewGuid().ToString());
                    }
                }
            }

            // 删除操作
            if (entityInfo.OperationType == DataFilterType.DeleteByObject)
            {
                // 设置删除时间
                if (entityInfo.PropertyName == nameof(IHasDeletionTime.DeletionTime) && entityInfo.EntityValue is IHasDeletionTime deletionTimeEntity)
                {
                    if (deletionTimeEntity.DeletionTime == null)
                    {
                        deletionTimeEntity.DeletionTime = DateTime.Now;
                    }
                }

                // 设置删除者 ID
                if (entityInfo.PropertyName == nameof(IDeleterId.DeleterId) && entityInfo.EntityValue is IDeleterId deleterIdEntity)
                {
                    if (deleterIdEntity.DeleterId == null)
                    {
                        var currentUser = GetCurrentUser();
                        if (currentUser != null && currentUser.Id.HasValue)
                        {
                            deleterIdEntity.DeleterId = currentUser.Id.Value;
                        }
                    }
                }

                // 设置软删除标记
                if (entityInfo.PropertyName == nameof(ISoftDelete.IsDeleted) && entityInfo.EntityValue is ISoftDelete softDeleteEntity)
                {
                    softDeleteEntity.IsDeleted = true;
                }
            }
        };
    }

    /// <summary>
    /// 配置全局软删除过滤器
    /// </summary>
    /// <param name="db">SqlSugar 客户端</param>
    private void ConfigureSoftDeleteFilter(ISqlSugarClient db)
    {
        // 软删除功能已废除，不再配置软删除过滤器
        return;
        // db.QueryFilter.Add(new TableFilterItem<ISoftDelete>(it => it.IsDeleted == false));
    }

    /// <summary>
    /// 从当前 HTTP 请求的 scoped 服务容器中获取当前用户信息
    /// 后台任务场景下 HttpContext 为 null，返回 null
    /// </summary>
    private ICurrentUser? GetCurrentUser()
    {
        return _httpContextAccessor.HttpContext?.RequestServices.GetService<ICurrentUser>();
    }
}
