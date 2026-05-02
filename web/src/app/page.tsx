import MapView from "@/components/MapView";
import Sidebar from "@/components/Sidebar";
import { MapStateProvider } from "@/context/MapStateContext";
import type { Location } from "@/types/location";

type LocationsResponse = {
  metadata: {
    totalCount: number;
    hasNextPage: boolean;
    nextCursor: string | null;
  };
  payload: {
    items: Location[];
  };
};

const API_BASE_URL = process.env.API_BASE_URL ?? "http://localhost:5000";

async function fetchAllLocations(): Promise<Location[]> {
  const all: Location[] = [];
  let cursor: string | null = null;
  do {
    const url = new URL(`${API_BASE_URL}/api/locations`);
    if (cursor) url.searchParams.set("cursor", cursor);
    const res = await fetch(url.toString(), { cache: "no-store" });
    const data: LocationsResponse = await res.json();
    all.push(...data.payload.items);
    cursor = data.metadata.nextCursor;
  } while (cursor !== null);
  return all;
}

export default async function Home() {
  const items = await fetchAllLocations();

  return (
    <MapStateProvider>
      <div className="flex h-screen">
        <Sidebar locations={items} />
        <div className="flex-1">
          <MapView locations={items} />
        </div>
      </div>
    </MapStateProvider>
  );
}
