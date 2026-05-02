using api.Data;
using api.Models;

namespace api.Endpoints;

internal static class LocationsEndpoints
{
    private const int PageSize = 10;

    public static IEndpointRouteBuilder MapLocationsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/locations", (string? cursor, ILogger<LocationsEndpoint> logger) =>
        {
            var offset = 0;

            if (!string.IsNullOrEmpty(cursor))
            {
                if (!int.TryParse(cursor, out var parsed) || parsed < 0)
                {
                    logger.LogWarning("Rejected request with invalid cursor: {Cursor}", cursor);
                    return Results.Problem(
                        detail: "cursor must be a non-negative integer",
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Invalid cursor");
                }
                offset = parsed;
            }

            var all = LocationData.AllLocations;
            var items = all.Skip(offset).Take(PageSize).ToList();
            var nextOffset = offset + PageSize;
            var hasNextPage = nextOffset < all.Count;

            var response = new LocationsResponse(
                new Metadata(all.Count, hasNextPage, hasNextPage ? nextOffset.ToString() : null),
                new Payload<Location>(items));

            return Results.Ok(response);
        })
        .WithName("GetLocations")
        .Produces<LocationsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }

    private sealed class LocationsEndpoint { }
}
