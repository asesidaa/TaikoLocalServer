using System.Reflection;
using System.Security.Authentication;
using Serilog.Sinks.File.Header;
using TaikoLocalServer.Logging;
using GameDatabase.Context;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using TaikoLocalServer.Middlewares;
using TaikoLocalServer.Services.Extentions;
using TaikoLocalServer.Settings;
using Throw;
using Serilog;
using SharedProject.Utils;

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

    // Add services to the container.
    builder.Services.AddOptions();
    builder.Services.AddSingleton<IGameDataService, GameDataService>();
    builder.Services.Configure<ServerSettings>(builder.Configuration.GetSection(nameof(ServerSettings)));
    builder.Services.Configure<DataSettings>(builder.Configuration.GetSection(nameof(DataSettings)));
    builder.Services.AddControllers().AddProtoBufNet();
    builder.Services.AddDbContext<TaikoDbContext>(option =>
    {
        var dbName = builder.Configuration["DbFileName"];
        if (string.IsNullOrEmpty(dbName))
        {
            dbName = Constants.DEFAULT_DB_NAME;
        }

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
    app.Use(async (context, next) =>
    {
        await next();
    
        if (context.Response.StatusCode >= 400)
        {
            Log.Error("Unknown request from: {RemoteIpAddress} {Method} {Path} {StatusCode}",
                context.Connection.RemoteIpAddress, context.Request.Method, context.Request.Path, context.Response.StatusCode);
        }
    });
    app.MapControllers();
    app.MapFallbackToFile("index.html");

    app.UseWhen(
        context => context.Request.Path.StartsWithSegments("/sys/servlet/PowerOn", StringComparison.InvariantCulture),
        applicationBuilder => applicationBuilder.UseAllNetRequestMiddleware());

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