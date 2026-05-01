"use client";

import { useMapState } from "@/context/MapStateContext";
import type { Location } from "./MapView";

type SidebarProps = {
  locations: Location[];
};

export default function Sidebar({ locations }: SidebarProps) {
  const { map } = useMapState();

  return (
    <aside className="w-64 h-screen overflow-y-auto border-r p-4">
      <h2 className="text-lg font-semibold mb-2">Locations</h2>
      <ul className="space-y-1">
        {locations.map((loc) => (
          <li key={loc.id}>
            <button
              type="button"
              disabled={!map}
              onClick={() =>
                map?.flyTo({ center: [loc.lng, loc.lat], zoom: 10 })
              }
              className="w-full text-left px-2 py-1 rounded hover:bg-gray-100 disabled:opacity-50"
            >
              {loc.name}
            </button>
          </li>
        ))}
      </ul>
    </aside>
  );
}
