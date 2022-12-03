using System.Reflection;
using System.Security.Authentication;
using GameDatabase.Context;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using SharedProject.Utils;
using TaikoLocalServer.Middlewares;
using TaikoLocalServer.Services.Extentions;
using TaikoLocalServer.Settings;
using Throw;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
    .InformationalVersion;
Log.Information("TaikoLocalServer version {Version}", version);

Log.Information("Server starting up...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
    {
        const string configurationsDirectory = "Configurations";
        config.AddJsonFile($"{configurationsDirectory}/Kestrel.json", true, false);
        config.AddJsonFile($"{configurationsDirectory}/Logging.json", false, false);
        config.AddJsonFile($"{configurationsDirectory}/Database.json", false, false);
        config.AddJsonFile($"{configurationsDirectory}/ServerSettings.json", false, false);
        config.AddJsonFile($"{configurationsDirectory}/DataSettings.json", true, false);
    });

    // Manually enable tls 1.0
    builder.WebHost.UseKestrel(kestrelOptions =>
    {
        kestrelOptions.ConfigureHttpsDefaults(options =>
            options.SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13);
    });

    builder.Host.UseSerilog((context, configuration) =>
    {
        configuration.WriteTo.Console().ReadFrom.Configuration(context.Configuration);
    });

    if (builder.Configuration.GetValue<bool>("ServerSettings:EnableMoreSongs"))
        Log.Warning("Song limit expanded! Use at your own risk!");

    // Add services to the container.
    builder.Services.AddOptions();
    builder.Services.AddSingleton<IGameDataService, GameDataService>();
    builder.Services.Configure<ServerSettings>(builder.Configuration.GetSection(nameof(ServerSettings)));
    builder.Services.Configure<DataSettings>(builder.Configuration.GetSection(nameof(DataSettings)));
    builder.Services.AddControllers().AddProtoBufNet();
    builder.Services.AddDbContext<TaikoDbContext>(option =>
    {
        var dbName = builder.Configuration["DbFileName"];
        if (string.IsNullOrEmpty(dbName)) dbName = Constants.DEFAULT_DB_NAME;

        var path = Path.Combine(PathHelper.GetRootPath(), dbName);
        option.UseSqlite($"Data Source={path}");
    });
    builder.Services.AddMemoryCache();
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAllCorsPolicy", policy =>
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

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms, " +
                                  "request host: {RequestHost}";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        };
    });

    var gameDataService = app.Services.GetService<IGameDataService>();
    gameDataService.ThrowIfNull();
    await gameDataService.InitializeAsync();

    // For reverse proxy
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    app.UseCors("AllowAllCorsPolicy");
    // For blazor hosting
    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();
    app.UseRouting();


    app.UseHttpLogging();
    app.MapControllers();
    app.MapFallbackToFile("index.html");

    app.UseWhen(
        context => context.Request.Path.StartsWithSegments("/sys/servlet/PowerOn", StringComparison.InvariantCulture),
        applicationBuilder => applicationBuilder.UseAllNetRequestMiddleware());

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}