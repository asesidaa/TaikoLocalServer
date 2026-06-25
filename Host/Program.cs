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
using TaikoLocalServer.Infrastructure.Persistence;
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
    builder.Configuration.AddJsonFile($"{configurationsDirectory}/DataSettings.json", optional: true, reloadOnChange: false);
    builder.Configuration.AddJsonFile($"{configurationsDirectory}/AuthSettings.json", optional: true, reloadOnChange: false);

    builder.Host.UseSerilog((context, configuration) =>
    {
        configuration
            .WriteTo.Console().ReadFrom.Configuration(context.Configuration)
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

    var gameDataCatalog = app.Services.GetService<IGameDataCatalog>();
    gameDataCatalog.ThrowIfNull();
    await gameDataCatalog.InitializeAsync();

    // Use response compression
    app.UseResponseCompression();

    app.Use(async (context, next) =>
    {
        if (ShouldAssumeProtobufRequest(context.Request))
        {
            context.Request.ContentType = "application/protobuf";
        }

        await next();
    });

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
    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();
    app.UseRouting();

    // Enable Authentication and Authorization middleware
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseHttpLogging();
    app.Use(async (context, next) =>
    {
        await next();

        if (context.Response.StatusCode == StatusCodes.Status404NotFound)
        {
            Log.Error("Unknown request from: {RemoteIpAddress} {Method} {Path} {StatusCode}",
                context.Connection.RemoteIpAddress, context.Request.Method, context.Request.Path, context.Response.StatusCode);
            Log.Error("Request headers: {Headers}", context.Request.Headers);
        }
        else if (context.Response.StatusCode >= StatusCodes.Status400BadRequest
                 && context.Response.StatusCode != StatusCodes.Status401Unauthorized)
        {
            Log.Warning("Unsuccessful request from: {RemoteIpAddress} {Method} {Path} {StatusCode}",
                context.Connection.RemoteIpAddress, context.Request.Method, context.Request.Path, context.Response.StatusCode);
            Log.Warning("Request headers: {Headers}", context.Request.Headers);
        }
    });
    app.MapControllers();
    app.MapFallbackToFile("index.html");

    app.UseAllnetMucha();

    app.Run();
}
catch (Exception ex) when (ex.GetType().Name is not "HostAbortedException")
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}

static bool ShouldAssumeProtobufRequest(HttpRequest request)
{
    if (!HttpMethods.IsPost(request.Method) || !string.IsNullOrWhiteSpace(request.ContentType))
    {
        return false;
    }

    var path = request.Path;
    return path.StartsWithSegments("/v11r01/chassis", StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments("/v10r03/chassis", StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments("/v09r02/chassis", StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments("/v08r01/chassis", StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments("/v08r00_tw/chassis", StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments(WhiteRoutePrefixes.Final, StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments(WhiteRoutePrefixes.Compatibility, StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments(MurasakiRoutePrefixes.Final, StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments(MurasakiRoutePrefixes.Compatibility, StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments(KimidoriRoutePrefixes.Game, StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments(MomoiroRoutePrefixes.Game, StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments("/v01r00/chassis", StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments("/v01r00_tw/chassis", StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments("/v12r08_ww/chassis", StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments("/v12r00_cn/chassis", StringComparison.OrdinalIgnoreCase);
}
