namespace DF.Telegram.Permissions;

public static class TelegramPermissions
{
    public const string GroupName = "Telegram";

    //Add your own permission names. Example:
    //public const string MyPermission1 = GroupName + ".MyPermission1";

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

    public static class QueueLog
    {
        public const string Default = GroupName + ".QueueLog";
    }

}
