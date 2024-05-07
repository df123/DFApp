using DFApp.Aria2;
using DFApp.Aria2.Notifications;
using DFApp.Aria2.Request;
using DFApp.Aria2.Response;
using DFApp.Aria2.Response.TellStatus;
using DFApp.Configuration;
using DFApp.Queue;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.ObjectMapping;

namespace DFApp.Background
{
    public class Aria2BackgroundWorker : DFAppBackgroundWorkerBase
    {
        private readonly ClientWebSocket _clientWebSocket;
        private readonly Aria2Manager _manager;
        private readonly IObjectMapper _mapper;
        private readonly IQueueBase<List<Aria2RequestDto>> _queueBase;
        private readonly List<Aria2ResponseDto> _responseDtos;

        public Aria2BackgroundWorker(IConfigurationInfoRepository configurationInfoRepository
            , Aria2Manager manager
            , IObjectMapper objectMapper)
            : base("DFApp.Background.Aria2BackgroundWorker", configurationInfoRepository)
        {
            _clientWebSocket = new ClientWebSocket();
            _manager = manager;
            _mapper = objectMapper;
            _queueBase = new QueueBase<List<Aria2RequestDto>>();
            _responseDtos = new List<Aria2ResponseDto>();
        }

        public override Task StartAsync(CancellationToken cancellationToken = default)
        {
            _executeTask = StartWork();

            if (_executeTask.IsCompleted)
            {
                return _executeTask;
            }

            return base.StartAsync(cancellationToken);
        }

        public async Task StartWork()
        {
            string aria2ws = await GetConfigurationInfo("aria2ws");
            if (string.IsNullOrWhiteSpace(aria2ws))
            {
                throw new Exception("error");
            }
            await _clientWebSocket.ConnectAsync(new Uri(aria2ws), StoppingToken);
            Logger.LogInformation("Aria2BackgroundWorker:Connect");
            var receiveTask = Task.Run(async () =>
            {
                var buffer = new byte[1024 * 4];
                while (!StoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        Logger.LogInformation($"Aria2BackgroundWorker:Connect:while:{_clientWebSocket.State}");
                        var result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), StoppingToken);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            break;
                        }
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
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
                while (!StoppingToken.IsCancellationRequested)
                {
                    var data = await _queueBase.GetItemAsync(StoppingToken);
                    try
                    {
                        foreach (var item in data!)
                        {
                            string dto = JsonSerializer.Serialize(item);
                            var buffer = Encoding.UTF8.GetBytes(dto);
                            await _clientWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text
                                , true, StoppingToken);
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


    }
}
