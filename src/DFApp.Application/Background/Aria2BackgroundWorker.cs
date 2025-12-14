using DFApp.Aria2;
using DFApp.Aria2.Notifications;
using DFApp.Aria2.Request;
using DFApp.Aria2.Response;
using DFApp.Aria2.Response.TellStatus;
using DFApp.Configuration;
using DFApp.Queue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace DFApp.Background
{
    public class Aria2BackgroundWorker : BackgroundService
    {
        private readonly ClientWebSocket _clientWebSocket;
        private readonly Aria2Manager _manager;
        private readonly IObjectMapper _mapper;
        private readonly IQueueManagement _queueManagement;
        private readonly IQueueBase<List<Aria2RequestDto>> _queueBase;
        private readonly List<Aria2ResponseDto> _responseDtos;

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IServiceScope _serviceScope;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfigurationInfoRepository _configurationInfoRepository;

        private ILogger Logger => _loggerFactory.CreateLogger(GetType().FullName!) ?? NullLogger.Instance;

        public Aria2BackgroundWorker(IServiceScopeFactory serviceScopeFactory
            , Aria2Manager manager
            , IObjectMapper objectMapper
            , IQueueManagement queueManagement)
        {
            _serviceScopeFactory = serviceScopeFactory;

            _serviceScope = _serviceScopeFactory.CreateScope();
            _loggerFactory = _serviceScope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            _configurationInfoRepository = _serviceScope.ServiceProvider.GetRequiredService<IConfigurationInfoRepository>();

            _clientWebSocket = new ClientWebSocket();
            _manager = manager;
            _mapper = objectMapper;
            _queueManagement = queueManagement;
            _queueBase = _queueManagement.GetQueue<List<Aria2RequestDto>>("Aria2RequestQueue");
            _responseDtos = new List<Aria2ResponseDto>();
        }

        public async Task<string> GetConfigurationInfo(string configurationName)
        {
            using (_configurationInfoRepository.DisableTracking())
            {
                string v = await _configurationInfoRepository.GetConfigurationInfoValue(configurationName, "DFApp.Background.Aria2BackgroundWorker");
                return v;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await StartWork(stoppingToken).ConfigureAwait(false);
        }

        public async Task StartWork(CancellationToken stoppingToken)
        {
            try
            {
                string aria2ws = await GetConfigurationInfo("aria2ws");
                if (string.IsNullOrWhiteSpace(aria2ws))
                {
                    throw new UserFriendlyException("aria2 ws连接不存在");
                }
                await _clientWebSocket.ConnectAsync(new Uri(aria2ws), stoppingToken);
                var receiveTask = Task.Run(async () =>
                {
                    var buffer = new byte[1024 * 1024 * 10];
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            if (_clientWebSocket.State != WebSocketState.Open)
                            {
                                return;
                            }
                            var result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);
                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                break;
                            }
                            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            if (message.Contains("\"error\":"))
                            {
                                Logger.LogDebug($"Aria2BackgroundWorker:Connect:message:{message}");
                            }
                            else
                            {
                                Logger.LogInformation($"Aria2BackgroundWorker:Connect:message:{message}");
                            }
                            ResponseBase? dto;
                            if (message.Contains("\"id\":") && (!message.Contains("\"error\":")))
                            {
                                var data = JsonSerializer.Deserialize<TellStatusResponseDto>(message);
                                dto = _mapper.Map<TellStatusResponseDto?, TellStatusResponse?>(data);
                            }
                            else if (message.Contains("method"))
                            {
                                dto = _mapper.Map<Aria2NotificationDto?, Aria2Notification?>(JsonSerializer.Deserialize<Aria2NotificationDto>(message));
                            }
                            else
                            {
                                continue;
                            }

                            if (dto != null)
                            {
                                var data = _manager.ProcessResponse(dto);

                                if (data != null)
                                {
                                    _queueBase.AddItem(_mapper.Map<List<Aria2Request>, List<Aria2RequestDto>>(data));
                                    foreach (var item in data)
                                    {
                                        _responseDtos.Add(new Aria2ResponseDto()
                                        {
                                            Id = item.Id
                                        });
                                    }

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"Aria2BackgroundWorker:receiveTask:{ex.Message}");
                        }

                    }
                });

                var sendTask = Task.Run(async () =>
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var data = await _queueBase.GetItemAsync(stoppingToken);
                        try
                        {
                            foreach (var item in data!)
                            {
                                string dto = JsonSerializer.Serialize(item);
                                var buffer = Encoding.UTF8.GetBytes(dto);
                                if (_clientWebSocket.State != WebSocketState.Open)
                                {
                                    return;
                                }
                                await _clientWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text
                                    , true, stoppingToken);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"Aria2BackgroundWorker:sendTask:{ex.Message}");
                        }

                    }
                });

                await receiveTask;
                await sendTask;
            }
            catch (Exception ex)
            {
                Logger.LogError("Aria2BackgroundWorker出现异常停止运行");
                Logger.LogError($"Aria2BackgroundWorker出现异常停止运行:{ex.Message}");
            }
        }

        public override void Dispose()
        {
            _serviceScope.Dispose();
            base.Dispose();
        }


    }
}
