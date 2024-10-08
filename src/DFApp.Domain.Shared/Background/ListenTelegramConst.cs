using System;
using System.Collections.Generic;
using System.Text;

namespace DFApp.Background
{
    public static class ListenTelegramConst
    {
        public const string ModuleName = "DFApp.Background.ListenTelegramService";
        public const string DocumentQueue = "ListenTelegramServiceDocumentQueue";
        public const string PhotoQueue = "ListenTelegramServicePhotoQueue";
        public const long SpaceUpperLimitInBytes = 2048L * 1024L * 1024L; // 2 GB
    }
}
