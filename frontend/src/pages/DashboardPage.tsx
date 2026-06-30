import { useMemo, useState, type ReactNode } from "react";
import { useQuery } from "@tanstack/react-query";
import {
  Wallet, Receipt, Coins, TrendingUp, GraduationCap, ArrowUpRight, ArrowDownRight,
  Filter, FileSpreadsheet, Printer, Calendar, CalendarDays, CalendarRange, ChevronDown, Maximize2, X,
} from "lucide-react";
import api from "../lib/api";
import type { Dashboard, DashboardPeriodo, Programa, ReporteIngresos, Servicio, Turno, UsuarioSistema } from "../types";
import { soles } from "../lib/utils";

const DONUT_COLORS = ["#6366f1", "#22c55e", "#f59e0b", "#8b5cf6", "#94a3b8"];
const BAR_COLORS = ["#6366f1", "#22c55e", "#f59e0b", "#8b5cf6", "#14b8a6"];

function isoHoy() { return new Date().toISOString().slice(0, 10); }
function isoOffset(days: number) { const d = new Date(); d.setDate(d.getDate() + days); return d.toISOString().slice(0, 10); }
function inicioMes() { const d = new Date(); return new Date(d.getFullYear(), d.getMonth(), 1).toISOString().slice(0, 10); }

function delta(actual: number, anterior: number) {
  if (anterior <= 0) return actual > 0 ? { txt: "+100%", up: true } : null;
  const pct = ((actual - anterior) / anterior) * 100;
  return { txt: `${pct >= 0 ? "+" : ""}${pct.toFixed(1)}%`, up: pct >= 0 };
}

