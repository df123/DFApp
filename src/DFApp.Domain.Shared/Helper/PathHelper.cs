
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
    }
}
