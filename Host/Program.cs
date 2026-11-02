using System.Reflection;
using Serilog.Sinks.File.Header;
using TaikoLocalServer.Adapters.AdminApi;
using TaikoLocalServer.Adapters.AdminApi.Controllers;
using TaikoLocalServer.Adapters.AllnetMucha;
using TaikoLocalServer.Adapters.GameProtocol.Blue;
using TaikoLocalServer.Adapters.GameProtocol.CnR00;
using TaikoLocalServer.Adapters.GameProtocol.Green;
using TaikoLocalServer.Adapters.GameProtocol.Kimidori;
using TaikoLocalServer.Adapters.GameProtocol.Momoiro;
using TaikoLocalServer.Adapters.GameProtocol.Murasaki;
using TaikoLocalServer.Adapters.GameProtocol.Red;
using TaikoLocalServer.Adapters.GameProtocol.White;
using TaikoLocalServer.Adapters.GameProtocol.WwR08;
using TaikoLocalServer.Adapters.GameProtocol.Yellow;
using TaikoLocalServer.Application;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure;
using TaikoLocalServer.Infrastructure.Identity;
using TaikoLocalServer.Infrastructure.Persistence;
using TaikoLocalServer.Cli;
using TaikoLocalServer.Logging;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using Throw;
using Serilog;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using TaikoLocalServer.Adapters.GameProtocol.Shared;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
    .InformationalVersion;
Log.Information("TaikoLocalServer version {Version}", version);

var buildTime = Assembly.GetEntryAssembly()?
    .GetCustomAttributes<AssemblyMetadataAttribute>()
    .FirstOrDefault(attr => attr.Key == "BuildTime")?.Value;

if (buildTime != null)
{
    Log.Information("Build time: {BuildTime}", buildTime);
}

