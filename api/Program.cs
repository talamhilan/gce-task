var builder = WebApplication.CreateBuilder(args);

const string CorsPolicy = "AllowLocalhost3000";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors(CorsPolicy);

var allLocations = Enumerable.Range(1, 50).Select(i => new Location(
    Id: i.ToString(),
    Lat: 40.7128 + (i * 0.001),
    Lng: -74.0060 + (i * 0.001),
    Name: $"Point {i}",
    Status: i % 2 == 0 ? "inactive" : "active"
)).ToList();

app.MapGet("/api/locations", (string? cursor) =>
{
    const int pageSize = 10;
    var offset = 0;

    if (!string.IsNullOrEmpty(cursor) && int.TryParse(cursor, out var parsed))
    {
        offset = parsed;
    }

    var items = allLocations.Skip(offset).Take(pageSize).ToList();
    var nextOffset = offset + pageSize;
    var hasNextPage = nextOffset < allLocations.Count;

    return Results.Ok(new
    {
        metadata = new
        {
            totalCount = allLocations.Count,
            hasNextPage,
            nextCursor = hasNextPage ? nextOffset.ToString() : null
        },
        payload = new { items }
    });
});

app.Run();

record Location(string Id, double Lat, double Lng, string Name, string Status);
