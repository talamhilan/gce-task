# gce-task

A .NET 10 Minimal API serves 50 fake location points to a Next.js 14 frontend that renders them on a MapLibre map with a clickable sidebar.

## Layout

- `api/` — .NET 10 Minimal API exposing `GET /api/locations`
- `frontend/` — Next.js 14 (App Router, TypeScript, Tailwind, `--src-dir`)

## Running locally

```sh
# terminal 1
cd api && dotnet run

# terminal 2
cd frontend && npm install && npm run dev
```

Then open http://localhost:3000.

The frontend fetches `http://localhost:5000/api/locations`. The API's `launchSettings.json` does not bind to port 5000 by default — either edit `api/Properties/launchSettings.json` to listen on `:5000`, or change the URL in `frontend/src/app/page.tsx`.

## Architecture

### Data flow

`app/page.tsx` is a Server Component. It fetches the API server-side, extracts `data.payload.items`, and hands the array to client components as a `locations` prop. `cache: "no-store"` is set on the fetch so the route is dynamic — without it, the response would be frozen at build time, which is confusing during development.

### Sharing the map instance

`MapView` creates the MapLibre `Map` in a mount effect and registers it via `MapStateContext`. `Sidebar` consumes the same context and calls `map.flyTo(...)` on click. The provider is scoped to the page, not the root layout, so the context only exists where it's needed.

### Pagination

The API supports cursor-based pagination via `?cursor=N` with 10 items per page. `nextCursor` is the offset of the next page (or `null` on the last page). The frontend currently fetches page 1 only.

### CORS

The API allows requests from `http://localhost:3000`.

## Judgment calls

Decisions that deviated from literal instructions or weren't explicitly asked about:

- **Path layout.** The Next.js project was scaffolded with `--src-dir`, so all source lives under `frontend/src/`. Components and context were placed at `src/components/` and `src/context/` — not the literal `frontend/components/` or `frontend/context/` from the prompts — so the `@/*` import alias (which resolves to `src/*`) works.
- **Repo `.gitignore`.** Added at the project root to exclude `bin/`, `obj/`, `*.log`, and `.claude/`. Without it, the first `git add .` would have staged ~35 .NET build artifacts and the local Claude Code settings file.
- **MapState uses `useState`, not `useRef`.** A ref would not trigger re-renders when the map is set, so `Sidebar` would not know to enable its buttons. State is the right primitive for "becomes available asynchronously, consumers must react."
- **`maplibre-gl` installed up front.** Needed to type the `Map` instance in the context. Adds the dep one task earlier than strictly necessary, but avoids `unknown` plumbing that would have to be replaced anyway.
- **No `dynamic({ ssr: false })`.** `MapView` is `"use client"` and only touches browser APIs inside `useEffect`. `maplibre-gl` v5 has no module-load-time `window` access. `next build` prerenders the tree without errors. If a future change moves browser code outside an effect, the fix is to wrap `MapView` in a thin client file that uses `dynamic` — `dynamic` with `ssr: false` cannot live in a Server Component (the page) in App Router.
- **`flyTo` hardcoded to `zoom: 10`.** Without it, `flyTo` keeps the current zoom (initial `1`), so clicking a sidebar button would just slide the globe sideways instead of focusing the location.
- **Sidebar buttons disabled while `map` is `null`.** Avoids silent no-op clicks during the brief window between SSR and the mount effect creating the map.
- **Provider moved between layout and page.** Initially placed in `app/layout.tsx` so `useMapState` would resolve when `MapView` started using it. Later moved to `app/page.tsx` per explicit instruction; both files are committed in their final state.

## Not implemented

- Frontend only consumes page 1 — cursor pagination is built in the API but unused.
- No loading or error UI on the fetch.
- No tests.
