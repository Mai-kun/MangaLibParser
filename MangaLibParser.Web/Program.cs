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
             .Enrich.WithProperty("WebApi", "MangaLibParser")
             .CreateLogger();
using var listener = new ActivityListenerConfiguration().TraceToSharedLogger();

builder.Services.AddOpenApi();
builder.Services.AddSingleton<PlaywrightBrowserManager>();
builder.Services.AddScoped<IUserListParserService, UserListParserService>();
builder.Services.AddScoped<IMangaInfoParserService, MangaInfoParserService>();
builder.Services.AddScoped<IUserLibrarySyncService, UserLibrarySyncService>();
builder.Services.AddScoped<IMarkdownCreatorService, MarkdownCreatorService>();

builder.Services.AddSingleton<IMarkdownPlanner, MarkdownPlanner>();
builder.Services.AddSingleton<IMangaParsingPlanner, MangaParsingPlanner>();
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