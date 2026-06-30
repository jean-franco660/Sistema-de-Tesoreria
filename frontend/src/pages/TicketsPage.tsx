import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Printer, ArrowLeft, Ticket as TicketIcon, Coins, XCircle, Calculator } from "lucide-react";
import api from "../lib/api";
import type { Ticket, TicketListItem } from "../types";
import ThermalTicket from "../components/ThermalTicket";
import { soles } from "../lib/utils";
import { StatCard } from "../components/ui/stat";

export default function TicketsPage() {
  const [sel, setSel] = useState<Ticket | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ["tickets"],
    queryFn: async () => (await api.get<TicketListItem[]>("/tickets")).data,
  });

  const ver = async (id: number) => setSel((await api.get<Ticket>(`/tickets/${id}`)).data);

  const emitidos = data?.filter((t) => t.estado === "Emitido") ?? [];
  const anulados = data?.filter((t) => t.estado === "Anulado").length ?? 0;
  const recaudado = emitidos.reduce((s, t) => s + t.total, 0);
  const promedio = emitidos.length ? recaudado / emitidos.length : 0;

  if (sel) {
    return (
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <h1 className="text-xl font-bold text-slate-800">Ticket {sel.numeroTicket}</h1>
          <div className="flex gap-2">
            <button onClick={() => window.print()} className="flex items-center gap-2 rounded-lg bg-cyan-800 px-4 py-2 text-sm font-medium text-white hover:bg-cyan-700">
              <Printer size={16} /> Imprimir
            </button>
            <button onClick={() => setSel(null)} className="flex items-center gap-2 rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm hover:bg-slate-50">
              <ArrowLeft size={16} /> Volver
            </button>
          </div>
        </div>
        <div className="mx-auto w-fit rounded-2xl border border-slate-200 bg-white p-4 shadow-card"><ThermalTicket ticket={sel} /></div>
      </div>
    );
  }

  return (
    <div className="space-y-5">
      <div>
        <h1 className="text-xl font-semibold tracking-tight text-slate-800">Tickets</h1>
        <p className="text-sm text-slate-500">Gestiona y consulta todos los tickets emitidos</p>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard icon={<TicketIcon className="text-white" size={20} />} color="bg-blue-500" label="Total tickets" value={`${data?.length ?? 0}`} />
        <StatCard icon={<Coins className="text-white" size={20} />} color="bg-emerald-500" label="Total recaudado" value={soles(recaudado)} />
        <StatCard icon={<XCircle className="text-white" size={20} />} color="bg-rose-500" label="Tickets anulados" value={`${anulados}`} />
        <StatCard icon={<Calculator className="text-white" size={20} />} color="bg-violet-500" label="Ticket promedio" value={soles(promedio)} />
      </div>

      <div className="overflow-x-auto rounded-2xl border border-slate-200 bg-white p-5 shadow-card">
        <table className="w-full text-sm">
          <thead>
            <tr className="text-left text-xs uppercase text-slate-400">
              <th className="pb-2">N°</th><th>Contador</th><th>Fecha</th><th>Alumno</th>
              <th className="text-right">Total</th><th>Usuario</th><th>Estado</th><th></th>
            </tr>
          </thead>
          <tbody>
            {isLoading && <tr><td colSpan={8} className="py-3 text-slate-400">Cargando…</td></tr>}
            {data?.map((t) => (
              <tr key={t.id} className="border-t border-slate-100 hover:bg-slate-50">
                <td className="py-2 font-mono font-medium text-slate-700">{t.numeroTicket}</td>
                <td className="font-mono text-slate-400">{t.contador}</td>
                <td className="text-slate-500">{new Date(t.fechaEmision).toLocaleString("es-PE")}</td>
                <td className="text-slate-600">{t.alumnoNombre}</td>
                <td className="text-right font-semibold text-emerald-600">{soles(t.total)}</td>

                <td>
                  <span className={`rounded-md px-2 py-0.5 text-xs font-medium ${t.estado === "Emitido" ? "bg-emerald-50 text-emerald-600" : "bg-rose-50 text-rose-600"}`}>{t.estado}</span>
                </td>
                <td className="text-right">
                  <button onClick={() => ver(t.id)} className="rounded-md px-2 py-1 text-sm font-medium text-cyan-800 hover:bg-primary-soft">Ver</button>
                </td>
              </tr>
            ))}
            {data && data.length === 0 && (
              <tr><td colSpan={8} className="py-3 text-center text-slate-400">Sin tickets</td></tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
