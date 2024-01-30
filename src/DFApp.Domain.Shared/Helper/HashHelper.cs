using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace DFApp.Helper
{
    public class HashHelper
    {
#nullable disable
        private static StringBuilder _sb;
        public static StringBuilder SB
        {
            get
            {
                return _sb ?? (_sb = new StringBuilder());
            }
        }
        private static SHA1 _mySHA1;

        public static SHA1 SHA1P
        {
            get
            {
                return _mySHA1 ?? (_mySHA1 = SHA1.Create());
            }
        }

#nullable restore

        public HashHelper()
        {
            if (_sb == null)
            {
                _sb = new StringBuilder();
            }
            if (_mySHA1 == null)
            {
                _mySHA1 = SHA1.Create();
            }
        }

        public static string CalculationHash(string text)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(text);
            byte[] hashBytes = SHA1P.ComputeHash(inputBytes);
            return PrintHash(hashBytes);
        }

        public static string CalculationHash(Stream fileStream)
        {
            fileStream.Position = 0;
            byte[] hashValue = SHA1P.ComputeHash(fileStream);
            return PrintHash(hashValue);
        }

        private static string PrintHash(byte[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                SB.Append($"{array[i]:X2}");
            }
            string result = SB.ToString();
            SB.Clear();
            return result;
        }
    }
}