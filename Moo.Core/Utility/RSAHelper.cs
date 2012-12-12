using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
namespace Moo.Core.Utility
{
    public static class RSAHelper
    {
        static RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();

        public static RSAParameters PublicKey
        {
            get
            {
                return rsa.ExportParameters(false);
            }
        }

        public static byte[] Decrypt(byte[] bytes)
        {
            return rsa.Decrypt(bytes, false);
        }
    }
}
