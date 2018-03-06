using System;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;
using System.IO;

namespace renstech.NET.SupernovaDispatcher.Utils
{
    public class Encrypt
    {
        private static readonly byte[] KEY = new byte[]
        {
            196,24,168,175,108,246,222,94,203,192,231,164,14,114,143,27,158,210,30,206,24,85,58,225
        };

        private static readonly byte[] IV = new byte[]
        {
            134,57,177,53,211,71,133,173
        };

        private void GenerateKey()
        {
            var crypto = new TripleDESCryptoServiceProvider();
            crypto.GenerateKey();
            crypto.GenerateIV();

            Debug.WriteLine(string.Join(",", crypto.Key));
            Debug.WriteLine(string.Join(",", crypto.IV));
        }

        public static string TripleDESEncrypt(string plain)
        {
            // Create encryption provider.
            var crypto = new TripleDESCryptoServiceProvider()
            {
                Key = KEY,
                IV = IV
            };

            crypto.Mode = CipherMode.CBC;
            crypto.Padding = System.Security.Cryptography.PaddingMode.PKCS7;

            string encryped = null;

            try
            {
                MemoryStream outStream = new MemoryStream();

                CryptoStream cStream = new CryptoStream(outStream,
                    crypto.CreateEncryptor(), CryptoStreamMode.Write);

                StreamWriter sWriter = new StreamWriter(cStream);
                sWriter.Write(plain);
                sWriter.Flush();
                cStream.FlushFinalBlock();
                sWriter.Flush();

                StreamReader Reader = new StreamReader(outStream);
                encryped = Reader.ReadLine();

                encryped = Convert.ToBase64String(outStream.GetBuffer(), 0, (int)outStream.Length);

                sWriter.Close();
                outStream.Close();
                cStream.Close();
            }
            catch (System.Exception ex)
            {
                return "";  
            }

            return encryped;
        }

        public static string TripleDESDecrypt(string encoded)
        {
            TripleDESCryptoServiceProvider DES = new TripleDESCryptoServiceProvider()
            {
                Key = KEY,
                IV = IV
            };

            DES.Mode = CipherMode.CBC;
            DES.Padding = System.Security.Cryptography.PaddingMode.PKCS7;

            ICryptoTransform DESDecrypt = DES.CreateDecryptor();

            string result = "";
            try
            {
                byte[] Buffer = Convert.FromBase64String(encoded);
                result = ASCIIEncoding.ASCII.GetString(DESDecrypt.TransformFinalBlock(Buffer, 0, Buffer.Length));
            }
            catch (Exception e)
            {
                return null;
            }
            return result;
        }

        public static string GetMD5Hash(string input)
        {
            MD5CryptoServiceProvider x = new MD5CryptoServiceProvider();
            byte[] bs = Encoding.UTF8.GetBytes(input);
            bs = x.ComputeHash(bs);
            StringBuilder s = new StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            string password = s.ToString();
            return password;
        }
    }
}
