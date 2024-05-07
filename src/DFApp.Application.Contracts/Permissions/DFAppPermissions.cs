namespace DFApp.Permissions;

public static class DFAppPermissions
{
    public const string GroupName = "DFApp";

    public static class Medias
    {
        public const string Default = GroupName + ".Medias";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Download = Default + ".Download";
    }

    public static class DynamicIP
    {
        public const string Default = GroupName + ".DynamicIP";
        public const string Delete = Default + ".Delete";
    }

    public static class Lottery
    {
        public const string Default = GroupName + ".Lottery";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class LogSink
    {
        public const string Default = GroupName + ".LogSink";
        public const string QueueSink = Default + ".QueueSink";
        public const string SignalRSink = Default + ".SignalRSink";
    }

    public static class Bookkeeping
    {
        public const string Default = GroupName + ".Bookkeeping";
    }

    public static class BookkeepingCategory
    {
        public const string Default = Bookkeeping.Default + ".Category";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class BookkeepingExpenditure
    {
        public const string Default = Bookkeeping.Default + ".Expenditure";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Analysis = Default + ".Analysis";
    }

    public static class FileUploadDownload
    {
        public const string Default = GroupName + ".FileUploadDownload";
        public const string Upload = Default + ".Upload";
        public const string Download = Default + ".Download";
        public const string Delete = Default + ".Delete";
    }

    public static class ConfigurationInfo
    {
        public const string Default = GroupName + ".ConfigurationInfo";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class ManagementBackground
    {
        public const string Default = GroupName + ".ManagementBackground";
        public const string Restart = Default + ".Restart";
        public const string Stop = Default + ".Stop";
    }

    public static class Aria2 {
        public const string Default = GroupName + ".Aria2";
        public const string Link = Default + ".Link";
        public const string Delete = Default + ".Delete";
    }


}
