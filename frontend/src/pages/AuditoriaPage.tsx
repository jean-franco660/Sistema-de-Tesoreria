import { useQuery } from "@tanstack/react-query";
import { ShieldCheck } from "lucide-react";
import api, { apiError } from "../lib/api";
import type { AuditLog } from "../types";

export default function AuditoriaPage() {
  const { data, isLoading, error } = useQuery({
    queryKey: ["auditoria"],
    queryFn: async () => (await api.get<AuditLog[]>("/auditoria")).data,
    retry: false,
  });

  return (
    <div className="space-y-5">
      <div>
        <h1 className="text-xl font-semibold tracking-tight text-slate-800">Auditoría</h1>
        <p className="text-sm text-slate-500">Registro de acciones de los usuarios del sistema</p>
      </div>

      {error && <p className="rounded-lg bg-rose-50 px-4 py-2 text-sm text-rose-600">{apiError(error)}</p>}

      <div className="overflow-x-auto rounded-2xl border border-slate-200 bg-white p-5 shadow-card">
        <table className="w-full text-sm">
          <thead>
            <tr className="text-left text-xs uppercase text-slate-400">
              <th className="pb-2">Fecha y hora</th><th>Usuario</th><th>IP</th><th>Acción</th><th>Detalle</th>
            </tr>
          </thead>
          <tbody>
            {isLoading && <tr><td colSpan={5} className="py-3 text-slate-400">Cargando…</td></tr>}
            {data?.map((l) => (
              <tr key={l.id} className="border-t border-slate-100">
                <td className="py-2 text-slate-500">{new Date(l.fecha).toLocaleString("es-PE")}</td>
                <td className="font-medium text-slate-700">{l.usuario}</td>
                <td className="font-mono text-slate-400">{l.ip}</td>
                <td>
                  <span className="inline-flex items-center gap-1.5 rounded-md bg-primary-soft px-2 py-0.5 text-xs font-medium text-cyan-800">
                    <ShieldCheck size={13} /> {l.accion}
                  </span>
                </td>
                <td className="text-slate-500">{l.detalle}</td>
              </tr>
            ))}
            {data && data.length === 0 && <tr><td colSpan={5} className="py-3 text-center text-slate-400">Sin registros</td></tr>}
          </tbody>
        </table>
      </div>
    </div>
  );
}
