namespace DataLayer
{
    public interface IDataService
    {
        void SaveData(string plainText);
        string ReadData(int id);
    }
}