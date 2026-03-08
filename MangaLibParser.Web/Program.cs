using MangaLibParser.Application.Abstractions;
using MangaLibParser.Infrastructure;
using MangaLibParser.Infrastructure.Parsers;
using MangaLibParser.Web.Endpoints;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<PlaywrightBrowserManager>();
builder.Services.AddScoped<IMangaInfoParserService, MangaInfoParserService>();
builder.Services.AddScoped<IUserListParserService, UserListParserService>();

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