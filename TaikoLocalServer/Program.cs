using System.Reflection;
using System.Text;
using Serilog.Sinks.File.Header;
using TaikoLocalServer.Logging;
using GameDatabase.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using TaikoLocalServer.Middlewares;
using TaikoLocalServer.Services.Extentions;
using TaikoLocalServer.Settings;
using Throw;
using Serilog;
using SharedProject.Utils;
using TaikoLocalServer.Controllers.Api;
using TaikoLocalServer.Filters;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

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
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
    builder.Services.AddOptions();
    builder.Services.AddSingleton<IGameDataService, GameDataService>();
    builder.Services.AddScoped<ISongLeaderboardService, SongLeaderboardService>();
    builder.Services.Configure<ServerSettings>(builder.Configuration.GetSection(nameof(ServerSettings)));
    builder.Services.Configure<DataSettings>(builder.Configuration.GetSection(nameof(DataSettings)));
    builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection(nameof(AuthSettings)));

    // Add Authentication with JWT
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration.GetSection(nameof(AuthSettings))["JwtIssuer"],
            ValidAudience = builder.Configuration.GetSection(nameof(AuthSettings))["JwtAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection(nameof(AuthSettings))["JwtKey"] ?? throw new InvalidOperationException()))
        };
    });

    builder.Services.AddScoped<AuthorizeIfRequiredAttribute>(); // Register the custom attribute

    builder.Services.AddControllers().AddProtoBufNet();
    builder.Services.AddDbContext<TaikoDbContext>(option =>
    {
        var dbName = builder.Configuration["DbFileName"];
        if (string.IsNullOrEmpty(dbName))
        {
            dbName = Constants.DefaultDbName;
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
    builder.Services.AddSingleton<SongBestResponseMapper>();

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

    // Use response compression
    app.UseResponseCompression();

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
        else if (context.Response.StatusCode != StatusCodes.Status200OK)
        {
            Log.Warning("Unsuccessful request from: {RemoteIpAddress} {Method} {Path} {StatusCode}",
                context.Connection.RemoteIpAddress, context.Request.Method, context.Request.Path, context.Response.StatusCode);
            Log.Warning("Request headers: {Headers}", context.Request.Headers);
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
