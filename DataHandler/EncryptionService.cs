using System;
using System.IO;
using System.Security.Cryptography;

namespace DataLayer
{
    public static class EncryptionService
    {
        public static byte[] Encrypt(string plainText, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            using var writer = new StreamWriter(cs);
            {
                writer.Write(plainText);
                writer.Flush();
                cs.FlushFinalBlock(); // 确保数据完整
            }

            return ms.ToArray();
        }

        public static string Decrypt(byte[] cipherText, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(cipherText);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cs);
            string result = reader.ReadToEnd();
            return string.IsNullOrEmpty(result) ? throw new Exception("Decryption failed: Empty result.") : result;
        }
    }
}