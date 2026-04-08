using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DFApp.Web.Queue;

/// <summary>
/// 队列管理接口，用于添加和处理队列任务
/// </summary>
public interface IQueueManagement
{
    /// <summary>
    /// 添加值到指定队列
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="queueName">队列名称</param>
    /// <param name="value">要添加的值</param>
    void AddQueueValue<T>(string queueName, List<T> value);

    /// <summary>
    /// 从指定队列获取值
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="queueName">队列名称</param>
    /// <returns>队列中的值列表</returns>
    List<T>? GetQueueValue<T>(string queueName);

    /// <summary>
    /// 清除指定队列
    /// </summary>
    /// <param name="queueName">队列名称</param>
    void ClearQueue(string queueName);
}

/// <summary>
/// 后台任务队列消费服务，用于执行通过 IBackgroundTaskQueue 入队的后台任务
/// </summary>
public class BackgroundQueueHostedService : BackgroundService
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<BackgroundQueueHostedService> _logger;

    public BackgroundQueueHostedService(
        IBackgroundTaskQueue taskQueue,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<BackgroundQueueHostedService> logger)
    {
        _taskQueue = taskQueue;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("后台任务队列消费服务启动");

        // 逐个消费队列中的任务
        await foreach (var task in ((BackgroundTaskQueue)_taskQueue).ReadAllAsync(stoppingToken))
        {
            try
            {
                await task(_serviceScopeFactory, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // 正常停止，不记录错误
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "后台任务执行出错");
            }
        }

        _logger.LogInformation("后台任务队列消费服务停止");
    }
}

/// <summary>
/// 队列管理实现
/// </summary>
public class QueueManagement : IQueueManagement
{
    private readonly Dictionary<string, object> _queues = new();

    public void AddQueueValue<T>(string queueName, List<T> value)
    {
        lock (_queues)
        {
            if (_queues.TryGetValue(queueName, out var existing))
            {
                if (existing is List<T> existingList)
                {
                    existingList.AddRange(value);
                }
            }
            else
            {
                _queues[queueName] = value;
            }
        }
    }

    public List<T>? GetQueueValue<T>(string queueName)
    {
        lock (_queues)
        {
            if (_queues.TryGetValue(queueName, out var existing) && existing is List<T> list)
            {
                return list;
            }
            return null;
        }
    }

    public void ClearQueue(string queueName)
    {
        lock (_queues)
        {
            _queues.Remove(queueName);
        }
    }
}
