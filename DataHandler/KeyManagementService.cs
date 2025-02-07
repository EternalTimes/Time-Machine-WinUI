using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;

namespace DataLayer
{
    public static class KeyManagementService
    {
        private static readonly string KeyPath = "key.dat";
        private static readonly string IVPath = "iv.dat";

        public static async Task<(byte[] Key, byte[] IV)> GetOrGenerateKeyAndIVAsync()
        {
            if (File.Exists(KeyPath) && File.Exists(IVPath))
            {
                return (await LoadFromFileAsync(KeyPath), await LoadFromFileAsync(IVPath));
            }

            // 生成新的 Key 和 IV
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.GenerateKey();
            aes.GenerateIV();

            await SaveToFileAsync(KeyPath, aes.Key);
            await SaveToFileAsync(IVPath, aes.IV);

            return (aes.Key, aes.IV);
        }

        private static async Task SaveToFileAsync(string filePath, byte[] data)
        {
            var provider = new DataProtectionProvider("LOCAL=user");
            var buffer = CryptographicBuffer.CreateFromByteArray(data);
            var protectedBuffer = await provider.ProtectAsync(buffer);
            CryptographicBuffer.CopyToByteArray(protectedBuffer, out byte[] protectedData);
            await File.WriteAllBytesAsync(filePath, protectedData);
        }

        private static async Task<byte[]> LoadFromFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Key file {filePath} not found.");
            }
            var protectedData = await File.ReadAllBytesAsync(filePath);
            var protectedBuffer = CryptographicBuffer.CreateFromByteArray(protectedData);
            var provider = new DataProtectionProvider();
            var buffer = await provider.UnprotectAsync(protectedBuffer);
            CryptographicBuffer.CopyToByteArray(buffer, out byte[] unprotectedData);
            return unprotectedData;
        }
    }
}
