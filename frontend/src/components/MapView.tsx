"use client";

import { useEffect, useRef } from "react";
import maplibregl from "maplibre-gl";
import "maplibre-gl/dist/maplibre-gl.css";
import { useMapState } from "@/context/MapStateContext";

export type Location = {
  id: string;
  lat: number;
  lng: number;
  name: string;
  status: string;
};

type MapViewProps = {
  locations: Location[];
};

export default function MapView({ locations }: MapViewProps) {
  const containerRef = useRef<HTMLDivElement | null>(null);
  const { map, setMap } = useMapState();

  useEffect(() => {
    if (!containerRef.current) return;

    const instance = new maplibregl.Map({
      container: containerRef.current,
      style: "https://demotiles.maplibre.org/style.json",
      center: [0, 20],
      zoom: 2,
    });

    setMap(instance);

    return () => {
      instance.remove();
      setMap(null);
    };
  }, [setMap]);

  useEffect(() => {
    if (!map) return;

    const markers: maplibregl.Marker[] = [];
    let cancelled = false;

    const addMarkers = () => {
      if (cancelled) return;
      for (const loc of locations) {
        const marker = new maplibregl.Marker()
          .setLngLat([loc.lng, loc.lat])
          .setPopup(new maplibregl.Popup().setText(loc.name))
          .addTo(map);
        markers.push(marker);
      }
    };

    if (map.loaded()) {
      addMarkers();
    } else {
      map.once("load", addMarkers);
    }

    return () => {
      cancelled = true;
      map.off("load", addMarkers);
      markers.forEach((m) => m.remove());
    };
  }, [map, locations]);

  return <div ref={containerRef} className="w-full h-screen" />;
}
