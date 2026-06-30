import { useEffect, useMemo, useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import {
  Receipt, Search, CheckCircle2, Clock, Wrench, GraduationCap,
  FileText, ScrollText, Award, CreditCard, Folder, ClipboardList, MoreHorizontal, Check,
  Trash2, ShoppingCart, Printer, Eye, Calendar, Star, X,
} from "lucide-react";
import api, { apiError } from "../lib/api";
import type { ConsultaDni, Periodo, Programa, Registro, Servicio, Ticket, TicketListItem, Turno } from "../types";
import { soles } from "../lib/utils";
import { numeroALetras } from "../lib/numeroALetras";
import { useAuth } from "../context/AuthContext";
import ThermalTicket from "../components/ThermalTicket";

function servicioEstilo(nombre: string): { icon: typeof Wrench; color: string } {
  const n = nombre.toLowerCase();
  if (n.includes("mantenimiento")) return { icon: Wrench, color: "bg-blue-500" };
  if (n.includes("modular")) return { icon: GraduationCap, color: "bg-slate-700" };
  if (n.includes("constancia")) return { icon: FileText, color: "bg-emerald-500" };
  if (n.includes("certificado de estudios")) return { icon: ScrollText, color: "bg-orange-500" };
  if (n.includes("título") || n.includes("titulo")) return { icon: Award, color: "bg-rose-500" };
  if (n.includes("carnet")) return { icon: CreditCard, color: "bg-cyan-500" };
  if (n.includes("folder") || n.includes("medalla")) return { icon: Folder, color: "bg-amber-500" };
  if (n.includes("examen")) return { icon: ClipboardList, color: "bg-indigo-500" };
  if (n.includes("fut")) return { icon: FileText, color: "bg-cyan-800" };
  return { icon: MoreHorizontal, color: "bg-slate-400" };
}

interface Sel { servicioId: number; nombre: string; importe: number; }

/** Edad en años cumplidos a partir de una fecha "YYYY-MM-DD". */
function calcularEdad(fecha: string): number {
  const f = new Date(fecha);
  const hoy = new Date();
  let edad = hoy.getFullYear() - f.getFullYear();
  const m = hoy.getMonth() - f.getMonth();
  if (m < 0 || (m === 0 && hoy.getDate() < f.getDate())) edad--;
  return edad < 0 ? 0 : edad;
}

export default function TesoreriaPage() {
  const { user } = useAuth();
  const qc = useQueryClient();
  const [dni, setDni] = useState("");
  const [auto, setAuto] = useState(true);
  const [consulta, setConsulta] = useState<ConsultaDni | null>(null);
  const [buscando, setBuscando] = useState(false);

  const [nombres, setNombres] = useState("");
  const [apellidos, setApellidos] = useState("");
  const [programaId, setProgramaId] = useState<number | "">("");
  const [turnoId, setTurnoId] = useState<number | "">("");
  const [seccionNuevo, setSeccionNuevo] = useState("U");
  const [sexoNuevo, setSexoNuevo] = useState("");
  const [fechaNacNuevo, setFechaNacNuevo] = useState("");
  const [celularNuevo, setCelularNuevo] = useState("");
  const [alumnoId, setAlumnoId] = useState<number | null>(null);

  const [seleccion, setSeleccion] = useState<Sel[]>([]);
  const [lineas, setLineas] = useState<Sel[]>([]);
  const [verTodos, setVerTodos] = useState(false);

  const [ticket, setTicket] = useState<Ticket | null>(null);
  const [msg, setMsg] = useState("");
  const [emitiendo, setEmitiendo] = useState(false);
  const [historial, setHistorial] = useState<TicketListItem[] | null>(null);

  const verHistorial = async () => {
    try { setHistorial((await api.get<TicketListItem[]>(`/tickets`, { params: { dni } })).data); }
    catch (e) { setMsg(apiError(e)); }
  };

  const { data: servicios } = useQuery({ queryKey: ["servicios"], queryFn: async () => (await api.get<Servicio[]>("/servicios?soloActivos=true")).data });
  const { data: programas } = useQuery({ queryKey: ["programas"], queryFn: async () => (await api.get<Programa[]>("/programas?soloActivos=true")).data });
  const { data: turnos } = useQuery({ queryKey: ["turnos"], queryFn: async () => (await api.get<Turno[]>("/turnos")).data });
  const { data: periodoActivo } = useQuery({ queryKey: ["periodo-activo"], queryFn: async () => (await api.get<Periodo | null>("/periodos/activo")).data });
  const { data: registros } = useQuery({ queryKey: ["registros", periodoActivo?.id], queryFn: async () => (await api.get<Registro[]>("/registros", { params: { periodoId: periodoActivo?.id } })).data, enabled: !!periodoActivo?.id });

  // Aulas (registros de matrícula) ya existentes para el programa + turno elegidos.
  const aulasCombo = (registros ?? []).filter((r) => r.programaId === Number(programaId) && r.turnoId === Number(turnoId));

  useEffect(() => {
    if (!auto || dni.length !== 8) { setConsulta(null); return; }
    let cancel = false;
    setBuscando(true);
    (async () => {
      try {
        const { data } = await api.get<ConsultaDni>(`/alumnos/consulta-dni/${dni}`);
        if (cancel) return;
        setConsulta(data);
        setNombres(data.nombres ?? ""); setApellidos(data.apellidos ?? "");
        setAlumnoId(data.existe ? data.alumnoId ?? null : null);
        setProgramaId(data.programaId ?? ""); setTurnoId(data.turnoId ?? "");
        setSexoNuevo(data.sexo ?? ""); setCelularNuevo(data.celular ?? "");
        setFechaNacNuevo(data.fechaNacimiento ? data.fechaNacimiento.slice(0, 10) : "");
        if (data.seccion) setSeccionNuevo(data.seccion);
      } catch (e) { if (!cancel) setMsg(apiError(e)); }
      finally { if (!cancel) setBuscando(false); }
    })();
    return () => { cancel = true; };
  }, [dni, auto]);

  const totalLineas = useMemo(() => lineas.reduce((s, l) => s + l.importe, 0), [lineas]);
  const totalSel = useMemo(() => seleccion.reduce((s, l) => s + l.importe, 0), [seleccion]);

  const toggleServicio = (s: Servicio) => {
    setSeleccion((sel) => sel.some((x) => x.servicioId === s.id)
      ? sel.filter((x) => x.servicioId !== s.id)
      : [...sel, { servicioId: s.id, nombre: s.nombre, importe: s.precio }]);
  };

  const agregar = () => {
    if (seleccion.length === 0) return;
    setLineas((ls) => [...ls, ...seleccion.filter((s) => s.importe > 0)]);
    setSeleccion([]);
  };

  const registrar = async () => {
    setMsg("");
    try {
      const { data } = await api.post<{ id: number }>("/alumnos", { dni, nombres, apellidos, programaId: Number(programaId), turnoId: Number(turnoId), seccion: seccionNuevo || "U", sexo: sexoNuevo || null, fechaNacimiento: fechaNacNuevo || null, celular: celularNuevo || null });
      setAlumnoId(data.id); setConsulta((c) => (c ? { ...c, existe: true } : c));
      qc.invalidateQueries({ queryKey: ["registros"] });
    } catch (e) { setMsg(apiError(e)); }
  };

  const emitir = async () => {
    setMsg(""); setEmitiendo(true);
    try {
      const { data } = await api.post<Ticket>("/tickets", { alumnoId, detalles: lineas.map((l) => ({ servicioId: l.servicioId, importe: l.importe })) });
      setTicket(data);
    } catch (e) { setMsg(apiError(e)); } finally { setEmitiendo(false); }
  };

  const nuevo = () => {
    setDni(""); setConsulta(null); setNombres(""); setApellidos("");
    setProgramaId(""); setTurnoId(""); setAlumnoId(null);
    setSeccionNuevo("U"); setSexoNuevo(""); setFechaNacNuevo(""); setCelularNuevo("");
    setSeleccion([]); setLineas([]); setTicket(null); setMsg("");
  };

  const puedeEmitir = !!alumnoId && lineas.length > 0 && !!periodoActivo;
  const saludo = (user?.nombreCompleto || user?.rol || "").split(" ")[0];

  if (ticket) {
    return (
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <h1 className="text-xl font-semibold tracking-tight text-slate-800">Ticket {ticket.numeroTicket} emitido ✓</h1>
          <div className="flex gap-2">
            <button onClick={() => window.print()} className="flex items-center gap-2 rounded-xl bg-gradient-to-r from-cyan-800 to-cyan-600 px-4 py-2 text-sm font-medium text-white shadow-glow"><Printer size={16} /> Imprimir</button>
            <button onClick={nuevo} className="rounded-xl border border-slate-300 bg-white px-4 py-2 text-sm font-medium hover:bg-slate-50">Nuevo ticket</button>
          </div>
        </div>
        <div className="mx-auto w-fit rounded-2xl border border-slate-200/70 bg-white p-4 shadow-card"><ThermalTicket ticket={ticket} /></div>
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 gap-6 xl:grid-cols-[1fr_360px]">
      <div className="space-y-5">
        {/* Saludo */}
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <h1 className="text-xl font-semibold tracking-tight text-slate-800">¡Hola, {saludo}! 👋</h1>
            <p className="text-sm text-slate-500">Bienvenido al sistema de ingresos por recursos propios</p>
          </div>
          {periodoActivo && (
            <span className="flex items-center gap-2 rounded-xl border border-emerald-200 bg-emerald-50 px-3 py-1.5 text-xs font-medium text-emerald-700">
              <span className="h-1.5 w-1.5 rounded-full bg-emerald-500" /> Período activo: {periodoActivo.nombre} · {periodoActivo.diasRestantes ?? 0} días restantes
            </span>
          )}
        </div>

        {!periodoActivo && (
          <div className="rounded-2xl border border-amber-200 bg-amber-50 p-4 text-sm text-amber-700">
            ⚠️ No existe un período académico activo. No se pueden emitir tickets. Comuníquese con el administrador.
          </div>
        )}

        {/* DNI del estudiante (card oscura) */}
        <div className="rounded-2xl bg-gradient-to-br from-cyan-900 to-cyan-900 p-5 text-white shadow-card">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <label className="text-sm font-semibold">DNI del estudiante</label>
            <label className="flex cursor-pointer items-center gap-2 text-sm text-white/70">
              Búsqueda automática
              <button type="button" onClick={() => setAuto((v) => !v)} className={`relative h-6 w-11 rounded-full transition-colors ${auto ? "bg-cyan-800" : "bg-white/20"}`}>
                <span className={`absolute top-0.5 h-5 w-5 rounded-full bg-white shadow transition-all ${auto ? "left-[22px]" : "left-0.5"}`} />
              </button>
            </label>
          </div>
          <div className="relative mt-3 max-w-md">
            <input value={dni} inputMode="numeric" maxLength={8} placeholder="12345678"
              onChange={(e) => setDni(e.target.value.replace(/\D/g, "").slice(0, 8))}
              className="h-11 w-full rounded-xl border border-white/10 bg-black/20 px-3 pr-10 text-sm text-white placeholder:text-white/40 focus:border-cyan-800 focus:outline-none focus:ring-2 focus:ring-cyan-800/40" />
            {dni.length === 8 && (buscando ? <Clock className="absolute right-3 top-3 animate-spin text-white/60" size={18} /> : consulta && <CheckCircle2 className="absolute right-3 top-3 text-emerald-400" size={18} />)}
          </div>

          {dni.length === 8 && consulta && (consulta.existe ? (
            <div className="mt-4 flex flex-wrap items-center gap-4 rounded-xl bg-white/[0.06] p-4">
              <img src={`https://api.dicebear.com/9.x/avataaars/svg?seed=${consulta.dni}&backgroundColor=c0e6ff,d1d4f9,ffd5dc`} alt="Avatar" className="h-14 w-14 shrink-0 rounded-full border-2 border-white/20 bg-white" />
              <div className="flex-1">
                <p className="text-lg font-semibold">{consulta.nombreCompleto}</p>
                <div className="mt-1 flex flex-wrap gap-2">
                  <span className="rounded-md bg-primary/30 px-2 py-0.5 text-xs font-medium text-white">{consulta.programa}</span>
                  <span className="rounded-md bg-white/10 px-2 py-0.5 text-xs text-white/80">Turno: {consulta.turno}</span>
                </div>
              </div>
              <button onClick={verHistorial} className="flex items-center gap-2 rounded-xl border border-white/15 bg-white/5 px-3 py-2 text-sm hover:bg-white/10"><Clock size={15} /> Ver historial</button>
            </div>
          ) : (
            <div className="mt-4 space-y-3 rounded-xl bg-white/[0.03] p-4 ring-1 ring-white/10">
              <p className="text-sm text-amber-300">{consulta.encontradoReniec ? "✓ Encontrado en RENIEC — seleccione programa y turno" : "No encontrado — complete los datos"}</p>
              <div className="grid gap-3 sm:grid-cols-2">
                <input placeholder="Apellidos" value={apellidos} onChange={(e) => setApellidos(e.target.value)} className="h-10 rounded-lg border border-white/10 bg-black/20 px-3 text-sm text-white placeholder:text-white/40 focus:border-cyan-800/60 focus:outline-none focus:ring-2 focus:ring-cyan-800/25" />
                <input placeholder="Nombres" value={nombres} onChange={(e) => setNombres(e.target.value)} className="h-10 rounded-lg border border-white/10 bg-black/20 px-3 text-sm text-white placeholder:text-white/40 focus:border-cyan-800/60 focus:outline-none focus:ring-2 focus:ring-cyan-800/25" />
                <select value={programaId} onChange={(e) => setProgramaId(Number(e.target.value))} className="h-10 rounded-lg border border-white/10 bg-black/20 px-3 text-sm text-white focus:border-cyan-800/60 focus:outline-none focus:ring-2 focus:ring-cyan-800/25 [&>option]:text-slate-800"><option value="">— Programa —</option>{programas?.map((p) => <option key={p.id} value={p.id}>{p.nombre}</option>)}</select>
                <select value={turnoId} onChange={(e) => setTurnoId(Number(e.target.value))} className="h-10 rounded-lg border border-white/10 bg-black/20 px-3 text-sm text-white focus:border-cyan-800/60 focus:outline-none focus:ring-2 focus:ring-cyan-800/25 [&>option]:text-slate-800"><option value="">— Turno —</option>{turnos?.map((t) => <option key={t.id} value={t.id}>{t.nombre}</option>)}</select>
                <select value={seccionNuevo} onChange={(e) => setSeccionNuevo(e.target.value)} title="Sección" className="h-10 rounded-lg border border-white/10 bg-black/20 px-3 text-sm text-white focus:border-cyan-800/60 focus:outline-none focus:ring-2 focus:ring-cyan-800/25 [&>option]:text-slate-800">
                  <option value="U">Sección U (Única)</option>
                  <option value="A">Sección A</option>
                  <option value="B">Sección B</option>
                  <option value="C">Sección C</option>
                  <option value="D">Sección D</option>
                </select>
                <select value={sexoNuevo} onChange={(e) => setSexoNuevo(e.target.value)} className="h-10 rounded-lg border border-white/10 bg-black/20 px-3 text-sm text-white focus:border-cyan-800/60 focus:outline-none focus:ring-2 focus:ring-cyan-800/25 [&>option]:text-slate-800"><option value="">— Sexo —</option><option value="H">H · Hombre</option><option value="M">M · Mujer</option></select>
                <input type="date" value={fechaNacNuevo} onChange={(e) => setFechaNacNuevo(e.target.value)} title="Fecha de nacimiento" className="h-10 rounded-lg border border-white/10 bg-black/20 px-3 text-sm text-white placeholder:text-white/40 focus:border-cyan-800/60 focus:outline-none focus:ring-2 focus:ring-cyan-800/25 [color-scheme:dark]" />
                <input value={celularNuevo} inputMode="numeric" onChange={(e) => setCelularNuevo(e.target.value.replace(/\D/g, "").slice(0, 9))} placeholder="Celular (opcional)" className="h-10 rounded-lg border border-white/10 bg-black/20 px-3 text-sm text-white placeholder:text-white/40 focus:border-cyan-800/60 focus:outline-none focus:ring-2 focus:ring-cyan-800/25" />
              </div>
              {fechaNacNuevo && <p className="text-[11px] text-white/50">Edad: {calcularEdad(fechaNacNuevo)} años (se calcula sola)</p>}
              {programaId !== "" && turnoId !== "" && aulasCombo.length > 0 && (
                <div className="flex flex-wrap items-center gap-1.5 text-[11px]">
                  <span className="text-white/50">Aulas ya creadas:</span>
                  {aulasCombo.map((r) => (
                    <button key={r.id} type="button" onClick={() => setSeccionNuevo(r.seccion)} className={`rounded-md px-2 py-0.5 transition ${seccionNuevo === r.seccion ? "bg-cyan-800 text-white" : "bg-white/10 text-white/80 hover:bg-white/20"}`}>Sec {r.seccion} · {r.cantidad}</button>
                  ))}
                  <span className="text-white/40">— el alumno entra al aula de la sección elegida.</span>
                </div>
              )}
              <button onClick={registrar} disabled={!nombres || !apellidos || !programaId || !turnoId} className="rounded-xl bg-cyan-800 px-4 py-2 text-sm font-medium text-white hover:bg-cyan-800 hover:text-white disabled:opacity-50">Registrar estudiante</button>
            </div>
          ))}
        </div>

        {/* Seleccionar servicios (multi-selección) */}
        <div className="rounded-2xl border border-slate-200/70 bg-white p-5 shadow-card">
          <div className="mb-3 flex items-center justify-between">
            <p className="text-sm font-semibold text-slate-700">Seleccionar servicios</p>
            <p className="text-xs text-slate-400">Marca uno o varios y agrégalos juntos</p>
          </div>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {(verTodos ? servicios : servicios?.slice(0, 8))?.map((s) => {
              const { icon: Icon, color } = servicioEstilo(s.nombre);
              const activo = seleccion.some((x) => x.servicioId === s.id);
              return (
                <button key={s.id} onClick={() => toggleServicio(s)} className={`flex min-h-[66px] items-start gap-3 rounded-xl border p-3 text-left transition-all ${activo ? "border-cyan-800 bg-primary-soft ring-1 ring-cyan-800" : "border-slate-200 hover:border-slate-300"}`}>
                  <div className={`grid h-9 w-9 shrink-0 place-items-center rounded-lg ${color}`}><Icon className="text-white" size={17} /></div>
                  <div className="min-w-0 flex-1">
                    <p className="text-xs font-medium leading-snug text-slate-700 line-clamp-2">{s.nombre}</p>
                    <p className="mt-0.5 text-xs font-semibold text-slate-400">{soles(s.precio)}</p>
                  </div>
                  <span className={`mt-0.5 grid h-5 w-5 shrink-0 place-items-center rounded border ${activo ? "border-cyan-800 bg-cyan-800 text-white" : "border-slate-300"}`}>{activo && <Check size={13} />}</span>
                </button>
              );
            })}
            {!verTodos && servicios && servicios.length > 8 && (
              <button onClick={() => setVerTodos(true)} className="flex items-center gap-3 rounded-xl border border-dashed border-slate-300 p-3 text-left hover:border-slate-400">
                <div className="grid h-9 w-9 shrink-0 place-items-center rounded-lg bg-slate-100"><MoreHorizontal className="text-slate-500" size={17} /></div>
                <div><p className="text-xs font-medium text-slate-700">Ver todos los servicios</p><p className="text-xs text-slate-400">+{servicios.length - 8} más</p></div>
              </button>
            )}
          </div>

          {/* Selección actual (importes editables) + un solo Agregar */}
          {seleccion.length > 0 && (
            <div className="mt-4 rounded-xl border border-slate-200 bg-slate-50/60 p-3">
              <p className="mb-2 text-xs font-medium text-slate-500">{seleccion.length} servicio(s) seleccionado(s)</p>
              <div className="space-y-2">
                {seleccion.map((s, idx) => (
                  <div key={s.servicioId} className="flex items-center gap-2 text-sm">
                    <span className="flex-1 truncate text-slate-600">{s.nombre}</span>
                    <div className="flex h-9 w-28 items-center rounded-lg border border-slate-300 bg-white px-2">
                      <span className="text-xs text-slate-400">S/</span>
                      <input type="number" min={0} step="0.10" value={s.importe || ""} onChange={(e) => setSeleccion((sel) => sel.map((x, i) => i === idx ? { ...x, importe: Number(e.target.value) } : x))} className="w-full bg-transparent px-1.5 text-sm focus:outline-none" />
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          <div className="mt-4 flex flex-wrap items-center justify-between gap-3">
            <p className="text-sm text-slate-500">Subtotal selección: <span className="font-semibold text-slate-700">{soles(totalSel)}</span></p>
            <button onClick={agregar} disabled={seleccion.length === 0} className="flex items-center gap-2 rounded-xl bg-gradient-to-r from-cyan-900 to-cyan-700 px-5 py-2.5 text-sm font-medium text-white shadow-glow transition-opacity hover:opacity-95 disabled:opacity-40 disabled:shadow-none">
              <ShoppingCart size={16} /> Agregar al ticket
            </button>
          </div>
        </div>

        {msg && <p className="rounded-lg bg-rose-50 px-4 py-2 text-sm text-rose-600">{msg}</p>}
      </div>

      {/* ===== Panel: Resumen del ticket ===== */}
      <div className="space-y-4">
        <div className="overflow-hidden rounded-2xl border border-slate-200/70 bg-white shadow-card xl:sticky xl:top-20">
          <div className="rounded-sm bg-gradient-to-br from-cyan-800 to-cyan-900 p-5 text-white shadow-card">
            <p className="text-sm font-semibold">Resumen del ticket</p>
            <div className="mt-3 flex justify-between">
              <div><p className="text-[11px] text-white/70">Ticket N°</p><p className="text-lg font-bold">{ticket ? (ticket as Ticket).numeroTicket : "—"}</p></div>
              <div className="text-right"><p className="text-[11px] text-white/70">Contador</p><p className="font-mono font-semibold">{ticket ? (ticket as Ticket).contador : "—"}</p></div>
            </div>
          </div>

          <div className="p-5">
            {consulta?.existe || alumnoId ? (
              <div className="mb-3 text-sm">
                <p className="font-semibold text-slate-800">{consulta?.nombreCompleto || `${apellidos} ${nombres}`}</p>
                <p className="text-slate-500">DNI: {dni}</p>
                {consulta?.programa && <p className="text-slate-500">Programa: {consulta.programa}</p>}
                {consulta?.turno && <p className="text-slate-500">Turno: {consulta.turno}</p>}
              </div>
            ) : <p className="mb-3 text-sm text-slate-400">Sin estudiante seleccionado</p>}

            <div className="border-t border-dashed border-slate-200 pt-3">
              <div className="flex justify-between text-[11px] font-medium uppercase text-slate-400"><span>Servicio</span><span>Importe</span></div>
              <div className="mt-2 space-y-2">
                {lineas.length === 0 && <p className="text-sm text-slate-400">Agrega servicios al ticket</p>}
                {lineas.map((l, i) => (
                  <div key={i} className="flex items-start justify-between gap-2 text-sm">
                    <span className="flex-1 text-slate-600">{l.nombre}</span>
                    <span className="font-medium text-slate-700">{soles(l.importe)}</span>
                    <button onClick={() => setLineas((ls) => ls.filter((_, j) => j !== i))} className="text-rose-400 hover:text-rose-600"><Trash2 size={14} /></button>
                  </div>
                ))}
              </div>
            </div>

            <div className="mt-3 flex items-center justify-between border-t border-dashed border-slate-200 pt-3">
              <span className="font-semibold text-slate-700">TOTAL</span>
              <span className="text-xl font-bold text-cyan-800">{soles(totalLineas)}</span>
            </div>
            {totalLineas > 0 && <p className="mt-1 text-xs text-slate-500">SON: {numeroALetras(totalLineas)}</p>}

            <button onClick={emitir} disabled={!puedeEmitir || emitiendo} className="mt-4 flex w-full items-center justify-center gap-2 rounded-sm bg-gradient-to-br from-cyan-800 to-cyan-900 p-5 px-4 py-2.5 text-sm font-medium text-white shadow-glow transition-opacity hover:opacity-95 disabled:opacity-40 disabled:shadow-none">
              <Printer size={16} /> {emitiendo ? "Emitiendo…" : "Imprimir ticket"}
            </button>
            <button disabled className="mt-2 flex w-full items-center justify-center gap-2 rounded-xl border border-slate-300 px-4 py-2.5 text-sm text-slate-500"><Eye size={16} /> Vista previa</button>
          </div>
        </div>

        {/* Accesos rápidos */}
        <div className="rounded-2xl border border-slate-200/70 bg-white p-5 shadow-card">
          <p className="mb-3 text-sm font-semibold text-slate-700">Accesos rápidos</p>
          <div className="grid grid-cols-2 gap-3 text-center">
            {[{ i: Search, t: "Buscar estudiante", k: "F2" }, { i: Receipt, t: "Historial de tickets", k: "F3" }, { i: Star, t: "Nuevo ticket", k: "F4" }, { i: Calendar, t: "Reporte diario", k: "F5" }].map((a) => (
              <button key={a.k} onClick={a.k === "F4" ? nuevo : undefined} className="rounded-xl border border-slate-200 p-3 transition-colors hover:bg-slate-50">
                <a.i className="mx-auto text-cyan-800" size={18} />
                <p className="mt-1.5 text-[11px] leading-tight text-slate-600">{a.t}</p>
                <p className="text-[10px] text-slate-400">{a.k}</p>
              </button>
            ))}
          </div>
        </div>

        {/* Estado de la impresora */}
        <div className="flex items-center gap-3 rounded-2xl border border-slate-200/70 bg-white p-4 shadow-card">
          <div className="grid h-10 w-10 place-items-center rounded-xl bg-emerald-50 text-emerald-600"><Printer size={18} /></div>
          <div className="flex-1">
            <p className="text-sm font-semibold text-slate-700">Estado de la impresora</p>
            <p className="flex items-center gap-1.5 text-xs text-emerald-600"><span className="h-1.5 w-1.5 rounded-full bg-emerald-500" /> Lista · 80 mm ESC/POS</p>
          </div>
        </div>
      </div>

      {/* Modal: historial de pagos del estudiante */}
      {historial && (
        <div className="fixed inset-0 z-50 grid place-items-center bg-slate-900/40 p-4 backdrop-blur-sm" onClick={() => setHistorial(null)}>
          <div className="max-h-[80vh] w-full max-w-2xl overflow-auto rounded-2xl bg-white p-6 shadow-xl" onClick={(e) => e.stopPropagation()}>
            <div className="mb-3 flex items-start justify-between">
              <div>
                <h3 className="text-lg font-semibold text-slate-800">Historial de pagos</h3>
                <p className="text-xs text-slate-500">DNI {dni}{consulta?.nombreCompleto ? ` · ${consulta.nombreCompleto}` : ""} · {historial.length} ticket(s) · Total {soles(historial.reduce((s, t) => s + t.total, 0))}</p>
              </div>
              <button onClick={() => setHistorial(null)} className="text-slate-400 hover:text-slate-600"><X size={18} /></button>
            </div>
            <table className="w-full text-sm">
              <thead>
                <tr className="text-left text-xs uppercase text-slate-400"><th className="pb-2">Ticket</th><th>Fecha</th><th className="text-right">Total</th><th>Usuario</th><th>Estado</th></tr>
              </thead>
              <tbody>
                {historial.map((t) => (
                  <tr key={t.id} className="border-t border-slate-100">
                    <td className="py-2 font-mono font-medium text-slate-700">{t.numeroTicket}</td>
                    <td className="text-slate-500">{new Date(t.fechaEmision).toLocaleString("es-PE")}</td>
                    <td className="text-right font-semibold text-emerald-600">{soles(t.total)}</td>
                    <td className="text-slate-500">{t.usuario}</td>
                    <td><span className={`rounded-md px-2 py-0.5 text-xs font-medium ${t.estado === "Emitido" ? "bg-emerald-50 text-emerald-600" : "bg-rose-50 text-rose-600"}`}>{t.estado}</span></td>
                  </tr>
                ))}
                {historial.length === 0 && <tr><td colSpan={5} className="py-4 text-center text-slate-400">Este estudiante aún no tiene pagos registrados.</td></tr>}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
