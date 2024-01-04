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
}
