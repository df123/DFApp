using DFApp.Aria2.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DFApp.Aria2.Notifications
{
    public class Aria2NotificationManager
    {
        public object ProcessNotification(Aria2Notification notification)
        {
            switch (notification.Method)
            {
                case Aria2Consts.OnDownloadStart:
                    throw new NotImplementedException();
                case Aria2Consts.OnDownloadPause:
                    throw new NotImplementedException();
                case Aria2Consts.OnDownloadStop:
                    throw new NotImplementedException();
                case Aria2Consts.OnDownloadComplete:
                    return DownloadCompleteHandler(notification.Params);
                case Aria2Consts.OnDownloadError:
                    throw new NotImplementedException();
                case Aria2Consts.OnBtDownloadComplete:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }


        public List<Aria2Request> DownloadCompleteHandler(List<ParamsItem> paramsItems)
        {
            List<Aria2Request> requests = new List<Aria2Request>();
            foreach (var item in paramsItems)
            {
                var request = new Aria2Request(Guid.NewGuid().ToString(), "");
                request.Method = Aria2Consts.TellStatus;
                request.Params.Add(item.GID);
                requests.Add(request);
            }
            return requests;
        }


    }
}
