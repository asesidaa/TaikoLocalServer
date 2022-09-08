namespace TaikoLocalServer.Services.Interfaces;

public interface IUserDatumService
{
    public Task<UserDatum?> GetFirstUserDatumOrNull(uint baid);
    
    public Task<UserDatum> GetFirstUserDatumOrDefault(uint baid);

    public Task<List<UserDatum>> GetAllUserData();

    public Task UpdateOrInsertUserDatum(UserDatum userDatum);
    
    public Task InsertUserDatum(UserDatum userDatum);
    
    public Task UpdateUserDatum(UserDatum userDatum);
}