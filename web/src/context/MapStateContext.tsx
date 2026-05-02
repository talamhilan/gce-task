"use client";

import { createContext, useContext, useState, type ReactNode } from "react";
import type { Map as MapLibreMap } from "maplibre-gl";

type MapStateContextValue = {
  map: MapLibreMap | null;
  setMap: (map: MapLibreMap | null) => void;
};

const MapStateContext = createContext<MapStateContextValue | undefined>(
  undefined,
);

export function MapStateProvider({ children }: { children: ReactNode }) {
  const [map, setMap] = useState<MapLibreMap | null>(null);
  return (
    <MapStateContext.Provider value={{ map, setMap }}>
      {children}
    </MapStateContext.Provider>
  );
}

export function useMapState() {
  const ctx = useContext(MapStateContext);
  if (ctx === undefined) {
    throw new Error("useMapState must be used within a MapStateProvider");
  }
  return ctx;
}
