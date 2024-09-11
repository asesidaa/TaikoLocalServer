namespace TaikoLocalServer.Services.Extentions;

public static class ServiceExtensions
{
    public static IServiceCollection AddTaikoDbServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserDatumService, UserDatumService>();
        services.AddScoped<ISongPlayDatumService, SongPlayDatumService>();
        services.AddScoped<ISongBestDatumService, SongBestDatumService>();
        services.AddScoped<IDanScoreDatumService, DanScoreDatumService>();
        services.AddScoped<IChallengeCompeteService, ChallengeCompeteService>();

        return services;
    }
}