using System;

namespace DFApp.Helper
{
    public class StorageUnitConversionHelper
    {
        public static double ByteToGB(double sizes)
        {
            return ByteToMB(sizes) / 1024d;
        }

        public static double ByteToMB(double sizes)
        {
            return ByteToKB(sizes) / 1024d;
        }

        public static double ByteToKB(double sizes)
        {
            return sizes / 1024d;
        }

        public static string ConvertDataUnit(double data)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            int index = 0;

            while (data >= 1024 && index < units.Length - 1)
            {
                data /= 1024;
                index++;
            }

            return Math.Round(data, 2) + " " + units[index];
        }

    }
}