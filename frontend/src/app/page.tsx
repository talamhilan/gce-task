import MapView, { type Location } from "@/components/MapView";
import Sidebar from "@/components/Sidebar";
import { MapStateProvider } from "@/context/MapStateContext";

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

export default async function Home() {
  const res = await fetch("http://localhost:5000/api/locations", {
    cache: "no-store",
  });
  const data: LocationsResponse = await res.json();
  const items = data.payload.items;

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
