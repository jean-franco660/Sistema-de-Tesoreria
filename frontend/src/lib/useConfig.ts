import { useQuery } from "@tanstack/react-query";
import api from "./api";
import type { Configuracion } from "../types";

/** Configuración institucional (logo, nombre, base legal…) compartida en toda la app. */
export function useConfig() {
  const { data } = useQuery({
    queryKey: ["config"],
    queryFn: async () => (await api.get<Configuracion>("/configuracion")).data,
    staleTime: 5 * 60 * 1000,
    retry: false,
  });
  return data;
}

/** Logo a usar: el subido en Configuración o el por defecto. */
export function logoSrc(config?: Configuracion | null) {
  return config?.logoBase64 && config.logoBase64.length > 0 ? config.logoBase64 : "/logo.avif";
}
