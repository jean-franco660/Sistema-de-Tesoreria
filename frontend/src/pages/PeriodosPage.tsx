import { useState, type ReactNode } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import {
  CalendarRange, Plus, Lock, FileText, Printer, X, Coins, Receipt, CalendarClock,
  ListChecks, FileSpreadsheet, ArrowLeft, Eye, ChevronDown, Users, CalendarDays,
} from "lucide-react";
import api, { apiError } from "../lib/api";
import type { Periodo, PeriodoDetalle, PeriodoResumen } from "../types";
import { useAuth } from "../context/AuthContext";
import { useConfig, logoSrc } from "../lib/useConfig";
import { soles } from "../lib/utils";

const PALETTE = ["#6366f1", "#10b981", "#f59e0b", "#3b82f6", "#ef4444", "#06b6d4", "#8b5cf6", "#ec4899"];

const TONES: Record<string, string> = {
  violet: "bg-violet-100 text-violet-600",
  emerald: "bg-emerald-100 text-emerald-600",
  blue: "bg-blue-100 text-blue-600",
  amber: "bg-amber-100 text-amber-600",
};

export default function PeriodosPage() {
  const qc = useQueryClient();
  const { isAdmin } = useAuth();
  const config = useConfig();
  const { data: periodos, isLoading } = useQuery({ queryKey: ["periodos"], queryFn: async () => (await api.get<Periodo[]>("/periodos")).data });
  const activo = periodos?.find((p) => p.estado === "Abierto");

  const [abrir, setAbrir] = useState(false);
  const [form, setForm] = useState({ nombre: "", fechaInicio: "", fechaFin: "", observaciones: "" });
  const [cierre, setCierre] = useState<PeriodoResumen | null>(null);
  const [acta, setActa] = useState<PeriodoResumen | null>(null);
  const [detalle, setDetalle] = useState<PeriodoDetalle | null>(null);
  const [expOpen, setExpOpen] = useState(false);
  const [msg, setMsg] = useState("");
  const [busy, setBusy] = useState(false);

  const refrescar = () => { qc.invalidateQueries({ queryKey: ["periodos"] }); qc.invalidateQueries({ queryKey: ["periodo-activo"] }); qc.invalidateQueries({ queryKey: ["dashboard-periodo"] }); };

  const guardarAbrir = async () => {
    setBusy(true); setMsg("");
    try { await api.post("/periodos", form); setAbrir(false); setForm({ nombre: "", fechaInicio: "", fechaFin: "", observaciones: "" }); refrescar(); }
    catch (e) { setMsg(apiError(e)); } finally { setBusy(false); }
  };

  const pedirCierre = async (p: Periodo) => {
    setMsg("");
    try { setCierre((await api.get<PeriodoResumen>(`/periodos/${p.id}/resumen`)).data); } catch (e) { setMsg(apiError(e)); }
  };

  const confirmarCierre = async () => {
    if (!cierre) return;
    setBusy(true);
    try { const { data } = await api.put<PeriodoResumen>(`/periodos/${cierre.id}/cerrar`); setCierre(null); setActa(data); refrescar(); }
    catch (e) { setMsg(apiError(e)); } finally { setBusy(false); }
  };

  const verDetalle = async (p: Periodo) => {
    setMsg("");
    try { setDetalle((await api.get<PeriodoDetalle>(`/periodos/${p.id}/detalle`)).data); } catch (e) { setMsg(apiError(e)); }
  };

  const exportarDetalle = () => {
    if (!detalle) return;
    setExpOpen(false);
    const head = ["Fecha", "Ticket", "DNI", "Estudiante", "Programa", "Concepto", "Importe", "Usuario"];
    const rows = detalle.items.map((i) => [new Date(i.fecha).toLocaleString("es-PE"), i.numeroTicket, i.dni, i.alumno, i.programa, i.servicio, i.importe.toFixed(2), i.usuario]);
    const csv = [head, ...rows].map((r) => r.map((c) => `"${String(c).replace(/"/g, '""')}"`).join(",")).join("\n");
    const a = document.createElement("a");
    a.href = URL.createObjectURL(new Blob(["﻿" + csv], { type: "text/csv;charset=utf-8;" }));
    a.download = `ingresos_${detalle.periodo.nombre}.csv`; a.click();
  };

  const fmt = (s?: string) => s ? new Date(s).toLocaleDateString("es-PE", { timeZone: "UTC" }) : "—";

  // ════════════════════ Detalle de ingresos del período ════════════════════
  if (detalle) {
    return (
      <div className="space-y-5">
        <div className="flex flex-wrap items-center justify-between gap-3 print:hidden">
          <div className="flex items-center gap-3">
            <button onClick={() => setDetalle(null)} className="grid h-9 w-9 place-items-center rounded-xl border border-slate-300 bg-white text-slate-500 hover:bg-slate-50"><ArrowLeft size={17} /></button>
            <div>
              <h1 className="flex flex-wrap items-center gap-2 text-xl font-bold tracking-tight text-slate-800">Ingresos del período · {detalle.periodo.nombre}
                <span className={`rounded-md px-2 py-0.5 text-xs font-medium ${detalle.periodo.estado === "Abierto" ? "bg-emerald-50 text-emerald-600" : "bg-slate-100 text-slate-500"}`}>{detalle.periodo.estado}</span>
              </h1>
              <p className="text-sm text-slate-500">{fmt(detalle.periodo.fechaInicio)} – {fmt(detalle.periodo.fechaFin)} · {detalle.items.length} registros · Total {soles(detalle.total)}</p>
            </div>
          </div>
          <div className="flex gap-2">
            <div className="relative">
              <button onClick={() => setExpOpen((v) => !v)} className="flex items-center gap-2 rounded-xl bg-emerald-50 px-3 py-2 text-sm font-medium text-emerald-700 hover:bg-emerald-100"><FileSpreadsheet size={16} /> Exportar <ChevronDown size={14} /></button>
              {expOpen && (
                <div className="absolute right-0 z-10 mt-1 w-44 overflow-hidden rounded-xl border border-slate-200 bg-white py-1 shadow-lg">
                  <button onClick={exportarDetalle} className="flex w-full items-center gap-2 px-3 py-2 text-left text-sm text-slate-600 hover:bg-slate-50"><FileSpreadsheet size={15} className="text-emerald-600" /> Excel (.csv)</button>
                </div>
              )}
            </div>
            <button onClick={() => window.print()} className="flex items-center gap-2 rounded-xl bg-gradient-to-r from-primary to-violet-600 px-3 py-2 text-sm font-medium text-white shadow-glow"><Printer size={16} /> Imprimir</button>
            <button onClick={() => setDetalle(null)} className="flex items-center gap-2 rounded-xl border border-slate-300 bg-white px-3 py-2 text-sm font-medium hover:bg-slate-50"><ArrowLeft size={16} /> Volver</button>
          </div>
        </div>

        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <MetricCard tone="emerald" icon={<Coins size={20} />} label="Ingresos totales" value={soles(detalle.total)} />
          <MetricCard tone="blue" icon={<Receipt size={20} />} label="Tickets" value={`${detalle.periodo.tickets}`} />
          <MetricCard tone="violet" icon={<ListChecks size={20} />} label="Conceptos cobrados" value={`${detalle.items.length}`} />
          <MetricCard tone="amber" icon={<Users size={20} />} label="Estudiantes" value={`${detalle.periodo.estudiantes}`} />
        </div>

        <div className="overflow-x-auto rounded-2xl border border-slate-200/70 bg-white p-5 shadow-card">
          <p className="mb-3 text-base font-semibold text-slate-800">Detalle de ingresos</p>
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-slate-200 text-left text-xs uppercase text-slate-400">
                <th className="px-2 py-2">Fecha</th><th className="px-2">Ticket</th><th className="px-2">Estudiante</th><th className="px-2">Programa</th><th className="px-2">Concepto</th><th className="px-2 text-right">Importe</th><th className="px-2">Usuario</th>
              </tr>
            </thead>
            <tbody>
              {detalle.items.map((i, idx) => (
                <tr key={idx} className="border-b border-slate-50 hover:bg-slate-50/60">
                  <td className="whitespace-nowrap px-2 py-2.5 text-slate-500">{new Date(i.fecha).toLocaleDateString("es-PE")}</td>
                  <td className="px-2 font-mono font-medium text-slate-700">{i.numeroTicket}</td>
                  <td className="px-2"><span className="text-slate-700">{i.alumno}</span><span className="block font-mono text-[11px] text-slate-400">DNI {i.dni}</span></td>
                  <td className="px-2 text-slate-500">{i.programa}</td>
                  <td className="px-2"><span className="rounded-md bg-slate-50 px-2 py-0.5 text-xs text-slate-600 ring-1 ring-slate-200">{i.servicio}</span></td>
                  <td className="whitespace-nowrap px-2 text-right font-semibold text-emerald-600">{soles(i.importe)}</td>
                  <td className="px-2 text-slate-500">{i.usuario}</td>
                </tr>
              ))}
              {detalle.items.length === 0 && <tr><td colSpan={7} className="py-4 text-center text-slate-400">Sin ingresos en este período</td></tr>}
            </tbody>
            <tfoot><tr className="border-t-2 border-slate-200 font-bold"><td className="px-2 py-2.5" colSpan={5}>TOTAL</td><td className="px-2 text-right text-primary">{soles(detalle.total)}</td><td /></tr></tfoot>
          </table>
        </div>

        <div className="grid gap-4 lg:grid-cols-3 print:hidden">
          <DonutCard title="Ingresos por concepto" rows={detalle.resumenPorServicio} />
          <DonutCard title="Ingresos por programa" rows={detalle.resumenPorPrograma} unidad="conceptos" />
          <DonutCard title="Ingresos por usuario" rows={detalle.resumenPorUsuario} unidad="tickets" />
        </div>
      </div>
    );
  }

  // ════════════════════ Acta de cierre (imprimible) ════════════════════
  if (acta) {
    return (
      <div className="space-y-4">
        <div className="flex items-center justify-between print:hidden">
          <h1 className="text-xl font-semibold tracking-tight text-slate-800">Acta de cierre · {acta.nombre}</h1>
          <div className="flex gap-2">
            <button onClick={() => window.print()} className="flex items-center gap-2 rounded-xl bg-gradient-to-r from-primary to-violet-600 px-4 py-2 text-sm font-medium text-white shadow-glow"><Printer size={16} /> Imprimir</button>
            <button onClick={() => setActa(null)} className="rounded-xl border border-slate-300 bg-white px-4 py-2 text-sm font-medium hover:bg-slate-50">Volver</button>
          </div>
        </div>
        <div id="ticket-print" className="mx-auto max-w-2xl rounded-2xl border border-slate-200/70 bg-white p-8 shadow-card">
          <div className="text-center">
            <img src={logoSrc(config)} alt="Logo" className="mx-auto h-16 w-auto object-contain" />
            <p className="mt-2 text-sm font-bold uppercase text-slate-800">{config?.nombreInstitucion}</p>
            <p className="text-xs text-slate-500">{config?.ciudad}</p>
            <h2 className="mt-4 text-lg font-bold tracking-wide text-slate-800">ACTA DE CIERRE DE INGRESOS</h2>
            <p className="text-xs text-slate-500">{config?.tituloComprobante}</p>
          </div>
          <div className="mx-auto mt-6 max-w-md space-y-2 text-sm">
            <Fila k="Período" v={acta.nombre} />
            <Fila k="Fecha de inicio" v={fmt(acta.fechaInicio)} />
            <Fila k="Fecha de fin" v={fmt(acta.fechaFin)} />
            <Fila k="Tickets emitidos" v={`${acta.tickets}`} />
            <Fila k="Estudiantes atendidos" v={`${acta.estudiantes}`} />
            <Fila k="Servicios procesados" v={`${acta.servicios}`} />
            <Fila k="Ingresos totales" v={soles(acta.ingresos)} bold />
            <Fila k="Usuario responsable" v={acta.usuarioCierre ?? "—"} />
            <Fila k="Fecha de cierre" v={acta.fechaCierre ? new Date(acta.fechaCierre).toLocaleString("es-PE") : "—"} />
          </div>
          <div className="mx-auto mt-12 w-56 border-t border-slate-400 pt-1 text-center text-xs text-slate-500">Firma del Tesorero</div>
        </div>
      </div>
    );
  }

  // ════════════════════ Vista principal ════════════════════
  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="flex items-start gap-3">
          <div className="grid h-11 w-11 shrink-0 place-items-center rounded-xl bg-gradient-to-br from-primary to-violet-600 text-white shadow-glow"><CalendarRange size={20} /></div>
          <div>
            <h1 className="text-xl font-bold tracking-tight text-slate-800">Períodos Académicos</h1>
            <p className="text-sm text-slate-500">Gestión financiera por período (apertura y cierre de caja)</p>
          </div>
        </div>
        {isAdmin && (
          <button onClick={() => setAbrir(true)} className="flex items-center gap-2 rounded-xl bg-gradient-to-r from-primary to-violet-600 px-4 py-2.5 text-sm font-semibold text-white shadow-glow transition hover:opacity-95"><Plus size={16} /> Nuevo período</button>
        )}
      </div>

      {msg && <p className="rounded-lg bg-rose-50 px-4 py-2 text-sm text-rose-600">{msg}</p>}

      {/* Tarjetas de estado */}
      {activo ? (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <MetricCard tone="violet" icon={<CalendarDays size={20} />} label="Período activo" value={activo.nombre}
            badge={<span className="rounded-md bg-emerald-50 px-2 py-0.5 text-[11px] font-medium text-emerald-600">Abierto</span>}
            hint={`${fmt(activo.fechaInicio)} – ${fmt(activo.fechaFin)}`} />
          <MetricCard tone="emerald" icon={<Coins size={20} />} label="Ingresos acumulados" value={soles(activo.ingresos)} hint="Total del período" />
          <MetricCard tone="blue" icon={<Receipt size={20} />} label="Tickets emitidos" value={`${activo.tickets}`} hint="En este período" />
          <MetricCard tone="amber" icon={<CalendarClock size={20} />} label="Días restantes" value={`${activo.diasRestantes ?? 0}`} hint="Para el cierre" />
        </div>
      ) : (
        <div className="rounded-2xl border border-amber-200 bg-amber-50 p-4 text-sm text-amber-700">
          ⚠️ No hay un período académico activo. {isAdmin ? "Abre uno para poder emitir tickets." : "Comuníquese con el administrador."}
        </div>
      )}

      {/* Histórico */}
      <div className="overflow-x-auto rounded-2xl border border-slate-200/70 bg-white p-5 shadow-card">
        <p className="mb-3 text-base font-semibold text-slate-800">Historial de períodos</p>
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-slate-200 text-left text-xs uppercase text-slate-400">
              <th className="px-2 pb-2">Período</th><th className="px-2">Inicio</th><th className="px-2">Fin</th><th className="px-2">Estado</th><th className="px-2 text-right">Tickets</th><th className="px-2 text-right">Ingresos</th><th className="px-2 text-right">Acciones</th>
            </tr>
          </thead>
          <tbody>
            {isLoading && <tr><td colSpan={7} className="py-3 text-slate-400">Cargando…</td></tr>}
            {periodos?.map((p) => (
              <tr key={p.id} className="border-b border-slate-50 hover:bg-slate-50/60">
                <td className="px-2 py-3"><span className="flex items-center gap-2 font-semibold text-slate-700"><span className={`h-2.5 w-2.5 rounded-full ${p.estado === "Abierto" ? "bg-primary" : "bg-slate-300"}`} /> {p.nombre}</span></td>
                <td className="px-2 text-slate-500">{fmt(p.fechaInicio)}</td>
                <td className="px-2 text-slate-500">{fmt(p.fechaFin)}</td>
                <td className="px-2"><span className={`rounded-md px-2 py-0.5 text-xs font-medium ${p.estado === "Abierto" ? "bg-emerald-50 text-emerald-600" : "bg-slate-100 text-slate-500"}`}>{p.estado}</span></td>
                <td className="px-2 text-right text-slate-600">{p.tickets}</td>
                <td className="px-2 text-right font-semibold text-emerald-600">{soles(p.ingresos)}</td>
                <td className="px-2 text-right">
                  <div className="inline-flex items-center gap-1.5">
                    <button onClick={() => verDetalle(p)} className="inline-flex items-center gap-1 rounded-lg border border-slate-200 px-2.5 py-1.5 text-xs font-medium text-slate-600 hover:border-primary hover:bg-primary-soft hover:text-primary"><Eye size={13} /> Ver ingresos</button>
                    {p.estado === "Abierto" && isAdmin && (
                      <button onClick={() => pedirCierre(p)} className="inline-flex items-center gap-1 rounded-lg border border-rose-200 bg-rose-50 px-2.5 py-1.5 text-xs font-medium text-rose-600 hover:bg-rose-100"><Lock size={13} /> Cerrar</button>
                    )}
                    {p.estado === "Cerrado" && (
                      <button onClick={() => api.get<PeriodoResumen>(`/periodos/${p.id}/resumen`).then((r) => setActa(r.data))} className="inline-flex items-center gap-1 rounded-lg border border-slate-200 px-2.5 py-1.5 text-xs font-medium text-slate-600 hover:bg-slate-100"><FileText size={13} /> Acta</button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
            {periodos && periodos.length === 0 && <tr><td colSpan={7} className="py-3 text-center text-slate-400">Sin períodos</td></tr>}
          </tbody>
        </table>
      </div>

      {/* Modal abrir */}
      {abrir && (
        <div className="fixed inset-0 z-50 grid place-items-center bg-slate-900/40 p-4 backdrop-blur-sm" onClick={() => setAbrir(false)}>
          <div className="w-full max-w-md rounded-2xl bg-white p-6 shadow-xl" onClick={(e) => e.stopPropagation()}>
            <div className="mb-4 flex items-center justify-between"><h3 className="text-lg font-semibold text-slate-800">Nuevo período académico</h3><button onClick={() => setAbrir(false)} className="text-slate-400 hover:text-slate-600"><X size={18} /></button></div>
            {msg && <p className="mb-3 rounded-lg bg-rose-50 px-3 py-2 text-sm text-rose-600">{msg}</p>}
            <label className="mb-1 block text-sm font-medium text-slate-600">Nombre (ej. 2026-II)</label>
            <input autoFocus value={form.nombre} onChange={(e) => setForm({ ...form, nombre: e.target.value })} className="mb-3 h-10 w-full rounded-lg border border-slate-300 px-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30" />
            <div className="mb-3 grid grid-cols-2 gap-3">
              <div><label className="mb-1 block text-sm font-medium text-slate-600">Inicio</label><input type="date" value={form.fechaInicio} onChange={(e) => setForm({ ...form, fechaInicio: e.target.value })} className="h-10 w-full rounded-lg border border-slate-300 px-3 text-sm" /></div>
              <div><label className="mb-1 block text-sm font-medium text-slate-600">Fin</label><input type="date" value={form.fechaFin} onChange={(e) => setForm({ ...form, fechaFin: e.target.value })} className="h-10 w-full rounded-lg border border-slate-300 px-3 text-sm" /></div>
            </div>
            <label className="mb-1 block text-sm font-medium text-slate-600">Observaciones</label>
            <textarea value={form.observaciones} onChange={(e) => setForm({ ...form, observaciones: e.target.value })} rows={2} className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30" />
            <div className="mt-5 flex justify-end gap-2">
              <button onClick={() => setAbrir(false)} className="rounded-lg border border-slate-300 px-4 py-2 text-sm hover:bg-slate-50">Cancelar</button>
              <button onClick={guardarAbrir} disabled={!form.nombre || !form.fechaInicio || !form.fechaFin || busy} className="rounded-lg bg-primary px-4 py-2 text-sm font-medium text-white hover:bg-primary-hover disabled:opacity-50">{busy ? "Abriendo…" : "Abrir período"}</button>
            </div>
          </div>
        </div>
      )}

      {/* Modal confirmar cierre */}
      {cierre && (
        <div className="fixed inset-0 z-50 grid place-items-center bg-slate-900/40 p-4 backdrop-blur-sm" onClick={() => setCierre(null)}>
          <div className="w-full max-w-md rounded-2xl bg-white p-6 shadow-xl" onClick={(e) => e.stopPropagation()}>
            <h3 className="text-lg font-semibold text-slate-800">Cerrar período {cierre.nombre}</h3>
            <p className="mt-1 text-sm text-slate-500">Revise el resumen antes de cerrar definitivamente.</p>
            <div className="mt-4 space-y-2 rounded-xl bg-slate-50 p-4 text-sm">
              <Fila k="Tickets emitidos" v={`${cierre.tickets}`} />
              <Fila k="Estudiantes atendidos" v={`${cierre.estudiantes}`} />
              <Fila k="Servicios registrados" v={`${cierre.servicios}`} />
              <Fila k="Ingresos totales" v={soles(cierre.ingresos)} bold />
            </div>
            <p className="mt-3 text-sm font-medium text-rose-600">¿Desea cerrar definitivamente el período académico? No se podrá modificar.</p>
            <div className="mt-5 flex justify-end gap-2">
              <button onClick={() => setCierre(null)} className="rounded-lg border border-slate-300 px-4 py-2 text-sm hover:bg-slate-50">Cancelar</button>
              <button onClick={confirmarCierre} disabled={busy} className="flex items-center gap-2 rounded-lg bg-rose-600 px-4 py-2 text-sm font-medium text-white hover:bg-rose-700 disabled:opacity-50"><Lock size={15} /> {busy ? "Cerrando…" : "Cerrar definitivamente"}</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

function MetricCard({ tone, icon, label, value, badge, hint }: { tone: keyof typeof TONES; icon: ReactNode; label: string; value: string; badge?: ReactNode; hint?: string }) {
  return (
    <div className="rounded-2xl border border-slate-200/70 bg-white p-4 shadow-card transition-shadow hover:shadow-soft">
      <div className="flex items-start gap-3">
        <div className={`grid h-11 w-11 shrink-0 place-items-center rounded-xl ${TONES[tone]}`}>{icon}</div>
        <div className="min-w-0">
          <p className="text-[11px] font-medium text-slate-400">{label}</p>
          <div className="flex flex-wrap items-center gap-2">
            <p className="truncate text-lg font-bold text-slate-800">{value}</p>
            {badge}
          </div>
          {hint && <p className="mt-0.5 truncate text-[11px] text-slate-400">{hint}</p>}
        </div>
      </div>
    </div>
  );
}

function Donut({ data, size = 116, stroke = 20 }: { data: { value: number; color: string }[]; size?: number; stroke?: number }) {
  const total = data.reduce((s, d) => s + d.value, 0) || 1;
  const r = (size - stroke) / 2;
  const c = 2 * Math.PI * r;
  let offset = 0;
  return (
    <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`} className="shrink-0">
      <g transform={`rotate(-90 ${size / 2} ${size / 2})`}>
        <circle cx={size / 2} cy={size / 2} r={r} fill="none" stroke="#f1f5f9" strokeWidth={stroke} />
        {data.map((d, i) => {
          const len = (d.value / total) * c;
          const el = <circle key={i} cx={size / 2} cy={size / 2} r={r} fill="none" stroke={d.color} strokeWidth={stroke} strokeDasharray={`${len} ${c - len}`} strokeDashoffset={-offset} />;
          offset += len;
          return el;
        })}
      </g>
    </svg>
  );
}

function DonutCard({ title, rows, unidad }: { title: string; rows: { nombre: string; cantidad: number; monto: number }[]; unidad?: string }) {
  const total = rows.reduce((s, r) => s + r.monto, 0) || 1;
  const data = rows.map((r, i) => ({ value: r.monto, color: PALETTE[i % PALETTE.length] }));
  return (
    <div className="rounded-2xl border border-slate-200/70 bg-white p-5 shadow-card">
      <p className="mb-4 text-sm font-semibold text-slate-800">{title}</p>
      {rows.length === 0 ? (
        <p className="py-6 text-center text-sm text-slate-400">Sin datos</p>
      ) : (
        <>
          <div className="flex items-center gap-4">
            <Donut data={data} />
            <div className="min-w-0 flex-1 space-y-2.5">
              {rows.map((r, i) => {
                const pct = ((r.monto / total) * 100).toFixed(1);
                return (
                  <div key={i}>
                    <div className="flex items-center gap-2 text-sm">
                      <span className="h-2.5 w-2.5 shrink-0 rounded-full" style={{ backgroundColor: PALETTE[i % PALETTE.length] }} />
                      <span className="min-w-0 flex-1 truncate text-slate-600">{r.nombre}</span>
                      <span className="font-semibold text-slate-700">{soles(r.monto)}</span>
                      {!unidad && <span className="w-12 shrink-0 text-right text-xs text-slate-400">{pct}%</span>}
                    </div>
                    {unidad && <p className="ml-4 text-[11px] text-slate-400">{r.cantidad} {unidad} · {pct}% del total</p>}
                  </div>
                );
              })}
            </div>
          </div>
          <div className="mt-4 flex justify-between border-t border-slate-100 pt-3 text-sm">
            <span className="text-slate-500">Total</span>
            <span className="font-bold text-primary">{soles(total)}</span>
          </div>
        </>
      )}
    </div>
  );
}

function Fila({ k, v, bold }: { k: string; v: string; bold?: boolean }) {
  return (
    <div className="flex justify-between border-b border-slate-100 py-1 last:border-0">
      <span className="text-slate-500">{k}</span>
      <span className={bold ? "font-bold text-slate-800" : "font-medium text-slate-700"}>{v}</span>
    </div>
  );
}
