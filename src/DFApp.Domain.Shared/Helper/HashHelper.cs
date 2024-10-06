using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DFApp.Helper
{
    public static class HashHelper
    {
        private static readonly SHA1 _sha1 = SHA1.Create();
        private static readonly MD5 _md5 = MD5.Create();
        private static readonly StringBuilder _sb = new StringBuilder();

        public static string CalculateHash(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException(nameof(text));

            byte[] inputBytes = Encoding.UTF8.GetBytes(text);
            return ComputeHash(_sha1, inputBytes);
        }

        public static string CalculateHash(Stream fileStream)
        {
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));

            fileStream.Position = 0;
            return ComputeHash(_sha1, fileStream);
        }

        public static string CalculateMD5(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException(nameof(text));

            byte[] inputBytes = Encoding.UTF8.GetBytes(text);
            return ComputeHash(_md5, inputBytes);
        }

        public static string CalculateMD5(Stream fileStream)
        {
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));

            fileStream.Position = 0;
            return ComputeHash(_md5, fileStream);
        }

        private static string ComputeHash(HashAlgorithm hashAlgorithm, byte[] inputBytes)
        {
            byte[] hashBytes = hashAlgorithm.ComputeHash(inputBytes);
            return FormatHash(hashBytes);
        }

        private static string ComputeHash(HashAlgorithm hashAlgorithm, Stream inputStream)
        {
            byte[] hashBytes = hashAlgorithm.ComputeHash(inputStream);
            return FormatHash(hashBytes);
        }

        private static string FormatHash(byte[] hashBytes)
        {
            _sb.Clear();
            foreach (byte b in hashBytes)
            {
                _sb.Append(b.ToString("X2"));
            }
            return _sb.ToString();
        }
    }
}
