using System.Reflection;
using Serilog.Sinks.File.Header;
using TaikoLocalServer.Adapters.AdminApi;
using TaikoLocalServer.Adapters.AdminApi.Controllers;
using TaikoLocalServer.Adapters.AllnetMucha;
using TaikoLocalServer.Adapters.GameProtocol.CnR00;
using TaikoLocalServer.Adapters.GameProtocol.WwR08;
using TaikoLocalServer.Application;
using TaikoLocalServer.Infrastructure;
using TaikoLocalServer.Infrastructure.Persistence;
using TaikoLocalServer.Cli;
using TaikoLocalServer.Logging;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using Throw;
using Serilog;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

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

    builder.Services.AddHttpLogging(options =>
    {
        options.LoggingFields = HttpLoggingFields.All;
        options.RequestBodyLogLimit = 32768;
        options.ResponseBodyLogLimit = 32768;
    });

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
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddAdminApi(builder.Configuration);
    builder.Services.AddAllnetMucha();
    builder.Services.AddGameProtocolWwR08();
    builder.Services.AddGameProtocolCnR00();

    builder.Services.AddControllers().AddProtoBufNet();
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
    builder.Services.AddSingleton<SongBestResponseMapper>();

    var app = builder.Build();

    // Migrate db
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<TaikoDbContext>();
        db.Database.Migrate();

        if (AdminCli.IsAdminCommand(args))
            return await AdminCli.RunAsync(args, db);
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
        if (path.StartsWithSegments("/data") || path.StartsWithSegments("/don-model")
            || (path.Value?.Contains(".db3", StringComparison.OrdinalIgnoreCase) ?? false))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }
        await next();
    });
    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles(new StaticFileOptions
    {
        // The WebUI's own files (js/ui.js, css/app.css, don-embed/…) keep fixed names, so without this browsers
        // guess a cache lifetime and run stale scripts after an update. no-cache = revalidate each load (304 if
        // unchanged). /_framework is left alone: Blazor's boot resources carry their own integrity hashes.
        OnPrepareResponse = ctx =>
        {
            if (!ctx.Context.Request.Path.StartsWithSegments("/_framework"))
            {
                ctx.Context.Response.Headers.CacheControl = "no-cache";
            }
        }
    });
    app.UseRouting();

    // Enable Authentication and Authorization middleware
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAdminApi();

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
