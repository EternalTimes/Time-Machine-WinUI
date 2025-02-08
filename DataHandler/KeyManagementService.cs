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
        private static readonly string KeyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TimeMachine", "key.dat");
        private static readonly string IVPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TimeMachine", "iv.dat");

        public static string GetKeyPath()
        {
            return KeyPath;
        }

        public static string GetIVPath()
        {
            return IVPath;
        }

        public static async Task<(byte[] Key, byte[] IV)> GetOrGenerateKeyAndIVAsync()
        {
            string directory = Path.GetDirectoryName(KeyPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (File.Exists(KeyPath) && File.Exists(IVPath))
            {
                return (await LoadFromFileAsync(KeyPath), await LoadFromFileAsync(IVPath));
            }

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
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

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
