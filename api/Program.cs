using api.Endpoints;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicy = "AllowLocalhost3000";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts =>
{
    opts.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GCE Task Locations API",
        Version = "v1"
    });
});

var app = builder.Build();

app.UseCors(CorsPolicy);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapLocationsEndpoints();

app.Run();

public partial class Program { }
