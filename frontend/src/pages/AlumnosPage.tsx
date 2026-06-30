import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Search, Users } from "lucide-react";
import api from "../lib/api";
import type { Alumno } from "../types";

export default function AlumnosPage() {
  const [buscar, setBuscar] = useState("");
  const { data, isLoading } = useQuery({
    queryKey: ["alumnos", buscar],
    queryFn: async () => (await api.get<Alumno[]>(`/alumnos`, { params: { buscar: buscar || undefined } })).data,
  });

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight text-slate-800">Estudiantes</h1>
          <p className="text-sm text-slate-500">Padrón de alumnos matriculados</p>
        </div>
        <span className="flex items-center gap-2 rounded-lg bg-primary-soft px-3 py-2 text-sm font-medium text-primary">
          <Users size={16} /> {data?.length ?? 0} registrados
        </span>
      </div>

      <div className="relative max-w-sm">
        <Search className="pointer-events-none absolute left-3 top-2.5 text-slate-400" size={18} />
        <input
          placeholder="Buscar por DNI, nombre o apellido…"
          value={buscar}
          onChange={(e) => setBuscar(e.target.value)}
          className="h-10 w-full rounded-lg border border-slate-200 bg-white pl-10 pr-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
        />
      </div>

      <div className="overflow-x-auto rounded-2xl border border-slate-200 bg-white p-5 shadow-card">
        <table className="w-full text-sm">
          <thead>
            <tr className="text-left text-xs uppercase text-slate-400">
              <th className="pb-2">DNI</th><th>Apellidos y Nombres</th><th>Programa</th><th>Turno</th>
            </tr>
          </thead>
          <tbody>
            {isLoading && <tr><td colSpan={4} className="py-3 text-slate-400">Cargando…</td></tr>}
            {data?.map((a) => (
              <tr key={a.id} className="border-t border-slate-100 hover:bg-slate-50">
                <td className="py-2 font-mono text-slate-600">{a.dni}</td>
                <td className="font-medium text-slate-700">{a.nombreCompleto}</td>
                <td className="text-slate-500">{a.programa}</td>
                <td>
                  <span className="rounded-md bg-slate-100 px-2 py-0.5 text-xs text-slate-600">{a.turno}</span>
                </td>
              </tr>
            ))}
            {data && data.length === 0 && (
              <tr><td colSpan={4} className="py-3 text-center text-slate-400">Sin resultados</td></tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
