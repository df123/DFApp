using DFApp.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace DFApp.Background
{
    public class DFAppBackgroundWorkerBase : IDFAppBackgroundWorkerBase
    {
        public IAbpLazyServiceProvider LazyServiceProvider { get; set; } = default!;

        public IServiceProvider ServiceProvider { get; set; } = default!;

        protected ILoggerFactory LoggerFactory => LazyServiceProvider.LazyGetRequiredService<ILoggerFactory>();

        protected ILogger Logger => LazyServiceProvider.LazyGetService<ILogger>(provider => LoggerFactory?.CreateLogger(GetType().FullName!) ?? NullLogger.Instance);

        protected CancellationTokenSource StoppingTokenSource { get; private set; }
        protected CancellationToken StoppingToken { get; private set; }

        protected Task? _executeTask;

        public Task? ExecuteTask { get { return _executeTask; } }

        protected readonly string _moduleName;

        public string ModuleName => _moduleName;

        protected bool _hasError;
        public bool HasError => _hasError;

        public DateTime StartTime { get; private set; }
        public DateTime RestartTime { get; private set; }
        public int RestartCount { get; private set; }
        public int ErrorCount { get; protected set; }
        public string ErrorDescription { get; protected set; }

        protected readonly IConfigurationInfoRepository _configurationInfoRepository;

        public DFAppBackgroundWorkerBase(string moduleName
            , IConfigurationInfoRepository configurationInfoRepository)
        {
            _executeTask = null;
            _moduleName = moduleName;
            _configurationInfoRepository = configurationInfoRepository;
            StartTime = DateTime.Now;
            _hasError = false;
            RestartCount = 0;
            ErrorCount = 0;
            ErrorDescription = string.Empty;
            StoppingTokenSource = new CancellationTokenSource();
            StoppingToken = StoppingTokenSource.Token;
        }

        public virtual async Task<string> GetConfigurationInfo(string configurationName)
        {
            string v = await _configurationInfoRepository.GetConfigurationInfoValue(configurationName, _moduleName);
            return v;
        }

        public virtual Task StartAsync(CancellationToken cancellationToken = default)
        {
            _hasError = false;
            Logger.LogDebug("Started background worker: " + ToString());
            return Task.CompletedTask;
        }

        public virtual async Task RestartAsync(CancellationToken cancellationToken = default)
        {
            await StopAsync();
            StoppingTokenSource = new CancellationTokenSource();
            StoppingToken = StoppingTokenSource.Token;
            _hasError = false;
            ErrorDescription = string.Empty;
            RestartTime = DateTime.Now;
            RestartCount++;

        }

        public virtual Task StopAsync(CancellationToken cancellationToken = default)
        {
            Logger.LogDebug("Stopped background worker: " + ToString());
            StoppingTokenSource.Cancel();
            StoppingTokenSource.Dispose();
            return Task.CompletedTask;
        }
    }
}
