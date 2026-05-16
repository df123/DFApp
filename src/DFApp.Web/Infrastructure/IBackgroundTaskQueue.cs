using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DFApp.Web.Queue;

/// <summary>
/// 后台任务队列接口，用于将任务加入后台执行
/// </summary>
public interface IBackgroundTaskQueue
{
    /// <summary>
    /// 将任务加入队列
    /// </summary>
    /// <param name="workItem">后台任务委托</param>
    void EnqueueTask(Func<IServiceScopeFactory, CancellationToken, Task> workItem);
}

/// <summary>
/// 后台任务队列实现
/// </summary>
public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<IServiceScopeFactory, CancellationToken, Task>> _channel;

    public BackgroundTaskQueue()
    {
        // 使用无界通道，避免任务入队时阻塞
        _channel = Channel.CreateUnbounded<Func<IServiceScopeFactory, CancellationToken, Task>>();
    }

    /// <summary>
    /// 将任务加入队列
    /// </summary>
    public async void EnqueueTask(Func<IServiceScopeFactory, CancellationToken, Task> workItem)
    {
        if (workItem == null)
        {
            throw new ArgumentNullException(nameof(workItem));
        }

        await _channel.Writer.WriteAsync(workItem);
    }

    /// <summary>
    /// 获取队列读取器，供后台服务消费
    /// </summary>
    public IAsyncEnumerable<Func<IServiceScopeFactory, CancellationToken, Task>> ReadAllAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
