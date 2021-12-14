using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace STHM.LIB
{
    public static class Cryptography
    {
        public static string Encrypt(string plainText, byte[] key, byte[] IV, int cycle = 1)
        {
            string cipherText = plainText;
            for (int i = 0; i < cycle; i++)
            {
                RijndaelManaged rijndaelManaged = new RijndaelManaged();
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (ICryptoTransform encryptor = rijndaelManaged.CreateEncryptor(key, IV))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                                streamWriter.Write(cipherText);
                        }
                        
                    }
                    cipherText = Convert.ToBase64String(memoryStream.ToArray());
                }
                rijndaelManaged.Clear();
            }
            return cipherText;
        }

        public static string Decrypt(string cipherText, byte[] key, byte[] IV, int cycle = 1)
        {
            string plainText = cipherText;
            for (int i = 0; i < cycle; i++)
            {
                RijndaelManaged rijndaelManaged = new RijndaelManaged();
                using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(plainText)))
                {
                    using (ICryptoTransform decryptor = rijndaelManaged.CreateDecryptor(key, IV))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader streamReader = new StreamReader(cryptoStream))
                                plainText = streamReader.ReadToEnd();
                        }
                    }
                }
                rijndaelManaged.Clear();
            }
            return plainText;
        }
    }
}
