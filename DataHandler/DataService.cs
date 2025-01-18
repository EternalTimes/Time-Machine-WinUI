namespace DataLayer
{
    public class DataService : IDataService
    {
        private readonly DatabaseService _databaseService;

        public DataService(string databasePath, byte[] key, byte[] iv)
        {
            _databaseService = new DatabaseService(databasePath, key, iv);
            _databaseService.InitializeDatabase();
        }

        public void SaveData(string plainText)
        {
            _databaseService.SaveData(plainText);
        }

        public string ReadData(int id)
        {
            return _databaseService.ReadData(id);
        }
    }
}