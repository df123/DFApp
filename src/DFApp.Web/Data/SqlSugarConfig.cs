using System;
using System.Linq;
using System.Reflection;
using DFApp.Web.Domain;
using DFApp.Web.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace DFApp.Web.Data;

/// <summary>
/// SqlSugar 配置类，提供数据库连接和自动化功能
/// </summary>
public class SqlSugarConfig
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="serviceProvider">服务提供程序</param>
    /// <param name="configuration">配置</param>
    public SqlSugarConfig(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
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

        // 配置 CreatorId 数据过滤器
        ConfigureCreatorIdFilter(db);

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
                        var currentUser = _serviceProvider.GetService<ICurrentUser>();
                        if (currentUser != null && currentUser.Id.HasValue)
                        {
                            creatorIdEntity.CreatorId = currentUser.Id.Value;
                        }
                    }
                }

                // 设置最后修改者 ID
                if (entityInfo.PropertyName == nameof(IModifierId.LastModifierId) && entityInfo.EntityValue is IModifierId modifierIdEntity)
                {
                    var currentUser = _serviceProvider.GetService<ICurrentUser>();
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
                    var currentUser = _serviceProvider.GetService<ICurrentUser>();
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
                        var currentUser = _serviceProvider.GetService<ICurrentUser>();
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
    /// 配置 CreatorId 数据过滤器
    /// </summary>
    /// <param name="db">SqlSugar 客户端</param>
    private void ConfigureCreatorIdFilter(ISqlSugarClient db)
    {
        var currentUser = _serviceProvider.GetService<ICurrentUser>();
        if (currentUser != null && currentUser.Id.HasValue)
        {
            db.QueryFilter.Add(new TableFilterItem<ICreatorId>(it => it.CreatorId == currentUser.Id.Value));
        }
    }
}
