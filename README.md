# gce-task

A .NET 10 Minimal API serves 50 fake location points to a Next.js 14 frontend that renders them on a MapLibre map with a clickable sidebar.

## Layout

- `api/` — .NET 10 Minimal API exposing `GET /api/locations`
- `frontend/` — Next.js 14 (App Router, TypeScript, Tailwind, `--src-dir`)

## Running locally

```sh
# terminal 1
cd api && dotnet run --urls=http://localhost:5000

# terminal 2
cd frontend && npm install && npm run dev
```

Then open http://localhost:3000.

The frontend fetches `http://localhost:5000/api/locations`. The API's `launchSettings.json` does not bind to port 5000 by default — pass `--urls` as above, or edit `api/Properties/launchSettings.json` to listen on `:5000`, or change the URL in `frontend/src/app/page.tsx`.

## Architecture

### Why MapLibre runs in a Client Component, not a Server Component

MapLibre GL JS instantiates its map by accessing browser APIs — `window`, `document`, the DOM `<div>` it mounts to, and a WebGL context — none of which exist during server-side rendering. A Server Component executes inside Node during the React Server Component render pass; if `MapView` were a Server Component, the bundler would import `maplibre-gl` and execute its constructor on the server, crashing with `window is not defined` (or worse, silently bundling map code that can never be used).

`MapView` is therefore marked `"use client"`, and every MapLibre API call lives inside `useEffect`, which only runs in the browser after hydration. The component is still rendered on the server during the initial App Router pass, but at that point its output is just a plain `<div ref={containerRef}>` — no map APIs are touched. The actual map only initializes once the effect fires client-side.

### Server Component fetches data, Client Component renders it

`app/page.tsx` is a Server Component (it's an `async` function with no `"use client"` directive). It runs on the server, walks the cursor pagination to assemble all 50 items from the .NET API, and returns JSX that includes Client Components with the data passed as plain props:

```tsx
const items = await fetchAllLocations(); // server-side fetch loop

return (
  <MapStateProvider>
    <Sidebar locations={items} />
    <MapView locations={items} />
  </MapStateProvider>
);
```

The location array crosses the server/client boundary as serializable props — embedded in the initial HTML payload sent to the browser. There's no client-side fetch, no XHR after page load, and the API URL never appears in the browser. `cache: "no-store"` is set on the fetch so the route stays dynamic; without it Next.js would freeze the response at build time.

### Sharing the map instance across the DOM via React Context

`Sidebar` and `MapView` are siblings in the DOM (a split layout — sidebar on the left, map on the right). They need to share a single object: the MapLibre `Map` instance. Lifting it up to `page.tsx` is not an option because `page.tsx` is a Server Component and cannot hold mutable client state.

`MapStateContext` solves this. The provider stores the `Map | null` in `useState`. `MapView` calls `setMap(instance)` from inside its mount effect; `Sidebar` reads `map` from the same context and calls `map.flyTo({ center, zoom })` on each button click. Because the value is React state (not a ref), `Sidebar` re-renders the moment the map becomes available — its buttons transition from `disabled` to enabled exactly when clicking them is meaningful. The provider lives in `page.tsx`, scoping the context to this route rather than the entire app.

### Pagination

The API supports cursor-based pagination via `?cursor=N` with 10 items per page. `nextCursor` is the offset of the next page, or `null` on the last page. The frontend's Server Component loops through pages in `page.tsx` (`do { ... } while (cursor !== null)`) to assemble the full 50-item set before rendering — five fetches × ten items.

### CORS

The API allows requests from `http://localhost:3000`.

## Judgment calls

Decisions that deviated from literal instructions or weren't explicitly asked about:

- **.NET 10 used instead of .NET 8.** The original prompt specified ".NET 10 Minimal API" so the project targets `net10.0` (the SDK on the build machine was 10.0.203). .NET 10 is the latest version but **not** the current LTS — .NET 8 is. If long-term-support stability matters more than newness, change `<TargetFramework>net10.0</TargetFramework>` in `api/api.csproj` to `net8.0`. The Minimal API code itself is compatible with both.
- **All 50 points loaded on initial page render.** The cursor-pagination contract on the API was preserved (still 10 per page); the frontend's Server Component loops through all 5 pages server-side before returning HTML, so the browser receives all 50 markers in the initial payload. The alternative — bumping the API page size to 50 — was rejected to keep the original 10-per-page contract intact and to actually exercise the cursor mechanism. With a local API and 50 items the 5 round trips are negligible; at larger scales add a `?limit=` parameter on the API.
- **`flyTo` hardcoded to `zoom: 10`.** Without an explicit zoom, `flyTo` keeps the current zoom (initial `2`), so clicking a sidebar button would just slide the globe sideways instead of focusing the location. `10` is roughly city-level — close enough to make a city legible without zooming past it.
- **Path layout.** The Next.js project was scaffolded with `--src-dir`, so all source lives under `frontend/src/`. Components and context were placed at `src/components/` and `src/context/` — not the literal `frontend/components/` or `frontend/context/` from the prompts — so the `@/*` import alias (which resolves to `src/*`) works.
- **Repo `.gitignore`.** Added at the project root to exclude `bin/`, `obj/`, `*.log`, and `.claude/`. Without it, the first `git add .` would have staged ~35 .NET build artifacts and the local Claude Code settings file.
- **MapState uses `useState`, not `useRef`.** A ref would not trigger re-renders when the map is set, so `Sidebar` would not know to enable its buttons. State is the right primitive for "becomes available asynchronously, consumers must react."
- **`maplibre-gl` installed up front.** Needed to type the `Map` instance in the context. Adds the dep one task earlier than strictly necessary, but avoids `unknown` plumbing that would have to be replaced anyway.
- **No `dynamic({ ssr: false })`.** `MapView` is `"use client"` and only touches browser APIs inside `useEffect`. `maplibre-gl` v5 has no module-load-time `window` access. `next build` prerenders the tree without errors. If a future change moves browser code outside an effect, the fix is to wrap `MapView` in a thin client file that uses `dynamic` — `dynamic` with `ssr: false` cannot live in a Server Component (the page) in App Router.
- **Sidebar buttons disabled while `map` is `null`.** Avoids silent no-op clicks during the brief window between SSR and the mount effect creating the map.
- **Marker effect uses a `cancelled` flag and `map.off("load", …)` cleanup.** Without these, an HMR reload or React strict-mode double-mount can leave a deferred `addMarkers` listener registered on the map after its effect's closure has already been "cleaned up", producing orphan markers that the next render then duplicates on top of.

## Not implemented

- No loading or error UI on the fetch.
- No tests.
