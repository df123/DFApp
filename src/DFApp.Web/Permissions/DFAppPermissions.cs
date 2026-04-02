namespace DFApp.Web.Permissions;

/// <summary>
/// 权限常量定义，包含所有功能模块的权限分组与权限名称
/// </summary>
public static class DFAppPermissions
{
    /// <summary>权限组根名称</summary>
    public const string GroupName = "DFApp";

    /// <summary>用户管理权限</summary>
    public static class UserManagement
    {
        public const string Default = GroupName + ".UserManagement";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string ChangePassword = Default + ".ChangePassword";
    }

    /// <summary>媒体管理权限</summary>
    public static class Medias
    {
        public const string Default = GroupName + ".Medias";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Download = Default + ".Download";
    }

    /// <summary>动态IP管理权限</summary>
    public static class DynamicIP
    {
        public const string Default = GroupName + ".DynamicIP";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>彩票管理权限</summary>
    public static class Lottery
    {
        public const string Default = GroupName + ".Lottery";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>日志接收器权限</summary>
    public static class LogSink
    {
        public const string Default = GroupName + ".LogSink";
        public const string QueueSink = Default + ".QueueSink";
        public const string SignalRSink = Default + ".SignalRSink";
    }

    /// <summary>记账管理权限</summary>
    public static class Bookkeeping
    {
        public const string Default = GroupName + ".Bookkeeping";
    }

    /// <summary>记账分类权限</summary>
    public static class BookkeepingCategory
    {
        public const string Default = Bookkeeping.Default + ".Category";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>记账支出权限</summary>
    public static class BookkeepingExpenditure
    {
        public const string Default = Bookkeeping.Default + ".Expenditure";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Analysis = Default + ".Analysis";
    }

    /// <summary>文件上传下载权限</summary>
    public static class FileUploadDownload
    {
        public const string Default = GroupName + ".FileUploadDownload";
        public const string Upload = Default + ".Upload";
        public const string Download = Default + ".Download";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>配置信息权限</summary>
    public static class ConfigurationInfo
    {
        public const string Default = GroupName + ".ConfigurationInfo";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>Aria2管理权限</summary>
    public static class Aria2
    {
        public const string Default = GroupName + ".Aria2";
        public const string Link = Default + ".Link";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>日志查看权限</summary>
    public static class LogViewer
    {
        public const string Default = GroupName + ".LogViewer";
    }

    /// <summary>RSS管理权限</summary>
    public static class Rss
    {
        public const string Default = GroupName + ".Rss";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string Download = Default + ".Download";
    }

    /// <summary>文件过滤权限</summary>
    public static class FileFilter
    {
        public const string Default = GroupName + ".FileFilter";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>电动车管理权限</summary>
    public static class ElectricVehicle
    {
        public const string Default = GroupName + ".ElectricVehicle";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Statistics = Default + ".Statistics";
    }

    /// <summary>电动车费用权限</summary>
    public static class ElectricVehicleCost
    {
        public const string Default = ElectricVehicle.Default + ".Cost";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Analysis = Default + ".Analysis";
    }

    /// <summary>电动车充电记录权限</summary>
    public static class ElectricVehicleChargingRecord
    {
        public const string Default = ElectricVehicle.Default + ".ChargingRecord";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>油价信息权限</summary>
    public static class GasolinePrice
    {
        public const string Default = ElectricVehicle.Default + ".GasolinePrice";
    }

    /// <summary>RSS订阅权限</summary>
    public static class RssSubscription
    {
        public const string Default = GroupName + ".RssSubscription";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string Download = Default + ".Download";
    }
}
