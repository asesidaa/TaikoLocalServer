using System.Security.Authentication;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using TaikoLocalServer.Middlewares;
using TaikoLocalServer.Settings;

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

var app = builder.Build();

// Migrate db
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TaikoDbContext>();
    db.Database.Migrate();
}

// For reverse proxy
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpLogging();
app.MapControllers();

app.UseWhen(context => context.Request.Path.StartsWithSegments("/sys/servlet/PowerOn", StringComparison.InvariantCulture),
            applicationBuilder => applicationBuilder.UseAllNetRequestMiddleware());

app.MapGet("/", () => "Hello world");

app.Run();