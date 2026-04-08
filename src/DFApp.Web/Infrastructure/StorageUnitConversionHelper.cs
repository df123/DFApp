namespace DFApp.Web.Infrastructure;

/// <summary>
/// 存储单位转换辅助类
/// </summary>
public static class StorageUnitConversionHelper
{
    /// <summary>
    /// 将字节数转换为 GB
    /// </summary>
    /// <param name="bytes">字节数</param>
    /// <returns>GB 值</returns>
    public static double ByteToGB(double bytes)
    {
        return bytes / (1024.0 * 1024.0 * 1024.0);
    }

    /// <summary>
    /// 将字节数转换为 MB
    /// </summary>
    /// <param name="bytes">字节数</param>
    /// <returns>MB 值</returns>
    public static double ByteToMB(double bytes)
    {
        return bytes / (1024.0 * 1024.0);
    }
}
