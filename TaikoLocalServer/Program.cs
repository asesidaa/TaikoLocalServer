using System.Security.Authentication;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using TaikoLocalServer.Middlewares;
using TaikoLocalServer.Services;
using TaikoLocalServer.Services.Extentions;
using TaikoLocalServer.Services.Interfaces;
using TaikoLocalServer.Settings;
using Throw;

var builder = WebApplication.CreateBuilder(args);
// Manually enable tls 1.0
builder.WebHost.UseKestrel(kestrelOptions =>
{
    kestrelOptions.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13;
    });
});

// Add services to the container.
builder.Services.AddOptions();
builder.Services.AddSingleton<IGameDataService, GameDataService>();
builder.Services.Configure<UrlSettings>(builder.Configuration.GetSection(nameof(UrlSettings)));
builder.Services.AddControllers().AddProtoBufNet();
builder.Services.AddDbContext<TaikoDbContext>(option =>
{
    var dbName = builder.Configuration["DbFileName"];
    if (string.IsNullOrEmpty(dbName))
    {
        dbName = Constants.DEFAULT_DB_NAME;
    }
    var path = Path.Combine(PathHelper.GetDataPath(), dbName);
    option.UseSqlite($"Data Source={path}");
});
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestProperties |
                            HttpLoggingFields.ResponseStatusCode;
});
builder.Services.AddMemoryCache();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCorsPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.Services.AddTaikoDbServices();

var app = builder.Build();

// Migrate db
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TaikoDbContext>();
    db.Database.Migrate();
}

var gameDataService = app.Services.GetService<IGameDataService>();
gameDataService.ThrowIfNull();
await gameDataService.InitializeAsync();

// For reverse proxy
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseCors("DevCorsPolicy");
// For blazor hosting
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();


app.UseHttpLogging();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.UseWhen(context => context.Request.Path.StartsWithSegments("/sys/servlet/PowerOn", StringComparison.InvariantCulture),
            applicationBuilder => applicationBuilder.UseAllNetRequestMiddleware());

app.Run();