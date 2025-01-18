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
        private static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyApp");

        private static readonly string KeyPath = Path.Combine(AppDataPath, "key.dat");
        private static readonly string IVPath = Path.Combine(AppDataPath, "iv.dat");

        static KeyManagementService()
        {
            if (!Directory.Exists(AppDataPath))
            {
                Directory.CreateDirectory(AppDataPath);
            }
        }

        public static async Task<(byte[] Key, byte[] IV)> GetOrGenerateKeyAndIVAsync()
        {
            if (File.Exists(KeyPath) && File.Exists(IVPath))
            {
                Log("Loading existing key and IV...");
                return (await LoadFromFileAsync(KeyPath), await LoadFromFileAsync(IVPath));
            }

            Log("Generating new key and IV...");
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.GenerateKey();
            aes.GenerateIV();

            await SaveToFileAsync(KeyPath, aes.Key);
            await SaveToFileAsync(IVPath, aes.IV);

            Log("New key and IV saved.");
            return (aes.Key, aes.IV);
        }

        private static async Task SaveToFileAsync(string filePath, byte[] data)
        {
            try
            {
                var provider = new DataProtectionProvider("LOCAL=user");
                var buffer = CryptographicBuffer.CreateFromByteArray(data);
                var protectedBuffer = await provider.ProtectAsync(buffer);
                CryptographicBuffer.CopyToByteArray(protectedBuffer, out byte[] protectedData);
                await File.WriteAllBytesAsync(filePath, protectedData);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save data to {filePath}: {ex.Message}", ex);
            }
        }

        private static async Task<byte[]> LoadFromFileAsync(string filePath)
        {
            try
            {
                var protectedData = await File.ReadAllBytesAsync(filePath);
                var protectedBuffer = CryptographicBuffer.CreateFromByteArray(protectedData);
                var provider = new DataProtectionProvider();
                var buffer = await provider.UnprotectAsync(protectedBuffer);
                CryptographicBuffer.CopyToByteArray(buffer, out byte[] unprotectedData);
                return unprotectedData;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load data from {filePath}: {ex.Message}", ex);
            }
        }

        private static void Log(string message)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
        }
    }
}
