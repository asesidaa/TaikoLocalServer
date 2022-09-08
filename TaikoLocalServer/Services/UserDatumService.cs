using TaikoLocalServer.Services.Interfaces;

namespace TaikoLocalServer.Services;

public class UserDatumService : IUserDatumService
{
    private readonly TaikoDbContext context;

    public UserDatumService(TaikoDbContext context)
    {
        this.context = context;
    }

    public async Task<UserDatum?> GetFirstUserDatumOrNull(uint baid)
    {
        return await context.UserData.FindAsync(baid);
    }

    public async Task<UserDatum> GetFirstUserDatumOrDefault(uint baid)
    {
        return await context.UserData.FindAsync(baid) ?? new UserDatum();
    }

    public async Task<List<UserDatum>> GetAllUserData()
    {
        return await context.UserData.ToListAsync();
    }

    public async Task UpdateOrInsertUserDatum(UserDatum userDatum)
    {
        if (await context.UserData.AnyAsync(datum => datum.Baid == userDatum.Baid))
        {
            context.UserData.Add(userDatum);
            await context.SaveChangesAsync();
            return;
        }

        context.Update(userDatum);
        await context.SaveChangesAsync();
    }

    public async Task InsertUserDatum(UserDatum userDatum)
    {
        context.UserData.Add(userDatum);
        await context.SaveChangesAsync();
    }

    public async Task UpdateUserDatum(UserDatum userDatum)
    {
        context.Update(userDatum);
        await context.SaveChangesAsync();
    }
}