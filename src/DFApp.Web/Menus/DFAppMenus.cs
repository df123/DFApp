namespace DFApp.Web.Menus;

public class DFAppMenus
{
    private const string Prefix = "DFApp";
    public const string Home = Prefix + ".Home";

    public static class Media
    {
        public const string Default = Prefix + ".Media";
        public const string ExternalLink = Default + ".ExternalLink";
        public const string Chart = Default + ".Chart";
        public const string Login = Default + ".Login";
    }

    public const string DynamicIP = Prefix + ".DynamicIP";
    public const string Lottery = Prefix + ".Lottery";
    public const string LogSink = Prefix + ".LogSink";
    public const string Bookkeeping = Prefix + ".Bookkeeping";
    public const string FileUploadDownload = Prefix + ".FileUploadDownload";

    public static class ConfigurationInfo
    {
        public const string Default = Prefix + ".ConfigurationInfo";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
    }

    public static class ManagementBackground
    {
        public const string Default = Prefix + ".ManagementBackground";
    }

    //Add your menu items here...

}

