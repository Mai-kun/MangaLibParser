using MangaLibParser.Application.Abstractions;
using MangaLibParser.Application.Services;
using MangaLibParser.Infrastructure;
using MangaLibParser.Infrastructure.Parsers;
using MangaLibParser.Web.Endpoints;
using Scalar.AspNetCore;
using Serilog;
using SerilogTracing;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Debug()
             .WriteTo.Seq("http://localhost:5341")
             .Enrich.FromLogContext()
             .CreateLogger();
using var listener = new ActivityListenerConfiguration().TraceToSharedLogger();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<PlaywrightBrowserManager>();
builder.Services.AddScoped<IMangaInfoParserService, MangaInfoParserService>();
builder.Services.AddScoped<IUserListParserService, UserListParserService>();
builder.Services.AddScoped<IUserLibrarySyncService, UserLibrarySyncService>();
builder.Services.AddSingleton(Log.Logger);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/api", options =>
    {
        options.WithTitle("MangaParser");
    });
}

app.MapMangaEndpoints();
app.MapUserEndpoints();
app.UseHttpsRedirection();
app.Run();