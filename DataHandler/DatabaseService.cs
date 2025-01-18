using Microsoft.Data.Sqlite;
using System;

namespace DataLayer
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public DatabaseService(string databasePath, byte[] key, byte[] iv)
        {
            _connectionString = $"Data Source={databasePath}";
            _key = key;
            _iv = iv;
        }

        public void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS SecureData (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EncryptedData BLOB NOT NULL
                )";
            using var command = new SqliteCommand(createTableQuery, connection);
            command.ExecuteNonQuery();
        }

        public void SaveData(string plainText)
        {
            byte[] encryptedData = EncryptionService.Encrypt(plainText, _key, _iv);

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string insertQuery = "INSERT INTO SecureData (EncryptedData) VALUES (@data)";
            using var command = new SqliteCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@data", encryptedData);
            command.ExecuteNonQuery();
        }

        public string ReadData(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string selectQuery = "SELECT EncryptedData FROM SecureData WHERE Id = @id";
            using var command = new SqliteCommand(selectQuery, connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                byte[] encryptedData = (byte[])reader["EncryptedData"];
                return EncryptionService.Decrypt(encryptedData, _key, _iv);
            }

            throw new Exception("Data not found.");
        }
    }
}