namespace api.Models;

public record Location(string Id, double Lat, double Lng, string Name, string Status);

public record Metadata(int TotalCount, bool HasNextPage, string? NextCursor);

public record Payload<T>(IReadOnlyList<T> Items);

public record LocationsResponse(Metadata Metadata, Payload<Location> Payload);
