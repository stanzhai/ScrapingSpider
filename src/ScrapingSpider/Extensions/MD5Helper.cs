using System;
using System.Security.Cryptography;
using System.Text;

namespace ScrapingSpider.Extensions
{
    public class MD5Helper
    {
        public static string GetMD5HashCode(string text)
        {
            var md5 = new MD5CryptoServiceProvider();
            var bytes = UnicodeEncoding.Default.GetBytes(text);
            var md5HashByte = md5.ComputeHash(bytes);
            return BitConverter.ToInt64(md5HashByte, 0).ToString();
        }
    }
}
