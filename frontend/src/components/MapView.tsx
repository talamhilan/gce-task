"use client";

export type Location = {
  id: string;
  lat: number;
  lng: number;
  name: string;
  status: string;
};

type MapViewProps = {
  items: Location[];
};

export default function MapView({ items }: MapViewProps) {
  return (
    <div className="p-4">
      <h2 className="text-lg font-semibold mb-2">Locations ({items.length})</h2>
      <ul className="space-y-1 text-sm">
        {items.map((item) => (
          <li key={item.id}>
            {item.name} — {item.lat}, {item.lng} ({item.status})
          </li>
        ))}
      </ul>
    </div>
  );
}
