import { useState, type ReactNode } from "react";
import { useQuery } from "@tanstack/react-query";
import { Printer, FileSpreadsheet, Coins, Receipt, ListChecks } from "lucide-react";
import api from "../lib/api";
import type { ReporteIngresos } from "../types";
import { soles } from "../lib/utils";

function hoyISO() { return new Date().toISOString().slice(0, 10); }
function inicioMesISO() { const d = new Date(); return new Date(d.getFullYear(), d.getMonth(), 1).toISOString().slice(0, 10); }

export default function ReportesPage() {
  const [desde, setDesde] = useState(inicioMesISO());
  const [hasta, setHasta] = useState(hoyISO());

  const { data, isLoading } = useQuery({
    queryKey: ["reporte-ingresos", desde, hasta],
    queryFn: async () => (await api.get<ReporteIngresos>("/reportes/ingresos", { params: { desde, hasta } })).data,
  });

  const exportarCsv = () => {
    if (!data) return;
    const head = ["Fecha", "Ticket", "Contador", "DNI", "Estudiante", "Programa", "Concepto", "Importe", "Usuario"];
    const rows = data.items.map((i) => [
      new Date(i.fecha).toLocaleString("es-PE"), i.numeroTicket, i.contador, i.dni,
      i.alumno, i.programa, i.servicio, i.importe.toFixed(2), i.usuario,
    ]);
    const csv = [head, ...rows].map((r) => r.map((c) => `"${String(c).replace(/"/g, '""')}"`).join(",")).join("\n");
    const blob = new Blob(["﻿" + csv], { type: "text/csv;charset=utf-8;" });
    const a = document.createElement("a");
    a.href = URL.createObjectURL(blob);
    a.download = `ingresos_${desde}_a_${hasta}.csv`;
    a.click();
  };

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-end justify-between gap-3 print:hidden">
        <div>
          <h1 className="text-xl font-semibold tracking-tight text-slate-800">Informe de ingresos</h1>
          <p className="text-sm text-slate-500">Contabilidad de recursos propios y actividades productivas</p>
        </div>
        <div className="flex items-end gap-2">
          <div>
            <label className="mb-1 block text-xs font-medium text-slate-600">Desde</label>
            <input type="date" value={desde} onChange={(e) => setDesde(e.target.value)} className="h-10 rounded-lg border border-slate-300 px-3 text-sm" />
          </div>
          <div>
            <label className="mb-1 block text-xs font-medium text-slate-600">Hasta</label>
            <input type="date" value={hasta} onChange={(e) => setHasta(e.target.value)} className="h-10 rounded-lg border border-slate-300 px-3 text-sm" />
          </div>
          <button onClick={exportarCsv} className="flex h-10 items-center gap-2 rounded-lg border border-slate-300 bg-white px-3 text-sm hover:bg-slate-50"><FileSpreadsheet size={16} /> Excel</button>
          <button onClick={() => window.print()} className="flex h-10 items-center gap-2 rounded-lg bg-cyan-800 px-3 text-sm font-medium text-white hover:bg-cyan-800"><Printer size={16} /> Imprimir</button>
        </div>
      </div>

      {/* Encabezado del informe (visible al imprimir) */}
      <div className="hidden text-center print:block">
        <h2 className="text-lg font-bold">IEST "HORACIO ZEBALLOS GÁMEZ" — JULIACA</h2>
        <p className="text-sm">Informe de Ingresos por Recursos Propios y Actividades Productivas</p>
        <p className="text-xs">Del {desde} al {hasta}</p>
      </div>

      {/* Tarjetas resumen */}
      <div className="grid gap-4 sm:grid-cols-3">
        <Resumen icon={<Coins className="text-white" size={20} />} color="bg-violet-500" label="Total recaudado" value={soles(data?.total ?? 0)} />
        <Resumen icon={<Receipt className="text-white" size={20} />} color="bg-emerald-500" label="Tickets" value={`${data?.cantidadTickets ?? 0}`} />
        <Resumen icon={<ListChecks className="text-white" size={20} />} color="bg-blue-500" label="Conceptos cobrados" value={`${data?.cantidadServicios ?? 0}`} />
      </div>

      {/* Detalle */}
      <div className="overflow-x-auto rounded-2xl border border-slate-200 bg-white p-5 shadow-card">
        <p className="mb-3 text-sm font-semibold text-slate-700">Detalle de ingresos</p>
        <table className="w-full text-sm">
          <thead>
            <tr className="text-left text-xs uppercase text-slate-400">
              <th className="pb-2">Fecha</th><th>Ticket</th><th>DNI</th><th>Estudiante</th><th>Programa</th><th>Concepto</th><th className="text-right">Importe</th><th>Usuario</th>
            </tr>
          </thead>
          <tbody>
            {isLoading && <tr><td colSpan={8} className="py-3 text-slate-400">Cargando…</td></tr>}
            {data?.items.map((i, idx) => (
              <tr key={idx} className="border-t border-slate-100">
                <td className="py-2 text-slate-500">{new Date(i.fecha).toLocaleDateString("es-PE")}</td>
                <td className="font-mono font-medium text-slate-700">{i.numeroTicket}</td>
                <td className="font-mono text-slate-500">{i.dni}</td>
                <td className="text-slate-600">{i.alumno}</td>
                <td className="text-slate-500">{i.programa}</td>
                <td className="text-slate-600">{i.servicio}</td>
                <td className="text-right font-semibold text-emerald-600">{soles(i.importe)}</td>
                <td className="text-slate-500">{i.usuario}</td>
              </tr>
            ))}
            {data && data.items.length === 0 && <tr><td colSpan={8} className="py-3 text-center text-slate-400">Sin ingresos en el periodo</td></tr>}
          </tbody>
          {data && data.items.length > 0 && (
            <tfoot>
              <tr className="border-t-2 border-slate-200 font-bold">
                <td className="py-2" colSpan={6}>TOTAL</td>
                <td className="text-right text-cyan-800">{soles(data.total)}</td>
                <td></td>
              </tr>
            </tfoot>
          )}
        </table>
      </div>

      {/* Resúmenes por servicio / programa */}
      <div className="grid gap-4 lg:grid-cols-2 print:hidden">
        <SubTabla titulo="Por concepto" filas={data?.resumenPorServicio ?? []} />
        <SubTabla titulo="Por programa" filas={data?.resumenPorPrograma ?? []} />
      </div>
    </div>
  );
}

function Resumen({ icon, color, label, value }: { icon: ReactNode; color: string; label: string; value: string }) {
  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-card">
      <div className={`mb-3 grid h-11 w-11 place-items-center rounded-xl ${color}`}>{icon}</div>
      <p className="text-xs text-slate-500">{label}</p>
      <p className="text-xl font-bold text-slate-800">{value}</p>
    </div>
  );
}

function SubTabla({ titulo, filas }: { titulo: string; filas: { nombre: string; cantidad: number; monto: number }[] }) {
  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-card">
      <p className="mb-3 text-sm font-semibold text-slate-700">{titulo}</p>
      <table className="w-full text-sm">
        <tbody>
          {filas.map((f, i) => (
            <tr key={i} className="border-t border-slate-100">
              <td className="py-1.5 text-slate-600">{f.nombre}</td>
              <td className="text-right text-slate-400">{f.cantidad}</td>
              <td className="text-right font-medium text-slate-700">{soles(f.monto)}</td>
            </tr>
          ))}
          {filas.length === 0 && <tr><td className="py-2 text-slate-400">Sin datos</td></tr>}
        </tbody>
      </table>
    </div>
  );
}
