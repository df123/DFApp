using DFApp.Aria2.Notifications;
using DFApp.Aria2.Repository.Response.TellStatus;
using DFApp.Aria2.Request;
using DFApp.Aria2.Response.TellStatus;
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

        public Aria2Manager(ITellStatusResultRepository tellStatusResultRepository)
        {
            _requestsHistory = new List<Aria2Request>();
            _resultRepository = tellStatusResultRepository;
        }

        public List<Aria2Request> ProcessResponse(ResponseBase? response)
        {
            if (response == null)
            {
                return new List<Aria2Request>();
            }

            if (response.Id == null)
            {
                return ProcessNotification(response as Aria2Notification);
            }
            else
            {
                var res = _requestsHistory.FirstOrDefault(x => x.Id == response.Id);
                if (res != null)
                {
                    switch (res.Method)
                    {
                        case Aria2Consts.TellStatus:
                            Logger.LogInformation("Aria2Manager:Aria2Consts.TellStatus");
                            TellStatusResponse? tellStatusResponse = response as TellStatusResponse;
                            if (tellStatusResponse != null)
                            {
                                _resultRepository.InsertAsync(tellStatusResponse.Result);
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

        public List<Aria2Request> ProcessNotification(Aria2Notification? notification)
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
                    return DownloadCompleteHandler(notification.Params);
                default:
                    Logger.LogInformation("default");
                    break;
            }
            return new List<Aria2Request>();
        }


        public List<Aria2Request> DownloadCompleteHandler(List<ParamsItem> paramsItems)
        {
            List<Aria2Request> requests = new List<Aria2Request>();
            foreach (var item in paramsItems)
            {
                var request = new Aria2Request(Guid.NewGuid().ToString(), "");
                request.Method = Aria2Consts.TellStatus;
                request.Params.Add(item.GID);
                _requestsHistory.Add(request);
                requests.Add(request);
            }
            return requests;
        }


    }
}
