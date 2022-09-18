using System.Reflection;
using System.Security.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using TaikoLocalServer.Middlewares;
using TaikoLocalServer.Services.Extentions;
using TaikoLocalServer.Settings;
using Throw;
using Serilog;

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
    // Manually enable tls 1.0
    builder.WebHost.UseKestrel(kestrelOptions =>
    {
        kestrelOptions.ConfigureHttpsDefaults(httpsOptions =>
        {
            httpsOptions.SslProtocols =
                SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13;
        });
    });

    builder.Host.UseSerilog((context, configuration) =>
    {
        configuration.WriteTo.Console().ReadFrom.Configuration(context.Configuration);
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