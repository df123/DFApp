using DFApp.Aria2.Notifications;
using DFApp.Aria2.Repository.Response.TellStatus;
using DFApp.Aria2.Request;
using DFApp.Aria2.Response.TellStatus;
using DFApp.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace DFApp.Aria2
{
    public class Aria2Manager : DomainService
    {
        private readonly ITellStatusResultRepository _resultRepository;
        private readonly List<Aria2Request> _requestsHistory;
        private readonly IConfigurationInfoRepository _configurationInfoRepository;

        public Aria2Manager(ITellStatusResultRepository tellStatusResultRepository
            ,IConfigurationInfoRepository configurationInfoRepository)
        {
            _requestsHistory = new List<Aria2Request>();
            _resultRepository = tellStatusResultRepository;
            _configurationInfoRepository = configurationInfoRepository;
        }

        public async Task<List<Aria2Request>> ProcessResponseAsync(ResponseBase? response)
        {
            if (response == null)
            {
                return new List<Aria2Request>();
            }

            if (response.Id == null)
            {
                return await ProcessNotificationAsync(response as Aria2Notification);
            }
            else
            {
                var res = _requestsHistory.FirstOrDefault(x => x.Id == response.Id);
                if (res != null)
                {
                    switch (res.Method)
                    {
                        case Aria2Consts.TellStatus:
                            TellStatusResponse? tellStatusResponse = response as TellStatusResponse;
                            if (tellStatusResponse != null && tellStatusResponse.Result != null)
                            {
                                var result = tellStatusResponse.Result;

                                // Log detailed information for debugging
                                Logger.LogInformation($"=== Saving TellStatusResult to Database ===");
                                Logger.LogInformation($"GID: {result.GID}");
                                Logger.LogInformation($"Dir: {result.Dir}");
                                Logger.LogInformation($"Status: {result.Status}");
                                Logger.LogInformation($"TotalLength: {result.TotalLength}");
                                Logger.LogInformation($"CompletedLength: {result.CompletedLength}");
                                Logger.LogInformation($"Files count: {result.Files?.Count ?? 0}");

                                if (result.Files != null && result.Files.Count > 0)
                                {
                                    foreach (var file in result.Files)
                                    {
                                        Logger.LogInformation($"  File[{file.Index}]: {file.Path}, Length: {file.Length}, Completed: {file.CompletedLength}");
                                        if (file.Uris != null)
                                        {
                                            Logger.LogInformation($"    Uris count: {file.Uris.Count}");
                                        }
                                    }
                                }

                                await _resultRepository.InsertAsync(result);
                                Logger.LogInformation($"=== Successfully saved TellStatusResult ===");
                            }
                            _requestsHistory.Remove(res);
                            break;
                        default:
                            break;
                    }
                }
            }
            return new List<Aria2Request>();
        }

        public async Task<List<Aria2Request>> ProcessNotificationAsync(Aria2Notification? notification)
        {
            if (notification == null)
            {
                return new List<Aria2Request>();
            }

            switch (notification.Method)
            {
                case Aria2Consts.OnDownloadStart:
                    Logger.LogInformation("OnDownloadStart");
                    break;
                case Aria2Consts.OnDownloadPause:
                    Logger.LogInformation("OnDownloadPause");
                    break;
                case Aria2Consts.OnDownloadStop:
                    Logger.LogInformation("OnDownloadStop");
                    break;
                case Aria2Consts.OnDownloadError:
                    Logger.LogInformation("OnDownloadError");
                    break;
                case Aria2Consts.OnDownloadComplete:
                case Aria2Consts.OnBtDownloadComplete:
                    return await DownloadCompleteHandlerAsync(notification.Params);
                default:
                    Logger.LogInformation("default");
                    break;
            }
            return new List<Aria2Request>();
        }


        public async Task<List<Aria2Request>> DownloadCompleteHandlerAsync(List<ParamsItem> paramsItems)
        {
            List<Aria2Request> requests = new List<Aria2Request>();
            string aria2secret = await _configurationInfoRepository.GetConfigurationInfoValue("aria2secret", "DFApp.Aria2.Aria2Service");
            foreach (var item in paramsItems)
            {
                var request = new Aria2Request(Guid.NewGuid().ToString(), "");
                request.Method = Aria2Consts.TellStatus;
                if (!string.IsNullOrWhiteSpace(aria2secret))
                {
                    request.Params.Add($"token:{aria2secret}");
                }
                request.Params.Add(item.GID);
                _requestsHistory.Add(request);
                requests.Add(request);
            }
            return requests;
        }


    }
}
