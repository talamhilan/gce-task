# gce-task

A .NET 8 Minimal API serves 50 fake location points to a Next.js 14 frontend that renders them on a MapLibre map with a clickable sidebar.

## Layout

- `api/` — .NET 8 Minimal API exposing `GET /api/locations`, organized into `Models/`, `Data/`, and `Endpoints/`
- `api.Tests/` — xUnit + WebApplicationFactory integration tests for the API
- `web/` — Next.js 14 (App Router, TypeScript, Tailwind, `--src-dir`)

## Highlights

- **Error handling** — invalid or negative `cursor` query parameters return RFC 9110 `application/problem+json` (HTTP 400) with a typed `ProblemDetails` body. Rejected requests are logged via `ILogger`.
- **Integration tests** — four `WebApplicationFactory<Program>` tests cover the happy path, last-page metadata, invalid cursor (400), and negative cursor (400). Run with `dotnet test`.
- **OpenAPI / Swagger UI** — Swashbuckle generates a real schema from the typed response records; the interactive UI is served at http://localhost:5000/swagger in Development.
- **Type-safe response contract** — concrete records (`LocationsResponse`, `Metadata`, `Payload<T>`, `Location`) instead of anonymous objects, so the wire format is stable and the OpenAPI schema is accurate.
- **Best practices** — separation of concerns (`Models/` / `Data/` / `Endpoints/`), endpoint extension methods, structured logging, CORS configured for the frontend origin, and a clean composition root in `Program.cs`.

## Prerequisites

- **.NET 8 SDK** — required to build and run both `api/` and `api.Tests/`. Verify with `dotnet --list-sdks` (you should see an `8.0.x` entry); install from https://dotnet.microsoft.com/download/dotnet/8.0 if missing. The SDK install includes the matching ASP.NET Core 8 runtime.
- **Node.js 18.18+** — Next.js 14 requirement. Verify with `node --version`.

## Running locally

```sh
# terminal 1
cd api && dotnet run

# terminal 2
cd web && npm install && npm run dev
```

Then open http://localhost:3000. The API binds to `http://localhost:5000` from `launchSettings.json`; Swagger UI is at http://localhost:5000/swagger when in Development.


## Running tests

```sh
cd api.Tests && dotnet test
```

Four `WebApplicationFactory<Program>` integration tests exercise the happy path, last-page metadata, invalid `cursor` (400), and negative `cursor` (400).

## Design Decisions

**Server Component fetches, Client Component renders.** The page is a Server Component that walks the cursor pagination server-side and passes the assembled `Location[]` to its Client Components as props. No client-side fetching, no API URL exposure to the browser.

**`"use client"` for MapLibre.** MapLibre GL JS touches `window`, `document`, and WebGL — none of which exist on the server. `MapView` is marked `"use client"` and all map APIs run inside `useEffect`, so the server emits a plain `<div>` and the map initializes after hydration.

**MapStateContext over prop drilling.** The MapLibre instance is shared between `Sidebar` and `MapView` via a `useState`-backed React Context. Using state (not a ref) means consumers re-render when the map becomes available — sidebar buttons go from disabled to enabled at the moment clicking them is meaningful.

**Typed response DTOs over anonymous objects.** The endpoint returns concrete records (`LocationsResponse`, `Metadata`, `Payload<T>`, `Location`) so the contract is stable and Swashbuckle generates a real OpenAPI schema. System.Text.Json's default camelCase policy preserves the original wire format.

**Pagination preserved, full set assembled server-side.** The API stays at 10 items per page; the frontend's Server Component loops through all 5 pages before returning HTML, so the browser receives all 50 markers in the initial payload. Keeps the pagination contract intact while still showing the full dataset on first paint.