export default function DashboardPage() {
  const [desde, setDesde] = useState(isoOffset(-30));
  const [hasta, setHasta] = useState(isoHoy());
  const [fProg, setFProg] = useState("");
  const [fServ, setFServ] = useState("");
  const [fUser, setFUser] = useState("");
  const [filtrosOpen, setFiltrosOpen] = useState(false);
  const [full, setFull] = useState(false);
  const [page, setPage] = useState(1);
  const porPagina = 12;

  const { data: dash } = useQuery({ queryKey: ["dashboard"], queryFn: async () => (await api.get<Dashboard>("/dashboard")).data });
  const { data: periodo } = useQuery({ queryKey: ["dashboard-periodo"], queryFn: async () => (await api.get<DashboardPeriodo>("/periodos/dashboard")).data });
  const { data: programas } = useQuery({ queryKey: ["programas"], queryFn: async () => (await api.get<Programa[]>("/programas?soloActivos=true")).data });
  const { data: servicios } = useQuery({ queryKey: ["servicios"], queryFn: async () => (await api.get<Servicio[]>("/servicios?soloActivos=true")).data });
  const { data: turnos } = useQuery({ queryKey: ["turnos"], queryFn: async () => (await api.get<Turno[]>("/turnos")).data });
  const { data: usuarios } = useQuery({ queryKey: ["usuarios-f"], queryFn: async () => (await api.get<UsuarioSistema[]>("/usuarios")).data, retry: false });
  const { data: reporte } = useQuery({
    queryKey: ["reporte", desde, hasta],
    queryFn: async () => (await api.get<ReporteIngresos>("/reportes/ingresos", { params: { desde, hasta } })).data,
  });

  const items = useMemo(() => (reporte?.items ?? []).filter((i) =>
    (!fProg || i.programa === fProg) && (!fServ || i.servicio === fServ) && (!fUser || i.usuario === fUser)), [reporte, fProg, fServ, fUser]);

  const totalFiltrado = items.reduce((s, i) => s + i.importe, 0);
  const totalPages = Math.max(1, Math.ceil(items.length / porPagina));
  const pageItems = items.slice((page - 1) * porPagina, page * porPagina);
  const activos = [fProg, fServ, fUser].filter(Boolean).length;

  const serie = dash?.ingresosDiarios ?? [];
  const maxDia = Math.max(...serie.map((d) => d.monto), 1);
  const promedioDia = serie.length ? serie.reduce((s, d) => s + d.monto, 0) / serie.length : 0;
  const mayor = serie.reduce((m, d) => (d.monto > m.monto ? d : m), { fecha: "", monto: 0 });
  const menor = serie.filter((d) => d.monto > 0).reduce((m, d) => (m.monto === 0 || d.monto < m.monto ? d : m), { fecha: "", monto: 0 });

  const exportarCsv = () => {
    const head = ["Fecha", "Ticket", "DNI", "Estudiante", "Programa", "Concepto", "Importe", "Usuario"];
    const rows = items.map((i) => [new Date(i.fecha).toLocaleString("es-PE"), i.numeroTicket, i.dni, i.alumno, i.programa, i.servicio, i.importe.toFixed(2), i.usuario]);
    const csv = [head, ...rows].map((r) => r.map((c) => `"${String(c).replace(/"/g, '""')}"`).join(",")).join("\n");
    const a = document.createElement("a");
    a.href = URL.createObjectURL(new Blob(["﻿" + csv], { type: "text/csv;charset=utf-8;" }));
    a.download = `reporte_${desde}_${hasta}.csv`; a.click();
  };

  const rapido = (d: string, h: string) => { setDesde(d); setHasta(h); setPage(1); };
  const limpiar = () => { setFProg(""); setFServ(""); setFUser(""); setPage(1); };

  const metricas = [
    { label: "Recaudado hoy", value: soles(dash?.totalRecaudadoHoy ?? 0), color: "bg-cyan-700", icon: <Wallet className="text-white" size={20} />, comp: delta(dash?.totalRecaudadoHoy ?? 0, dash?.totalRecaudadoAyer ?? 0), periodo: "vs ayer" },
    { label: "Tickets hoy", value: `${dash?.ticketsHoy ?? 0}`, color: "bg-emerald-500", icon: <Receipt className="text-white" size={20} />, comp: delta(dash?.ticketsHoy ?? 0, dash?.ticketsAyer ?? 0), periodo: "vs ayer" },
    { label: "Recaudado este mes", value: soles(dash?.totalRecaudadoMes ?? 0), color: "bg-amber-500", icon: <Coins className="text-white" size={20} />, comp: delta(dash?.totalRecaudadoMes ?? 0, dash?.totalRecaudadoMesPasado ?? 0), periodo: "vs mes pasado" },
    { label: "Tickets este mes", value: `${dash?.ticketsMes ?? 0}`, color: "bg-blue-500", icon: <TrendingUp className="text-white" size={20} />, comp: delta(dash?.ticketsMes ?? 0, dash?.ticketsMesPasado ?? 0), periodo: "vs mes pasado" },
    { label: "Programas activos", value: `${dash?.programasActivos ?? 0}`, color: "bg-fuchsia-500", icon: <GraduationCap className="text-white" size={20} />, comp: null, periodo: "— sin cambios" },
  ];

  return (
    <div className="space-y-5">
      {/* Toolbar: título + rango + exportar (arriba) */}
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight text-slate-800">Dashboard</h1>
          <p className="text-sm text-slate-500">Análisis de ingresos por recursos propios y actividades productivas</p>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <span className="flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-600 shadow-card">
            <Calendar size={15} /> {new Date(desde).toLocaleDateString("es-PE")} – {new Date(hasta).toLocaleDateString("es-PE")}
          </span>
          <button onClick={exportarCsv} className="flex items-center gap-2 rounded-lg bg-emerald-50 px-3 py-2 text-sm font-medium text-emerald-700 hover:bg-emerald-100"><FileSpreadsheet size={16} /> Excel</button>
          <button onClick={() => window.print()} className="flex items-center gap-2 rounded-lg bg-gradient-to-br from-cyan-600 to-slate-800 px-3 py-2 text-sm font-medium text-white hover:bg-slate-100"><Printer size={16} /> Imprimir</button>
        </div>
      </div>

      {/* Métricas */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5">
        {metricas.map((m) => (
          <div key={m.label} className="rounded-2xl border border-slate-200 bg-white p-4 shadow-card">
            <div className="flex items-center gap-3">
              <div className={`grid h-12 w-12 shrink-0 place-items-center rounded-xl ${m.color}`}>{m.icon}</div>
              <div>
                <p className="text-[11px] font-medium text-slate-400">{m.label}</p>
                <p className="text-base font-semibold text-slate-800">{m.value}</p>
              </div>
            </div>
            {m.comp ? (
              <p className={`mt-2 flex items-center gap-1 text-[11px] font-medium ${m.comp.up ? "text-emerald-600" : "text-rose-500"}`}>
                {m.comp.up ? <ArrowUpRight size={12} /> : <ArrowDownRight size={12} />} {m.comp.txt} {m.periodo}
              </p>
            ) : <p className="mt-2 text-[11px] text-slate-400">{m.periodo}</p>}
          </div>
        ))}
      </div>

      {/* Gestión financiera por período */}
      {periodo?.hayPeriodoActivo && (
        <div className="rounded-2xl border border-slate-200/70 bg-gradient-to-br from-cyan-600 to-slate-800 p-5 text-white shadow-card">
          <div className="mb-3 flex items-center gap-2">
            <CalendarRange size={16} className="text-cyan-800" />
            <p className="text-sm font-semibold">Gestión financiera por período · <span className="text-red-600">{periodo.nombre}</span></p>
          </div>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
            {[
              { l: "Ingresos acumulados", v: soles(periodo.ingresosAcumulados) },
              { l: "Tickets emitidos", v: `${periodo.tickets}` },
              { l: "Estudiantes atendidos", v: `${periodo.estudiantes}` },
              { l: "Servicios registrados", v: `${periodo.servicios}` },
              { l: "Días restantes", v: `${periodo.diasRestantes}` },
            ].map((x) => (
              <div key={x.l} className="rounded-xl bg-white/[0.06] p-3 ring-1 ring-white/[0.06]">
                <p className="text-[11px] text-white/60">{x.l}</p>
                <p className="text-base font-semibold">{x.v}</p>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Filtros colapsable */}
      <div className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-card">
        <button onClick={() => setFiltrosOpen((o) => !o)} className="flex w-full items-center justify-between px-5 py-3 text-left">
          <span className="flex items-center gap-2 text-sm font-semibold text-slate-700">
            <Filter size={15} /> Filtros
            {activos > 0 && <span className="rounded-full bg-cyan-900 px-2 py-0.5 text-[11px] font-medium text-white">{activos}</span>}
          </span>
          <ChevronDown size={18} className={`text-slate-400 transition-transform ${filtrosOpen ? "rotate-180" : ""}`} />
        </button>
        {filtrosOpen && (
          <div className="border-t border-slate-100 px-5 py-4">
            <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6">
              <Field label="Fecha desde"><input type="date" value={desde} onChange={(e) => { setDesde(e.target.value); setPage(1); }} className="h-10 w-full rounded-lg border border-slate-300 px-3 text-sm" /></Field>
              <Field label="Fecha hasta"><input type="date" value={hasta} onChange={(e) => { setHasta(e.target.value); setPage(1); }} className="h-10 w-full rounded-lg border border-slate-300 px-3 text-sm" /></Field>
              <Field label="Programa"><Sel value={fProg} onChange={setFProg} opts={["Todos los programas", ...(programas?.map((p) => p.nombre) ?? [])]} /></Field>
              <Field label="Servicio"><Sel value={fServ} onChange={setFServ} opts={["Todos los servicios", ...(servicios?.map((s) => s.nombre) ?? [])]} /></Field>
              <Field label="Usuario"><Sel value={fUser} onChange={setFUser} opts={["Todos los usuarios", ...(usuarios?.map((u) => u.username) ?? [])]} /></Field>
              <Field label="Turno"><Sel value="" onChange={() => {}} opts={["Todos los turnos", ...(turnos?.map((t) => t.nombre) ?? [])]} /></Field>
            </div>
            <div className="mt-3 flex flex-wrap items-center gap-2">
              <span className="mr-1 text-xs text-slate-400">Rápidos:</span>
              <Chip icon={<Calendar size={13} />} label="Hoy" onClick={() => rapido(isoHoy(), isoHoy())} />
              <Chip icon={<CalendarRange size={13} />} label="Semana" onClick={() => rapido(isoOffset(-6), isoHoy())} />
              <Chip icon={<CalendarDays size={13} />} label="Este mes" onClick={() => rapido(inicioMes(), isoHoy())} />
              <Chip icon={<CalendarDays size={13} />} label="Mes pasado" onClick={() => { const d = new Date(); rapido(new Date(d.getFullYear(), d.getMonth() - 1, 1).toISOString().slice(0, 10), new Date(d.getFullYear(), d.getMonth(), 0).toISOString().slice(0, 10)); }} />
              <button onClick={limpiar} className="ml-auto rounded-lg border border-slate-300 px-3 py-1.5 text-xs text-slate-600 hover:bg-slate-50">Limpiar</button>
              <button onClick={() => setFiltrosOpen(false)} className="rounded-lg bg-cyan-900 px-3 py-1.5 text-xs font-medium text-white hover:bg-cyan-800">Aplicar</button>
            </div>
          </div>
        )}
      </div>

      {/* Analítica: gráfico + dona + programas */}
      <div className="grid gap-5 lg:grid-cols-3">
        <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-card lg:col-span-2">
          <p className="mb-3 text-sm font-semibold text-slate-700">Ingresos últimos 30 días</p>
          <LineChart data={serie} max={maxDia} />
          <div className="mt-4 grid grid-cols-2 gap-4 border-t border-slate-100 pt-4 text-center sm:grid-cols-4">
            <Mini label="Total ingresos" value={soles(serie.reduce((s, d) => s + d.monto, 0))} />
            <Mini label="Promedio diario" value={soles(promedioDia)} />
            <Mini label="Mayor ingreso" value={soles(mayor.monto)} hint={mayor.fecha ? new Date(mayor.fecha).toLocaleDateString("es-PE") : ""} />
            <Mini label="Menor ingreso" value={soles(menor.monto)} hint={menor.fecha ? new Date(menor.fecha).toLocaleDateString("es-PE") : ""} />
          </div>
        </div>

        <div className="space-y-5">
          <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-card">
            <p className="mb-3 text-sm font-semibold text-slate-700">Servicios más cobrados</p>
            <div className="flex items-center gap-4">
              <Donut items={(dash?.serviciosMasCobrados ?? []).slice(0, 5)} />
              <ul className="flex-1 space-y-1.5 text-xs">
                {(dash?.serviciosMasCobrados ?? []).slice(0, 5).map((s, i) => {
                  const total = (dash?.serviciosMasCobrados ?? []).reduce((a, b) => a + b.monto, 0) || 1;
                  return (
                    <li key={i} className="flex items-center justify-between gap-2">
                      <span className="flex items-center gap-1.5 truncate text-slate-600"><span className="h-2.5 w-2.5 rounded-sm" style={{ background: DONUT_COLORS[i] }} />{s.nombre}</span>
                      <span className="font-medium text-slate-700">{Math.round((s.monto / total) * 100)}%</span>
                    </li>
                  );
                })}
                {(dash?.serviciosMasCobrados ?? []).length === 0 && <li className="text-slate-400">Sin datos</li>}
              </ul>
            </div>
          </div>

          <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-card">
            <p className="mb-3 text-sm font-semibold text-slate-700">Programas con más ingresos</p>
            <ul className="space-y-2.5">
              {(dash?.programasConMasPagos ?? []).slice(0, 5).map((p, i) => {
                const max = Math.max(...(dash?.programasConMasPagos ?? []).map((x) => x.monto), 1);
                return (
                  <li key={i} className="text-xs">
                    <div className="mb-1 flex justify-between"><span className="truncate text-slate-600">{p.nombre}</span><span className="font-semibold text-slate-700">{soles(p.monto)}</span></div>
                    <div className="h-1.5 w-full overflow-hidden rounded-full bg-slate-100"><div className="h-full rounded-full" style={{ width: `${(p.monto / max) * 100}%`, background: BAR_COLORS[i] }} /></div>
                  </li>
                );
              })}
              {(dash?.programasConMasPagos ?? []).length === 0 && <li className="text-xs text-slate-400">Sin datos</li>}
            </ul>
          </div>
        </div>
      </div>

      {/* Detalle de ingresos: a todo el ancho (lo más importante) */}
      <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-card">
        <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
          <p className="text-base font-semibold text-slate-800">Detalle de ingresos</p>
          <div className="flex items-center gap-2">
            <span className="rounded-lg bg-cyan-800-soft px-3 py-1 text-sm font-medium text-cyan-800">Total: {soles(totalFiltrado)} · {items.length} registros</span>
            <button onClick={() => setFull(true)} className="flex items-center gap-2 rounded-lg border border-slate-300 px-3 py-1.5 text-sm font-medium text-slate-600 hover:bg-slate-50">
              <Maximize2 size={15} /> Pantalla completa
            </button>
          </div>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-slate-200 text-left text-xs uppercase text-slate-400">
                <th className="px-2 py-2">Fecha</th><th className="px-2">Ticket</th><th className="px-2">DNI</th><th className="px-2">Estudiante</th><th className="px-2">Programa</th><th className="px-2">Concepto</th><th className="px-2 text-right">Importe</th><th className="px-2">Usuario</th>
              </tr>
            </thead>
            <tbody>
              {pageItems.map((i, idx) => (
                <tr key={idx} className="border-b border-slate-50 hover:bg-slate-50">
                  <td className="whitespace-nowrap px-2 py-2.5 text-slate-500">{new Date(i.fecha).toLocaleDateString("es-PE")}</td>
                  <td className="px-2 font-mono font-medium text-slate-700">{i.numeroTicket}</td>
                  <td className="px-2 font-mono text-slate-500">{i.dni}</td>
                  <td className="px-2 text-slate-700">{i.alumno}</td>
                  <td className="px-2 text-slate-500">{i.programa}</td>
                  <td className="px-2 text-slate-600">{i.servicio}</td>
                  <td className="whitespace-nowrap px-2 text-right font-semibold text-emerald-600">{soles(i.importe)}</td>
                  <td className="px-2 text-slate-500">{i.usuario}</td>
                </tr>
              ))}
              {items.length === 0 && <tr><td colSpan={8} className="py-4 text-center text-slate-400">Sin ingresos en el periodo seleccionado</td></tr>}
            </tbody>
          </table>
        </div>
        <div className="mt-3 flex items-center justify-between text-xs text-slate-500">
          <span>Mostrando {items.length === 0 ? 0 : (page - 1) * porPagina + 1} a {Math.min(page * porPagina, items.length)} de {items.length} resultados</span>
          <div className="flex items-center gap-1">
            <button disabled={page <= 1} onClick={() => setPage((p) => p - 1)} className="rounded border border-slate-200 px-2 py-1 disabled:opacity-40">←</button>
            <span className="px-2 py-1">{page} / {totalPages}</span>
            <button disabled={page >= totalPages} onClick={() => setPage((p) => p + 1)} className="rounded border border-slate-200 px-2 py-1 disabled:opacity-40">→</button>
          </div>
        </div>
      </div>

      {/* Visor a pantalla completa (tipo Excel) */}
      {full && (
        <div className="fixed inset-0 z-50 flex flex-col bg-white">
          <div className="flex flex-wrap items-center justify-between gap-2 border-b border-slate-200 px-6 py-3">
            <div>
              <p className="text-lg font-bold text-slate-800">Detalle de ingresos</p>
              <p className="text-xs text-slate-500">{new Date(desde).toLocaleDateString("es-PE")} – {new Date(hasta).toLocaleDateString("es-PE")} · {items.length} registros · Total {soles(totalFiltrado)}</p>
            </div>
            <div className="flex items-center gap-2">
              <button onClick={exportarCsv} className="flex items-center gap-2 rounded-lg bg-emerald-50 px-3 py-2 text-sm font-medium text-emerald-700 hover:bg-emerald-100"><FileSpreadsheet size={16} /> Excel</button>
              <button onClick={() => setFull(false)} className="flex items-center gap-2 rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-600 hover:bg-slate-50"><X size={16} /> Cerrar</button>
            </div>
          </div>
          <div className="flex-1 overflow-auto">
            <table className="w-full text-sm">
              <thead className="sticky top-0 z-10 bg-slate-100 text-left text-xs uppercase text-slate-500 shadow-sm">
                <tr>
                  <th className="px-4 py-3">#</th><th className="px-3">Fecha</th><th className="px-3">Ticket</th><th className="px-3">DNI</th><th className="px-3">Estudiante</th><th className="px-3">Programa</th><th className="px-3">Concepto</th><th className="px-3 text-right">Importe</th><th className="px-3">Usuario</th>
                </tr>
              </thead>
              <tbody>
                {items.map((i, idx) => (
                  <tr key={idx} className={idx % 2 ? "bg-slate-50/60" : ""}>
                    <td className="px-4 py-2 text-slate-400">{idx + 1}</td>
                    <td className="whitespace-nowrap px-3 text-slate-500">{new Date(i.fecha).toLocaleString("es-PE")}</td>
                    <td className="px-3 font-mono font-medium text-slate-700">{i.numeroTicket}</td>
                    <td className="px-3 font-mono text-slate-500">{i.dni}</td>
                    <td className="whitespace-nowrap px-3 text-slate-700">{i.alumno}</td>
                    <td className="px-3 text-slate-500">{i.programa}</td>
                    <td className="px-3 text-slate-600">{i.servicio}</td>
                    <td className="whitespace-nowrap px-3 text-right font-semibold text-emerald-600">{soles(i.importe)}</td>
                    <td className="px-3 text-slate-500">{i.usuario}</td>
                  </tr>
                ))}
                {items.length === 0 && <tr><td colSpan={9} className="py-6 text-center text-slate-400">Sin ingresos en el periodo</td></tr>}
              </tbody>
              <tfoot className="sticky bottom-0 bg-white">
                <tr className="border-t-2 border-slate-200 font-bold">
                  <td className="px-4 py-3" colSpan={7}>TOTAL ({items.length} registros)</td>
                  <td className="px-3 text-right text-cyan-800">{soles(totalFiltrado)}</td>
                  <td></td>
                </tr>
              </tfoot>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}

function Field({ label, children }: { label: string; children: ReactNode }) {
  return <div><label className="mb-1 block text-xs font-medium text-slate-600">{label}</label>{children}</div>;
}
function Sel({ value, onChange, opts }: { value: string; onChange: (v: string) => void; opts: string[] }) {
  return (
    <select value={value} onChange={(e) => onChange(e.target.value === opts[0] ? "" : e.target.value)} className="h-10 w-full rounded-lg border border-slate-300 px-3 text-sm">
      {opts.map((o, i) => <option key={i} value={i === 0 ? "" : o}>{o}</option>)}
    </select>
  );
}
function Chip({ icon, label, onClick }: { icon: ReactNode; label: string; onClick: () => void }) {
  return <button onClick={onClick} className="flex items-center gap-1.5 rounded-lg border border-slate-200 px-3 py-1.5 text-xs text-slate-600 hover:bg-slate-50">{icon} {label}</button>;
}
function Mini({ label, value, hint }: { label: string; value: string; hint?: string }) {
  return <div><p className="text-[11px] text-slate-400">{label}</p><p className="text-sm font-bold text-slate-800">{value}</p>{hint && <p className="text-[10px] text-slate-400">{hint}</p>}</div>;
}

function Donut({ items }: { items: { nombre: string; monto: number }[] }) {
  const total = items.reduce((s, i) => s + i.monto, 0) || 1;
  let acc = 0;
  return (
    <svg viewBox="0 0 42 42" className="h-32 w-32 shrink-0 -rotate-90">
      <circle cx="21" cy="21" r="15.915" fill="none" stroke="#f1f5f9" strokeWidth="5" />
      {items.map((it, i) => {
        const pct = (it.monto / total) * 100;
        const el = <circle key={i} cx="21" cy="21" r="15.915" fill="none" stroke={DONUT_COLORS[i % DONUT_COLORS.length]} strokeWidth="5" strokeDasharray={`${pct} ${100 - pct}`} strokeDashoffset={-acc} />;
        acc += pct;
        return el;
      })}
    </svg>
  );
}

function LineChart({ data, max }: { data: { fecha: string; monto: number }[]; max: number }) {
  const W = 600, H = 200, pad = 28, bottom = 24;
  if (data.length === 0) return <p className="text-sm text-slate-400">Sin datos</p>;
  const pts = data.map((d, i) => ({ x: pad + (i / (data.length - 1)) * (W - pad - 8), y: H - bottom - (d.monto / max) * (H - bottom - 10) }));
  const line = pts.map((p, i) => `${i === 0 ? "M" : "L"}${p.x.toFixed(1)},${p.y.toFixed(1)}`).join(" ");
  const area = `${line} L${pts[pts.length - 1].x.toFixed(1)},${H - bottom} L${pts[0].x.toFixed(1)},${H - bottom} Z`;
  const ticks = [0, 0.25, 0.5, 0.75, 1];
  return (
    <svg viewBox={`0 0 ${W} ${H}`} className="w-full">
      <defs><linearGradient id="grad" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stopColor="#6366f1" stopOpacity="0.25" /><stop offset="1" stopColor="#6366f1" stopOpacity="0" /></linearGradient></defs>
      {ticks.map((t, i) => { const y = H - bottom - t * (H - bottom - 10); return (<g key={i}><line x1={pad} y1={y} x2={W - 8} y2={y} stroke="#f1f5f9" /><text x={2} y={y + 3} fontSize="8" fill="#94a3b8">{`S/ ${Math.round(max * t)}`}</text></g>); })}
      <path d={area} fill="url(#grad)" />
      <path d={line} fill="none" stroke="#6366f1" strokeWidth="2" />
      {pts.filter((_, i) => i % 4 === 0).map((p, i) => <circle key={i} cx={p.x} cy={p.y} r="2.5" fill="#6366f1" />)}
      {data.filter((_, i) => i % 6 === 0).map((d, i) => <text key={i} x={pad + ((i * 6) / (data.length - 1)) * (W - pad - 8)} y={H - 6} fontSize="8" fill="#94a3b8" textAnchor="middle">{new Date(d.fecha).toLocaleDateString("es-PE", { day: "2-digit", month: "short" })}</text>)}
    </svg>
  );
}
