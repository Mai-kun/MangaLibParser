using MangaLibParser.Application.Abstractions;
using MangaLibParser.Application.Services;
using MangaLibParser.Infrastructure;
using MangaLibParser.Infrastructure.Parsers;
using MangaLibParser.Web;
using MangaLibParser.Web.Endpoints;
using Scalar.AspNetCore;
using Serilog;
using SerilogTracing;

var builder = WebApplication.CreateBuilder(args);

using var listener = new ActivityListenerConfiguration().TraceToSharedLogger();

builder.Services.AddOpenApi();
builder.Services.AddSingleton<PlaywrightBrowserManager>();
builder.Services.AddScoped<IUserListParserService, UserListParserService>();
builder.Services.AddScoped<IMangaInfoParserService, MangaInfoParserService>();
builder.Services.AddScoped<IUserLibrarySyncService, UserLibrarySyncService>();
builder.Services.AddScoped<IMarkdownCreatorService, MarkdownCreatorService>();

builder.Services.AddSingleton<IMarkdownPlanner, MarkdownPlanner>();
builder.Services.AddSingleton<IMangaParsingPlanner, MangaParsingPlanner>();
builder.Services.AddSerilog((services, configuration) =>
{
    configuration.ReadFrom.Configuration(builder.Configuration);
    configuration.ReadFrom.Services(services);
    configuration.Enrich.FromLogContext();
    configuration.MinimumLevel.Debug();
    configuration.WriteTo.Seq("http://localhost:5341");
});

builder.Services.AddExceptionsHandlers();

var app = builder.Build();
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/api", options =>
    {
        options.WithTitle("MangaParser");
    });
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

app.MapMangaEndpoints();
app.MapUserEndpoints();

app.Run();