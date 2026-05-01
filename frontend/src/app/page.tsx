import MapView, { type Location } from "@/components/MapView";

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

  return <MapView items={items} />;
}
