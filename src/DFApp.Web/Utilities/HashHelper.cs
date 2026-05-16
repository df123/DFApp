using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DFApp.Web.Utilities;

/// <summary>
/// 哈希计算工具类
/// </summary>
public static class HashHelper
{
    /// <summary>
    /// 计算流的 SHA1 哈希值
    /// </summary>
    /// <param name="stream">输入流</param>
    /// <returns>SHA1 哈希字符串（小写十六进制）</returns>
    public static string CalculateHash(Stream stream)
    {
        stream.Position = 0;
        var hashBytes = SHA1.HashData(stream);
        return ConvertToHexString(hashBytes);
    }

    /// <summary>
    /// 计算字符串的 SHA1 哈希值
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <returns>SHA1 哈希字符串（小写十六进制）</returns>
    public static string CalculateHash(string input)
    {
        var hashBytes = SHA1.HashData(Encoding.UTF8.GetBytes(input));
        return ConvertToHexString(hashBytes);
    }

    private static string ConvertToHexString(byte[] bytes)
    {
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }
}