Log.Information("Server starting up...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    const string configurationsDirectory = "Configurations";
    builder.Configuration.AddJsonFile($"{configurationsDirectory}/Kestrel.json", optional: true, reloadOnChange: false);
    builder.Configuration.AddJsonFile($"{configurationsDirectory}/Logging.json", optional: false, reloadOnChange: false);
    builder.Configuration.AddJsonFile($"{configurationsDirectory}/Database.json", optional: false, reloadOnChange: false);
    builder.Configuration.AddJsonFile($"{configurationsDirectory}/ServerSettings.json", optional: false, reloadOnChange: false);
    // No longer shipped: an older install's copy is read once, to import its rules (and its own JWT key) into the
    // database and to infer Mode when ServerSettings.json doesn't set it.
    builder.Configuration.AddJsonFile($"{configurationsDirectory}/AuthSettings.json", optional: true, reloadOnChange: false);

    // The live log (Server Management › Logs) reads the same events as the console and the log file.
    var logBuffer = new LogBuffer();
    builder.Services.AddSingleton<TaikoLocalServer.Application.Abstractions.ILogBuffer>(logBuffer);
    builder.Host.UseSerilog((context, configuration) =>
    {
        configuration
            .WriteTo.Console().ReadFrom.Configuration(context.Configuration)
            .WriteTo.Sink(logBuffer)
            .WriteTo.Logger(x =>
            {
                x.WriteTo.File(new CsvFormatter(),
                    path: "./Logs/HeadClerkLog-.csv",
                    hooks: new HeaderWriter("Date,ChassisId,ShopId,Baid,PlayedAt,IsRight,Type,Amount"),
                    rollingInterval: RollingInterval.Day);
                x.Filter.ByIncludingOnly("StartsWith(@m, 'CSV WRITE:')");
            });
    });

    if (builder.Configuration.GetValue<bool>("ServerSettings:EnableMoreSongs"))
    {
        Log.Warning("Song limit expanded! Use at your own risk!");
    }

    var serverSettingsConfig = builder.Configuration.GetSection("ServerSettings");
    var enabledEras = GameProtocolApplicationParts.ReadEnabledEras(serverSettingsConfig);

    if (enabledEras.Count == 0)
    {
        Log.Fatal("ServerSettings.Eras has no enabled era. At least one era (Nijiiro, Green, Blue, Yellow, Red, White, Murasaki, Kimidori, or Momoiro) must be enabled in Host/Configurations/ServerSettings.json. Refusing to start.");
        throw new InvalidOperationException("No game eras enabled.");
    }

    Log.Information("Enabled game eras: {Eras}", string.Join(", ", enabledEras));

    builder.Services.AddHttpLogging(options =>
    {
        options.LoggingFields = HttpLoggingFields.All;
        options.RequestBodyLogLimit = 32768;
        options.ResponseBodyLogLimit = 32768;
    });

    // Add response compression services
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<GzipCompressionProvider>();
        options.Providers.Add<BrotliCompressionProvider>();
    });

    builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Fastest;
    });

    // Add services to the container.
    builder.Services.AddOptions();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration, enabledEras);
    builder.Services.AddAdminApi(builder.Configuration);
    builder.Services.AddAllnetMucha();
    // Every game's cabinets are checked (blocked, approval only), whichever games are on.
    TaikoLocalServer.Adapters.GameProtocol.Shared.Cabinets.CabinetFilter.AddOnce(builder.Services);
    if (enabledEras.Contains(GameEra.Nijiiro))
    {
        builder.Services.AddGameProtocolWwR08();
        builder.Services.AddGameProtocolCnR00();
    }
    if (enabledEras.Contains(GameEra.Green))
    {
        builder.Services.AddGameProtocolGreen();
    }
    if (enabledEras.Contains(GameEra.Blue))
    {
        builder.Services.AddGameProtocolBlue();
    }
    if (enabledEras.Contains(GameEra.Yellow))
    {
        builder.Services.AddGameProtocolYellow();
    }
    if (enabledEras.Contains(GameEra.Red))
    {
        builder.Services.AddGameProtocolRed();
    }
    if (enabledEras.Contains(GameEra.White))
    {
        builder.Services.AddGameProtocolWhite();
    }
    if (enabledEras.Contains(GameEra.Murasaki))
    {
        builder.Services.AddGameProtocolMurasaki();
    }
    if (enabledEras.Contains(GameEra.Kimidori))
    {
        builder.Services.AddGameProtocolKimidori();
    }
    if (enabledEras.Contains(GameEra.Momoiro))
    {
        builder.Services.AddGameProtocolMomoiro();
    }

    builder.Services.AddControllers()
        .AddProtoBufNet()
        .ConfigureApplicationPartManager(apm =>
        {
            // Adapter assemblies referenced by Host are auto-discovered as ApplicationParts.
            // Remove disabled-era assemblies so their controllers are not routed.
            GameProtocolApplicationParts.RemoveDisabledGameProtocolApplicationParts(apm, enabledEras);
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
    var app = builder.Build();

    // Migrate db
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<TaikoDbContext>();
        var migrator = scope.ServiceProvider.GetRequiredService<DatabaseStartupMigrator>();
        await migrator.MigrateAsync(db);
        // Access › Activity keeps a year, Sign-ins 90 days; sessions go a week after their last use (the token lifetime).
        db.Activity.Where(e => e.Time < DateTime.UtcNow.AddDays(-365)).ExecuteDelete();
        db.SignInAttempts.Where(e => e.Time < DateTime.UtcNow.AddDays(-90)).ExecuteDelete();
        db.WebSessions.Where(e => e.LastSeenAt < DateTime.UtcNow.AddDays(-7)).ExecuteDelete();

        if (AdminCli.IsAdminCommand(args))
            return await AdminCli.RunAsync(args, db);
    }
    await app.Services.GetRequiredService<ServerAccess>().InitializeAsync();
    await app.Services.GetRequiredService<TaikoLocalServer.Application.Abstractions.ICabinetRegistry>().InitializeAsync();
    await app.Services.GetRequiredService<TaikoLocalServer.Application.Abstractions.IShopLiveSeasons>().InitializeAsync();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms, " +
                                  "request host: {RequestHost}";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        };
        // A WebUI page load fetches dozens of files (the WASM app, CSS, game art); logging each buries the lines that
        // matter. Successful GETs of files drop to Debug; API calls, game requests (.php) and failures still log.
        options.GetLevel = (httpContext, _, ex) =>
        {
            var path = httpContext.Request.Path;
            var isFile = HttpMethods.IsGet(httpContext.Request.Method)
                         && (path.StartsWithSegments("/api/Assets") || (Path.HasExtension(path.Value) && !path.Value!.EndsWith(".php")));
            return ex is null && httpContext.Response.StatusCode < 400 && isFile
                ? Serilog.Events.LogEventLevel.Debug
                : Serilog.Events.LogEventLevel.Information;
        };
    });

    TaikoLocalServer.Infrastructure.GameDataCatalog.PathHelper.MoveLegacyGameDataFolder();
    var gameDataCatalog = app.Services.GetService<IGameDataCatalog>();
    gameDataCatalog.ThrowIfNull();
    await gameDataCatalog.InitializeAsync();

    // Use response compression
    app.UseResponseCompression();

    // For reverse proxy
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    app.UseCors("AllowAllCorsPolicy");
    // For blazor hosting
    if (app.Environment.IsDevelopment())
    {
        // The Blazor WASM boot manifest under /_framework is not content-hashed,
        // so a cached one will pin mismatched assembly versions across rebuilds.
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/_framework"))
            {
                context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
                context.Response.Headers.Pragma = "no-cache";
            }
            await next();
        });
    }
    // Server data shares wwwroot with the WebUI; only the WebUI's own files may be served statically.
    // Datatables and models go through authenticated api/GameData and api/Assets instead.
    app.Use(async (context, next) =>
    {
        var path = context.Request.Path;
        // /don-model: the folder's old name, in case an install still has one.
        if (path.StartsWithSegments("/data") || path.StartsWithSegments("/game_data") || path.StartsWithSegments("/don-model")
            || (path.Value?.Contains(".db3", StringComparison.OrdinalIgnoreCase) ?? false))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }
        await next();
    });
    app.UseBlazorFrameworkFiles();
    // The WebUI's own files (index.html, js/ui.js, js/don-viewer.js, css/app.css, lib/three/…) keep fixed names, so
    // without this browsers guess a cache lifetime and run stale scripts after an update. no-cache = revalidate each
    // load (304 if unchanged). /_framework is left alone: Blazor's boot resources carry their own integrity hashes.
    var webUiFiles = new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            if (!ctx.Context.Request.Path.StartsWithSegments("/_framework"))
            {
                ctx.Context.Response.Headers.CacheControl = "no-cache";
            }
        }
    };
    app.UseStaticFiles(webUiFiles);
    app.UseRouting();
    app.UseGameProtocolProtobufRequestContentTypeFallback();

    // Enable Authentication and Authorization middleware
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAdminApi();

    app.UseHttpLogging();
    // Spots game requests the server doesn't handle (yet). The Web UI's own API is left out: its 404s are normal
    // (a missing avatar, say) and the page shows its errors. Sign-in headers never reach the log.
    app.Use(async (context, next) =>
    {
        await next();

        if (context.Request.Path.StartsWithSegments("/api")) return;
        var headers = context.Request.Headers
            .Where(h => !h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) && !h.Key.Equals("Cookie", StringComparison.OrdinalIgnoreCase))
            .Select(h => $"{h.Key}: {h.Value}");
        if (context.Response.StatusCode == StatusCodes.Status404NotFound)
        {
            Log.Error("Unknown request from: {RemoteIpAddress} {Method} {Path} {StatusCode}",
                context.Connection.RemoteIpAddress, context.Request.Method, context.Request.Path, context.Response.StatusCode);
            Log.Error("Request headers: {Headers}", headers);
        }
        else if (context.Response.StatusCode >= StatusCodes.Status400BadRequest
                 && context.Response.StatusCode != StatusCodes.Status401Unauthorized)
        {
            Log.Warning("Unsuccessful request from: {RemoteIpAddress} {Method} {Path} {StatusCode}",
                context.Connection.RemoteIpAddress, context.Request.Method, context.Request.Path, context.Response.StatusCode);
            Log.Warning("Request headers: {Headers}", headers);
        }
    });
    app.MapControllers();
    app.MapFallbackToFile("index.html", webUiFiles);

    app.UseAllnetMucha();

    app.Run();
    return 0;
}
catch (Exception ex) when (ex.GetType().Name is not "HostAbortedException")
{
    Log.Fatal(ex, "Unhandled exception");
    return 1;
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
