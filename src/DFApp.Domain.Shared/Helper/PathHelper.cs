
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace DFApp.Helper
{
    public class PathHelper
    {
        public static string RemoveInvalidPath(string path)
        {
            string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

            Regex r = new Regex($"[{Regex.Escape(invalidChars)}]");

            string result = r.Replace(path, "");
            return result;
        }

        /// <summary>
        /// 使用指定的字符分割字符串，无法获取分割的值则返回GUID
        /// </summary>
        /// <param name="input">输入值</param>
        /// <param name="splitChart">分割字符串</param>
        /// <param name="position">获取指定位置</param>
        /// <returns>返回分割获取的值或GUID</returns>
        public static  string SplitStringAndGetValueAtPosition(string? input,string splitChart, int position)
        {
            if (string.IsNullOrWhiteSpace(input) 
                || string.IsNullOrWhiteSpace(splitChart) 
                || !input.Contains(splitChart) )
            {
                return Guid.NewGuid().ToString();
            }
            string[] strs = input.Split(splitChart);
            if (strs == null || strs.Length <= 0 || strs.Length - 1 < position)
            {
                return Guid.NewGuid().ToString();
            }
            return strs[position];
        }

    }
}
