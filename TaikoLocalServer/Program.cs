using Microsoft.AspNetCore.HttpOverrides;
var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddControllers().AddProtoBufNet();
// builder.Services.AddHttpLogging(options => op);

var app = builder.Build();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpLogging();
app.MapControllers();

app.MapGet("/", () => "Hello world");

app.Run();