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

var cities = new (string Name, double Lat, double Lng)[]
{
    ("New York", 40.7128, -74.0060),
    ("Los Angeles", 34.0522, -118.2437),
    ("Chicago", 41.8781, -87.6298),
    ("Toronto", 43.6532, -79.3832),
    ("Mexico City", 19.4326, -99.1332),
    ("Vancouver", 49.2827, -123.1207),
    ("Miami", 25.7617, -80.1918),
    ("Honolulu", 21.3069, -157.8583),
    ("São Paulo", -23.5505, -46.6333),
    ("Buenos Aires", -34.6037, -58.3816),
    ("Lima", -12.0464, -77.0428),
    ("Rio de Janeiro", -22.9068, -43.1729),
    ("Bogotá", 4.7110, -74.0721),
    ("Santiago", -33.4489, -70.6693),
    ("London", 51.5074, -0.1278),
    ("Paris", 48.8566, 2.3522),
    ("Berlin", 52.5200, 13.4050),
    ("Madrid", 40.4168, -3.7038),
    ("Rome", 41.9028, 12.4964),
    ("Moscow", 55.7558, 37.6173),
    ("Stockholm", 59.3293, 18.0686),
    ("Athens", 37.9838, 23.7275),
    ("Istanbul", 41.0082, 28.9784),
    ("Amsterdam", 52.3676, 4.9041),
    ("Reykjavík", 64.1466, -21.9426),
    ("Cairo", 30.0444, 31.2357),
    ("Lagos", 6.5244, 3.3792),
    ("Johannesburg", -26.2041, 28.0473),
    ("Nairobi", -1.2864, 36.8172),
    ("Casablanca", 33.5731, -7.5898),
    ("Cape Town", -33.9249, 18.4241),
    ("Tokyo", 35.6762, 139.6503),
    ("Beijing", 39.9042, 116.4074),
    ("Mumbai", 19.0760, 72.8777),
    ("Bangkok", 13.7563, 100.5018),
    ("Singapore", 1.3521, 103.8198),
    ("Seoul", 37.5665, 126.9780),
    ("Dubai", 25.2048, 55.2708),
    ("Hong Kong", 22.3193, 114.1694),
    ("Manila", 14.5995, 120.9842),
    ("Karachi", 24.8607, 67.0011),
    ("Tehran", 35.6892, 51.3890),
    ("Riyadh", 24.7136, 46.6753),
    ("Jakarta", -6.2088, 106.8456),
    ("Kuala Lumpur", 3.1390, 101.6869),
    ("Sydney", -33.8688, 151.2093),
    ("Auckland", -36.8485, 174.7633),
    ("Melbourne", -37.8136, 144.9631),
    ("Brisbane", -27.4698, 153.0251),
    ("Perth", -31.9505, 115.8605),
};

var allLocations = cities.Select((c, idx) => new Location(
    Id: (idx + 1).ToString(),
    Lat: c.Lat,
    Lng: c.Lng,
    Name: c.Name,
    Status: idx % 2 == 0 ? "active" : "inactive"
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
