import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import {
  TrendingUp, TrendingDown, Scale, ArrowDownCircle, ArrowUpCircle,
  FileSpreadsheet, Printer,
} from "lucide-react";
import api from "../lib/api";
import type { Comparativa } from "../types";
import { soles, cn } from "../lib/utils";

function hoyISO() { return new Date().toISOString().slice(0, 10); }
function inicioMesISO() { const d = new Date(); return new Date(d.getFullYear(), d.getMonth(), 1).toISOString().slice(0, 10); }

export default function ComparativaPage() {
  const [desde, setDesde] = useState(inicioMesISO());
  const [hasta, setHasta] = useState(hoyISO());

  const { data, isLoading } = useQuery({
    queryKey: ["comparativa", desde, hasta],
    queryFn: async () => (await api.get<Comparativa>("/comparativa", { params: { desde, hasta } })).data,
  });

  const balance = data?.balance ?? 0;
  const positivo = balance >= 0;

  const exportarCsv = () => {
    if (!data) return;
    const head = ["Tipo", "Fecha", "Descripción", "Detalle", "Monto", "Usuario"];
    const rows = data.movimientos.map((m) => [
      m.tipo, new Date(m.fecha).toLocaleDateString("es-PE"), m.descripcion, m.detalle, m.monto.toFixed(2), m.usuario,
    ]);
    const csv = [head, ...rows].map((r) => r.map((c) => `"${String(c).replace(/"/g, '""')}"`).join(",")).join("\n");
    const blob = new Blob(["﻿" + csv], { type: "text/csv;charset=utf-8;" });
    const a = document.createElement("a");
    a.href = URL.createObjectURL(blob);
    a.download = `comparativa_${desde}_a_${hasta}.csv`;
    a.click();
  };

  return (
    <div className="space-y-5">
      {/* Encabezado + filtros */}
      <div className="flex flex-wrap items-end justify-between gap-3 print:hidden">
        <div>
          <h1 className="text-xl font-semibold tracking-tight text-slate-800">Comparativa de Ingresos y Egresos</h1>
          <p className="text-sm text-slate-500">Tesorería · ingresos (tickets) frente a egresos (comprobantes)</p>
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
          <button onClick={() => window.print()} className="flex h-10 items-center gap-2 rounded-lg bg-cyan-800 px-3 text-sm font-medium text-white hover:bg-cyan-900"><Printer size={16} /> Imprimir</button>
        </div>
      </div>

      {/* Tarjetas resumen */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <Stat icon={<TrendingUp size={20} className="text-white" />} color="bg-emerald-500"
          label="Total ingresos" value={soles(data?.totalIngresos ?? 0)} sub={`${data?.cantidadIngresos ?? 0} tickets`} />
        <Stat icon={<TrendingDown size={20} className="text-white" />} color="bg-rose-500"
          label="Total egresos" value={soles(data?.totalEgresos ?? 0)} sub={`${data?.cantidadEgresos ?? 0} comprobantes`} />
        <Stat icon={<Scale size={20} className="text-white" />} color={positivo ? "bg-indigo-500" : "bg-amber-500"}
          label="Balance" value={soles(balance)} sub={positivo ? "Superávit" : "Déficit"} valueClass={positivo ? "text-emerald-600" : "text-rose-600"} />
        <Stat icon={<Scale size={20} className="text-white" />} color="bg-slate-500"
          label="Margen" value={data && data.totalIngresos > 0 ? `${((balance / data.totalIngresos) * 100).toFixed(1)}%` : "—"} sub="Balance / ingresos" />
      </div>

      {/* Gráfico de barras ingresos vs egresos por día */}
      <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-card">
        <div className="mb-4 flex items-center justify-between">
          <p className="text-sm font-semibold text-slate-700">Ingresos vs Egresos por día</p>
          <div className="flex items-center gap-4 text-xs">
            <span className="flex items-center gap-1.5 text-slate-500"><span className="h-2.5 w-2.5 rounded-sm bg-emerald-500" /> Ingresos</span>
            <span className="flex items-center gap-1.5 text-slate-500"><span className="h-2.5 w-2.5 rounded-sm bg-rose-500" /> Egresos</span>
          </div>
        </div>
        {isLoading ? (
          <p className="py-10 text-center text-sm text-slate-400">Cargando…</p>
        ) : (
          <BarChart serie={data?.serie ?? []} />
        )}
      </div>

      {/* Desglose: egresos por categoría / ingresos por concepto */}
      <div className="grid gap-4 lg:grid-cols-2 print:hidden">
        <Desglose titulo="Egresos por categoría" filas={data?.egresosPorCategoria ?? []} color="bg-rose-500" total={data?.totalEgresos ?? 0} />
        <Desglose titulo="Ingresos por concepto" filas={data?.ingresosPorConcepto ?? []} color="bg-emerald-500" total={data?.totalIngresos ?? 0} />
      </div>

      {/* Movimientos unificados */}
      <div className="overflow-x-auto rounded-2xl border border-slate-200 bg-white p-5 shadow-card">
        <p className="mb-3 text-sm font-semibold text-slate-700">Últimos movimientos</p>
        <table className="w-full text-sm">
          <thead>
            <tr className="text-left text-xs uppercase text-slate-400">
              <th className="pb-2">Tipo</th><th>Fecha</th><th>Descripción</th><th>Detalle</th><th className="text-right">Monto</th><th>Usuario</th>
            </tr>
          </thead>
          <tbody>
            {data?.movimientos.map((m, idx) => (
              <tr key={idx} className="border-t border-slate-100">
                <td className="py-2">
                  <span className={cn("inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium",
                    m.tipo === "Ingreso" ? "bg-emerald-50 text-emerald-700" : "bg-rose-50 text-rose-700")}>
                    {m.tipo === "Ingreso" ? <ArrowUpCircle size={12} /> : <ArrowDownCircle size={12} />} {m.tipo}
                  </span>
                </td>
                <td className="text-slate-500">{new Date(m.fecha).toLocaleDateString("es-PE")}</td>
                <td className="font-medium text-slate-700">{m.descripcion}</td>
                <td className="text-slate-500">{m.detalle}</td>
                <td className={cn("text-right font-semibold", m.tipo === "Ingreso" ? "text-emerald-600" : "text-rose-600")}>
                  {m.tipo === "Ingreso" ? "+" : "−"}{soles(m.monto)}
                </td>
                <td className="text-slate-500">{m.usuario}</td>
              </tr>
            ))}
            {data && data.movimientos.length === 0 && (
              <tr><td colSpan={6} className="py-4 text-center text-slate-400">Sin movimientos en el periodo</td></tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function Stat({ icon, color, label, value, sub, valueClass }: {
  icon: React.ReactNode; color: string; label: string; value: string; sub?: string; valueClass?: string;
}) {
  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-card">
      <div className={cn("mb-3 grid h-11 w-11 place-items-center rounded-xl", color)}>{icon}</div>
      <p className="text-xs text-slate-500">{label}</p>
      <p className={cn("text-xl font-bold text-slate-800", valueClass)}>{value}</p>
      {sub && <p className="mt-0.5 text-xs text-slate-400">{sub}</p>}
    </div>
  );
}

/** Gráfico de barras agrupadas (SVG): ingresos y egresos por día. */
function BarChart({ serie }: { serie: { fecha: string; ingresos: number; egresos: number }[] }) {
  if (serie.length === 0) return <p className="py-10 text-center text-sm text-slate-400">Sin datos en el periodo</p>;

  const W = 760, H = 220, padL = 44, padB = 26, padT = 10;
  const max = Math.max(1, ...serie.map((d) => Math.max(d.ingresos, d.egresos)));
  const innerW = W - padL - 8;
  const innerH = H - padB - padT;
  const grupo = innerW / serie.length;
  const barW = Math.min(14, grupo / 2.6);
  const y = (v: number) => padT + innerH - (v / max) * innerH;

  return (
    <svg viewBox={`0 0 ${W} ${H}`} className="w-full" preserveAspectRatio="xMidYMid meet">
      {/* Ejes guía */}
      {[0, 0.25, 0.5, 0.75, 1].map((t) => (
        <g key={t}>
          <line x1={padL} y1={padT + innerH * (1 - t)} x2={W - 8} y2={padT + innerH * (1 - t)} stroke="#e2e8f0" strokeWidth={1} />
          <text x={padL - 6} y={padT + innerH * (1 - t) + 3} textAnchor="end" className="fill-slate-400 text-[9px]">{(max * t).toFixed(0)}</text>
        </g>
      ))}
      {serie.map((d, i) => {
        const cx = padL + i * grupo + grupo / 2;
        return (
          <g key={i}>
            <rect x={cx - barW - 1} y={y(d.ingresos)} width={barW} height={padT + innerH - y(d.ingresos)} rx={2} className="fill-emerald-500" />
            <rect x={cx + 1} y={y(d.egresos)} width={barW} height={padT + innerH - y(d.egresos)} rx={2} className="fill-rose-500" />
            {serie.length <= 18 && (
              <text x={cx} y={H - 8} textAnchor="middle" className="fill-slate-400 text-[8px]">{d.fecha.slice(5)}</text>
            )}
          </g>
        );
      })}
    </svg>
  );
}

function Desglose({ titulo, filas, color, total }: {
  titulo: string; filas: { nombre: string; cantidad: number; monto: number }[]; color: string; total: number;
}) {
  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-card">
      <p className="mb-3 text-sm font-semibold text-slate-700">{titulo}</p>
      <div className="space-y-2.5">
        {filas.map((f, i) => {
          const pct = total > 0 ? (f.monto / total) * 100 : 0;
          return (
            <div key={i}>
              <div className="mb-1 flex items-center justify-between text-sm">
                <span className="text-slate-600">{f.nombre} <span className="text-xs text-slate-400">({f.cantidad})</span></span>
                <span className="font-medium text-slate-700">{soles(f.monto)}</span>
              </div>
              <div className="h-2 overflow-hidden rounded-full bg-slate-100">
                <div className={cn("h-full rounded-full", color)} style={{ width: `${Math.max(2, pct)}%` }} />
              </div>
            </div>
          );
        })}
        {filas.length === 0 && <p className="py-2 text-sm text-slate-400">Sin datos</p>}
      </div>
    </div>
  );
}
