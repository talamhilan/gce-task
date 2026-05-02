using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace api.Tests;

public class LocationsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public LocationsEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task FirstPage_ReturnsOk_With10Items_TotalCount50_NextCursor10()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/locations");

        response.EnsureSuccessStatusCode();
        var doc = await response.Content.ReadFromJsonAsync<JsonElement>();
        var metadata = doc.GetProperty("metadata");
        Assert.Equal(50, metadata.GetProperty("totalCount").GetInt32());
        Assert.True(metadata.GetProperty("hasNextPage").GetBoolean());
        Assert.Equal("10", metadata.GetProperty("nextCursor").GetString());
        Assert.Equal(10, doc.GetProperty("payload").GetProperty("items").GetArrayLength());
    }

    [Fact]
    public async Task LastPage_HasNoNextCursor()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/locations?cursor=40");

        response.EnsureSuccessStatusCode();
        var doc = await response.Content.ReadFromJsonAsync<JsonElement>();
        var metadata = doc.GetProperty("metadata");
        Assert.False(metadata.GetProperty("hasNextPage").GetBoolean());
        Assert.Equal(JsonValueKind.Null, metadata.GetProperty("nextCursor").ValueKind);
        Assert.Equal(10, doc.GetProperty("payload").GetProperty("items").GetArrayLength());
    }

    [Fact]
    public async Task InvalidCursor_Returns400ProblemDetails()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/locations?cursor=abc");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var doc = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(400, doc.GetProperty("status").GetInt32());
        Assert.Equal("Invalid cursor", doc.GetProperty("title").GetString());
    }

    [Fact]
    public async Task NegativeCursor_Returns400()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/locations?cursor=-5");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
