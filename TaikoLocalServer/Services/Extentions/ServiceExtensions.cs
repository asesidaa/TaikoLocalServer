using TaikoLocalServer.Services.Interfaces;

namespace TaikoLocalServer.Services.Extentions;

public static class ServiceExtensions
{
    public static IServiceCollection AddTaikoDbServices(this IServiceCollection services)
    {
        services.AddScoped<ICardService, CardService>();
        services.AddScoped<IUserDatumService, UserDatumService>();
        services.AddScoped<ISongPlayDatumService, SongPlayDatumService>();
        services.AddScoped<ISongBestDatumService, SongBestDatumService>();
        services.AddScoped<IDanScoreDatumService, DanScoreDatumService>();

        return services;
    }
}