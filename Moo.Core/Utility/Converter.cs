using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;

namespace Moo.Core.Utility
{
    /// <summary>
    /// 转换
    /// </summary>
    public static class Converter
    {
        static byte[] IV;
        static byte[] Key;

        static Converter()
        {
            using (DES des = DESCryptoServiceProvider.Create())
            {
                des.GenerateIV();
                des.GenerateKey();
                IV = des.IV;
                Key = des.Key;
            }
        }

        public static string ToHexString(byte[] arr)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte value in arr)
            {
                char high = (char)((value >> 4) & 0x0F);
                char low = (char)(value & 0x0F);
                high = (char)(high < 10 ? high + '0' : high + 'A' - 10);
                low = (char)(low < 10 ? low + '0' : low + 'A' - 10);
                sb.Append(high);
                sb.Append(low);
            }
            return sb.ToString();
        }
        public static byte[] FromHexString(string hexString)
        {
            byte[] result = new byte[hexString.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = byte.Parse(hexString.Substring(i * 2, 2));
            }
            return result;
        }

        public static string ToSHA256Hash(string text)
        {
            return ToHexString(SHA256.Create().ComputeHash(Encoding.Unicode.GetBytes(text)));
        }

        public static byte[] Encrypt(byte[] input)
        {
            using (DES des = DESCryptoServiceProvider.Create())
            {
                using (ICryptoTransform encryptor = des.CreateEncryptor(Key, IV))
                {
                    using (MemoryStream mem = new MemoryStream())
                    {
                        using (CryptoStream stream = new CryptoStream(mem, encryptor, CryptoStreamMode.Write))
                        {
                            stream.Write(input, 0, input.Length);
                        }
                        return mem.ToArray();
                    }
                }
            }
        }

        public static byte[] Decrypt(byte[] input)
        {
            using (DES des = DESCryptoServiceProvider.Create())
            {
                using (ICryptoTransform decryptor = des.CreateDecryptor(Key, IV))
                {
                    using (MemoryStream mem = new MemoryStream())
                    {
                        using (CryptoStream stream = new CryptoStream(mem, decryptor, CryptoStreamMode.Write))
                        {
                            stream.Write(input, 0, input.Length);
                        }
                        return mem.ToArray();
                    }
                }

            }
        }
    }
}