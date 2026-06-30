import { useState, useEffect, type ReactNode, type MouseEvent } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import {
  FileSpreadsheet, Printer, GraduationCap, Clock, Layers, ArrowLeft, Plus, Users, Trash2,
  ChevronRight, ChevronUp, FolderPlus, FileText, Boxes, Armchair, BarChart3, CalendarDays,
  Sun, Moon, Sunset, Scissors, Hammer, Zap, Utensils, Monitor, ChefHat, Bike, PenTool, Shirt, Briefcase,
} from "lucide-react";
import type { LucideIcon } from "lucide-react";
import api, { apiError } from "../lib/api";
import type { Matricula, Registro, Periodo, Programa, Turno } from "../types";
import { useConfig, logoSrc } from "../lib/useConfig";

const SECCIONES = ["U", "A", "B", "C", "D"];

export default function MatriculaPage() {
  const config = useConfig();
  const qc = useQueryClient();
  const [view, setView] = useState<"lista" | "crear" | "detalle">("lista");
  const [periodoId, setPeriodoId] = useState<number | "">("");

  // ---- crear registro ----
  const [cProg, setCProg] = useState<number | "">("");
  const [cTurno, setCTurno] = useState<number | "">("");
  const [cSeccion, setCSeccion] = useState("U");
  const [cProfesor, setCProfesor] = useState("");
  const [cModulo, setCModulo] = useState("");
  const [cMsg, setCMsg] = useState("");
  const [creando, setCreando] = useState(false);

  // ---- detalle (padrón) ----
  const [data, setData] = useState<Matricula | null>(null);
  const [registroSel, setRegistroSel] = useState<Registro | null>(null);
  const [descargando, setDescargando] = useState(false);
  const [aviso, setAviso] = useState("");

  // ---- filtros de la lista ----
  const [turnoFiltro, setTurnoFiltro] = useState<number | "">("");
  const [seccionFiltro, setSeccionFiltro] = useState("");
  const [colapsados, setColapsados] = useState<Set<string>>(new Set());
  const toggleColapso = (prog: string) => setColapsados((s) => { const n = new Set(s); n.has(prog) ? n.delete(prog) : n.add(prog); return n; });

  const { data: programas } = useQuery({ queryKey: ["programas"], queryFn: async () => (await api.get<Programa[]>("/programas?soloActivos=true")).data });
  const { data: turnos } = useQuery({ queryKey: ["turnos"], queryFn: async () => (await api.get<Turno[]>("/turnos")).data });
  const { data: periodos } = useQuery({ queryKey: ["periodos"], queryFn: async () => (await api.get<Periodo[]>("/periodos")).data });

  // Período activo (o el más reciente) por defecto.
  useEffect(() => {
    if (periodoId === "" && periodos && periodos.length) {
      const activo = periodos.find((p) => p.estado === "Abierto") ?? periodos[0];
      if (activo) setPeriodoId(activo.id);
    }
  }, [periodos, periodoId]);

  const { data: registros, dataUpdatedAt } = useQuery({
    queryKey: ["registros", periodoId],
    queryFn: async () => (await api.get<Registro[]>("/registros", { params: { periodoId: periodoId || undefined } })).data,
    enabled: periodoId !== "",
  });

  const verRegistro = async (r: Registro) => {
    setAviso("");
    try { const { data } = await api.get<Matricula>(`/registros/${r.id}`); setData(data); setRegistroSel(r); setView("detalle"); }
    catch (e) { setAviso(apiError(e)); }
  };

  const exportarExcel = async () => {
    if (!registroSel || !data) return;
    setDescargando(true);
    try {
      const res = await api.get(`/registros/${registroSel.id}/excel`, { responseType: "blob" });
      const url = URL.createObjectURL(res.data as Blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = `registro_${data.programa}_${data.turno}_${data.seccion}.xlsx`;
      a.click();
      URL.revokeObjectURL(url);
    } catch (e) { setAviso(apiError(e)); } finally { setDescargando(false); }
  };

  const crear = async () => {
    if (!cProg || !cTurno) { setCMsg("Selecciona programa y horario."); return; }
    setCreando(true); setCMsg("");
    try {
      await api.post("/registros", { programaId: Number(cProg), turnoId: Number(cTurno), seccion: cSeccion || "U", periodoId: periodoId || undefined, profesor: cProfesor || null, moduloFormativo: cModulo || null });
      await qc.invalidateQueries({ queryKey: ["registros"] });
      setCProg(""); setCTurno(""); setCSeccion("U"); setCProfesor(""); setCModulo(""); setView("lista");
      setAviso("✓ Registro de matrícula creado. Se irá llenando solo cuando registres pagos en Tesorería para esa combinación.");
    } catch (e) { setCMsg(apiError(e)); } finally { setCreando(false); }
  };

  const eliminar = async (r: Registro, ev: MouseEvent) => {
    ev.stopPropagation();
    if (!window.confirm(`¿Eliminar el registro vacío de ${r.programa} · ${r.turno} · Sección ${r.seccion}?`)) return;
    try { await api.delete(`/registros/${r.id}`); await qc.invalidateQueries({ queryKey: ["registros"] }); }
    catch (e) { window.alert(apiError(e)); }
  };

  // Secciones ya usadas para el programa+turno elegido (para no duplicar al crear).
  const seccionesUsadas = (registros ?? [])
    .filter((r) => r.programaId === Number(cProg) && r.turnoId === Number(cTurno))
    .map((r) => r.seccion);

  const fmt = (s?: string | null) => (s ? new Date(s).toLocaleDateString("es-PE", { timeZone: "UTC" }) : "");
  const periodoNombre = periodos?.find((p) => p.id === periodoId)?.nombre ?? "";

  // ───────────────────────── Detalle: padrón del registro (solo lectura) ─────────────────────────
  if (view === "detalle" && data) {
    return (
      <div className="space-y-4">
        <div className="flex flex-wrap items-center justify-between gap-2 print:hidden">
          <button onClick={() => { setData(null); setView("lista"); }} className="flex items-center gap-1.5 rounded-xl border border-slate-300 bg-white px-4 py-2 text-sm font-medium hover:bg-slate-50"><ArrowLeft size={16} /> Volver a registros</button>
          <div className="flex gap-2">
            <button onClick={exportarExcel} disabled={descargando} className="flex items-center gap-2 rounded-xl bg-emerald-50 px-4 py-2 text-sm font-medium text-emerald-700 hover:bg-emerald-100 disabled:opacity-50"><FileSpreadsheet size={16} /> {descargando ? "Generando…" : "Descargar Excel"}</button>
            <button onClick={() => window.print()} className="flex items-center gap-2 rounded-xl bg-gradient-to-r from-primary to-violet-600 px-4 py-2 text-sm font-medium text-white shadow-glow"><Printer size={16} /> Imprimir / PDF</button>
          </div>
        </div>

        {data.cantidad === 0 && (
          <p className="rounded-xl border border-amber-200 bg-amber-50 px-4 py-2.5 text-xs text-amber-700 print:hidden">
            Este registro aún no tiene estudiantes. Se completará automáticamente cuando registres pagos en <b>Tesorería</b> para
            <b> {data.programa}</b> · <b>{data.turno}</b> · Sección <b>{data.seccion}</b> en el período <b>{data.periodo}</b>.
          </p>
        )}

        <div id="matricula-print" className="overflow-x-auto rounded-2xl border border-slate-300 bg-white p-5 text-[11px] text-slate-800 shadow-card">
          <div className="relative mb-2 flex min-h-[3rem] items-center justify-center">
            <img src="/logo_minedu.png" alt="MINEDU" className="absolute left-0 top-1 h-10 w-auto object-contain" onError={(e) => { (e.currentTarget as HTMLImageElement).style.display = "none"; }} />
            <div className="translate-x-[5mm] text-center">
              <p className="text-sm font-bold tracking-wide">REGISTRO DE MATRÍCULA {data.periodo.replace("-", " - ")}</p>
              <p className="text-xs font-semibold">EDUCACIÓN TÉCNICO-PRODUCTIVA</p>
            </div>
            <img src={logoSrc(config)} alt="Logo" className="absolute right-[16mm] top-0 h-12 w-auto object-contain" />
          </div>

          <table className="mb-3 w-full table-fixed border-collapse">
            <colgroup><col className="w-[17%]" /><col className="w-[33%]" /><col className="w-[17%]" /><col className="w-[33%]" /></colgroup>
            <tbody>
              <tr><td colSpan={4} className="border border-slate-400 bg-slate-100 px-2 py-1 text-left text-[10px] font-bold">DATOS DEL CENTRO DE EDUCACIÓN TÉCNICO-PRODUCTIVA</td></tr>
              <tr><Celda k="Nombre del CETPRO:" v={data.nombreInstitucion} /><Celda k="DRE/GRE:" v={data.dreGre} /></tr>
              <tr><Celda k="Código modular:" v={data.codigoModular} /><Celda k="UGEL:" v={data.ugel} /></tr>
              <tr><Celda k="Resolución de creación o autorización:" v={data.resolucionCreacion} /><Celda k="Resolución de autorización del programa de estudios:" v={data.resolucionAutorizacion} /></tr>
              <tr><Celda k="Dirección:" v={data.direccion} /><Celda k="Periodo lectivo:" v={data.periodoLectivo} /></tr>
              <tr><Celda k="Programa de estudios:" v={data.programa} /><Celda k="Módulo formativo:" v={data.moduloFormativo ?? ""} /></tr>
              <tr><Celda k="Profesor:" v={data.profesor ?? ""} /><Celda k="Periodo:" v={data.periodoInicio ? `INICIO: ${fmt(data.periodoInicio)}` : ""} /></tr>
              <tr><Celda k="Nivel formativo:" v={data.nivelFormativo} /><Celda k="Modalidad del servicio educativo:" v={data.modalidadServicio} /></tr>
              <tr><Celda k="Tipo de plan de estudios:" v={data.tipoPlan} /><Celda k="Sección:" v={`"${data.seccion}"`} /></tr>
              <tr><Celda k="Turno:" v={data.turno} /><td className="border border-slate-400" colSpan={2} /></tr>
            </tbody>
          </table>

          <table className="w-full border-collapse">
            <thead>
              <tr className="bg-slate-100 text-center text-[9px] font-bold">
                <Th>N°</Th><Th>Código Matrícula</Th><Th className="text-left">Apellidos y Nombres (Orden Alfabético)</Th><Th>Sexo H-M</Th><Th>Fecha de nacimiento</Th><Th>Edad</Th><Th>N° de UD</Th><Th>N° de créditos</Th><Th>N° de teléfono</Th>
              </tr>
            </thead>
            <tbody>
              {data.estudiantes.map((e) => (
                <tr key={e.n} className="text-center">
                  <Td>{e.n}</Td><Td className="font-mono">{e.codigoMatricula}</Td><Td className="text-left">{e.apellidosNombres}</Td>
                  <Td>{e.sexo ?? ""}</Td><Td>{fmt(e.fechaNacimiento)}</Td><Td>{e.edad ?? ""}</Td><Td /><Td>20</Td><Td>{e.celular ?? ""}</Td>
                </tr>
              ))}
              {data.estudiantes.length === 0 && <tr><Td className="text-center text-slate-400" colSpan={9}>Sin estudiantes en este registro por ahora.</Td></tr>}
            </tbody>
          </table>
          <p className="mt-2 text-[10px] text-slate-500">Total de estudiantes matriculados: <b>{data.cantidad}</b></p>
        </div>
      </div>
    );
  }

  // ───────────────────────── Crear registro ─────────────────────────
  if (view === "crear") {
    return (
      <div className="space-y-5">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-xl font-semibold tracking-tight text-slate-800">Nuevo registro de matrícula</h1>
            <p className="text-sm text-slate-500">Crea un aula (puede estar vacía). Los estudiantes entran solos al pagar su matrícula en Tesorería.</p>
          </div>
          <button onClick={() => { setView("lista"); setCMsg(""); }} className="flex items-center gap-1.5 rounded-xl border border-slate-300 bg-white px-4 py-2 text-sm font-medium hover:bg-slate-50"><ArrowLeft size={16} /> Volver</button>
        </div>

        <div className="rounded-2xl border border-slate-200/70 bg-white p-5 shadow-card">
          <div className="grid gap-4 lg:grid-cols-2">
            <div>
              <label className="mb-1 flex items-center gap-1.5 text-xs font-medium text-slate-600"><GraduationCap size={14} /> Programa de estudios</label>
              <select value={cProg} onChange={(e) => setCProg(e.target.value ? Number(e.target.value) : "")} className="h-10 w-full rounded-lg border border-slate-300 px-3 text-sm">
                <option value="">— Seleccione —</option>
                {programas?.map((p) => <option key={p.id} value={p.id}>{p.nombre}</option>)}
              </select>
            </div>
            <div>
              <label className="mb-1 flex items-center gap-1.5 text-xs font-medium text-slate-600"><Clock size={14} /> Horario (turno)</label>
              <select value={cTurno} onChange={(e) => setCTurno(e.target.value ? Number(e.target.value) : "")} className="h-10 w-full rounded-lg border border-slate-300 px-3 text-sm">
                <option value="">— Seleccione —</option>
                {turnos?.map((t) => <option key={t.id} value={t.id}>{t.nombre}</option>)}
              </select>
            </div>
            <div>
              <label className="mb-1 flex items-center gap-1.5 text-xs font-medium text-slate-600"><GraduationCap size={14} /> Profesor / Docente</label>
              <input value={cProfesor} onChange={(e) => setCProfesor(e.target.value.toUpperCase())} placeholder="Apellidos y nombres del docente" className="h-10 w-full rounded-lg border border-slate-300 px-3 text-sm" />
            </div>
            <div>
              <label className="mb-1 flex items-center gap-1.5 text-xs font-medium text-slate-600"><Layers size={14} /> Módulo formativo <span className="font-normal text-slate-400">(opcional)</span></label>
              <input value={cModulo} onChange={(e) => setCModulo(e.target.value.toUpperCase())} placeholder="Ej. CORTE DE CABELLO" className="h-10 w-full rounded-lg border border-slate-300 px-3 text-sm" />
            </div>
          </div>

          <div className="mt-4">
            <label className="mb-1.5 flex items-center gap-1.5 text-xs font-medium text-slate-600"><Layers size={14} /> Sección</label>
            <div className="flex flex-wrap gap-2">
              {SECCIONES.map((s) => {
                const usada = seccionesUsadas.includes(s);
                const active = cSeccion === s;
                return (
                  <button key={s} disabled={usada} onClick={() => setCSeccion(s)}
                    className={`rounded-xl border px-4 py-2 text-sm font-medium transition ${usada ? "cursor-not-allowed border-slate-100 bg-slate-50 text-slate-300" : active ? "border-primary bg-primary-soft text-primary ring-1 ring-primary" : "border-slate-200 text-slate-600 hover:border-primary/50 hover:bg-slate-50"}`}>
                    Sección {s}{usada ? " · ya existe" : ""}
                  </button>
                );
              })}
            </div>
            <p className="mt-2 text-[11px] text-slate-400">Período: <b className="text-slate-600">{periodoNombre || "activo"}</b>. La sección «U» es la única/general cuando no hay paralelos.</p>
          </div>

          {cMsg && <p className="mt-4 rounded-lg bg-rose-50 px-3 py-2 text-sm text-rose-600">{cMsg}</p>}

          <div className="mt-5 flex justify-end">
            <button onClick={crear} disabled={creando} className="flex items-center gap-2 rounded-xl bg-primary px-5 py-2.5 text-sm font-medium text-white shadow-glow hover:bg-primary-hover disabled:opacity-50">
              <FolderPlus size={16} /> {creando ? "Creando…" : "Crear registro"}
            </button>
          </div>
        </div>
      </div>
    );
  }

  // ───────────────────────── Lista de registros (solo ver) ─────────────────────────
  // Filtra por horario/sección y agrupa por programa.
  const filtrados = (registros ?? []).filter((r) =>
    (turnoFiltro === "" || r.turnoId === turnoFiltro) &&
    (seccionFiltro === "" || r.seccion === seccionFiltro));

  const grupos = Array.from(
    filtrados.reduce((m, r) => { (m.get(r.programa) ?? m.set(r.programa, []).get(r.programa)!).push(r); return m; }, new Map<string, Registro[]>())
  ).sort((a, b) => a[0].localeCompare(b[0]));

  const totalEstudiantes = filtrados.reduce((s, a) => s + a.cantidad, 0);
  const minsActual = dataUpdatedAt ? Math.floor((Date.now() - dataUpdatedAt) / 60000) : -1;
  const actualizado = minsActual < 0 ? "—" : minsActual <= 0 ? "ahora" : `hace ${minsActual} min`;

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="flex items-start gap-3">
          <div className="grid h-11 w-11 shrink-0 place-items-center rounded-xl bg-gradient-to-br from-cyan-600 to-slate-800 text-white shadow-glow"><FileText size={20} /></div>
          <div>
            <h1 className="text-xl font-bold tracking-tight text-slate-800">Registros de Matrícula</h1>
            <p className="max-w-2xl text-sm text-slate-500">Cada registro es un aula (programa · horario · sección). Haz clic para ver sus estudiantes; se actualizan solos desde Tesorería.</p>
          </div>
        </div>
        <button onClick={() => { setView("crear"); setAviso(""); }} className="flex items-center gap-2 rounded-xl bg-gradient-to-r from-cyan-600 to-slate-800 px-4 py-2.5 text-sm font-semibold text-white shadow-glow transition hover:opacity-95">
          <Plus size={16} /> Crear registro
        </button>
      </div>

      {aviso && <p className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-2.5 text-sm text-emerald-700">{aviso}</p>}

      {/* Filtros + estadísticas */}
      <div className="rounded-2xl border border-slate-200/70 bg-white p-4 shadow-card">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div className="flex flex-wrap items-center gap-2.5">
            <FiltroIcon icon={<CalendarDays size={15} />} label="Período">
              <select value={periodoId} onChange={(e) => setPeriodoId(e.target.value ? Number(e.target.value) : "")} className="bg-transparent text-sm font-semibold text-slate-700 focus:outline-none">
                {periodos?.map((p) => <option key={p.id} value={p.id}>{p.nombre}{p.estado === "Abierto" ? " · activo" : ""}</option>)}
              </select>
            </FiltroIcon>
            <FiltroIcon icon={<Clock size={15} />} label="Horario">
              <select value={turnoFiltro} onChange={(e) => setTurnoFiltro(e.target.value ? Number(e.target.value) : "")} className="bg-transparent text-sm font-semibold text-slate-700 focus:outline-none">
                <option value="">Todos</option>
                {turnos?.map((t) => <option key={t.id} value={t.id}>{t.nombre}</option>)}
              </select>
            </FiltroIcon>
            <FiltroIcon icon={<Users size={15} />} label="Sección (clase)">
              <select value={seccionFiltro} onChange={(e) => setSeccionFiltro(e.target.value)} className="bg-transparent text-sm font-semibold text-slate-700 focus:outline-none">
                <option value="">Todas</option>
                {SECCIONES.map((s) => <option key={s} value={s}>Sección {s}</option>)}
              </select>
            </FiltroIcon>
            {(turnoFiltro !== "" || seccionFiltro !== "") && (
              <button onClick={() => { setTurnoFiltro(""); setSeccionFiltro(""); }} className="text-xs font-medium text-red-600 hover:underline">Limpiar</button>
            )}
          </div>

          <div className="flex flex-wrap gap-2.5">
            <Stat icon={<Boxes size={16} />} value={grupos.length} label="Programas" tone="violet" />
            <Stat icon={<Armchair size={16} />} value={filtrados.length} label="Aulas" tone="blue" />
            <Stat icon={<Users size={16} />} value={totalEstudiantes} label="Estudiantes" tone="emerald" />
            <Stat icon={<BarChart3 size={16} />} value="Actualizado" label={actualizado} tone="amber" />
          </div>
        </div>
      </div>

      {/* Programas (acordeón) con sus aulas */}
      <div className="space-y-3.5">
        {grupos.map(([programa, aulas]) => {
          const total = aulas.reduce((s, a) => s + a.cantidad, 0);
          const { Icon } = programaEstilo(programa);
          const abierto = !colapsados.has(programa);
          const ordenadas = [...aulas].sort((a, b) => a.turno.localeCompare(b.turno) || a.seccion.localeCompare(b.seccion));
          return (
            <div key={programa} className="overflow-hidden rounded-2xl border border-slate-200/90 border-l-4  bg-white shadow-card">
              <button onClick={() => toggleColapso(programa)} className="flex w-full flex-wrap items-center gap-2.5 px-5 py-3.5 text-left transition hover:bg-slate-50/60">
                <div className="grid h-10 w-10 shrink-0 place-items-center rounded-xl bg-gradient-to-br from-cyan-700 to-slate-800 text-white"><Icon size={18} /></div>
                <p className="mr-1 text-sm font-bold uppercase tracking-wide text-slate-800">{programa}</p>
                <span className="flex items-center gap-1 rounded-full bg-green-100 px-2.5 py-1 text-[11px] font-semibold text-green-900"><Users size={12} /> {total} estudiantes</span>
                <span className="flex items-center gap-1 rounded-full bg-red-100 px-2.5 py-1 text-[11px] font-semibold text-cyan-800"><Layers size={12} /> {aulas.length} {aulas.length === 1 ? "aula" : "aulas"}</span>
                <ChevronUp size={18} className={`ml-auto shrink-0 text-slate-400 transition-transform ${abierto ? "" : "rotate-180"}`} />
              </button>

              {abierto && (
                <div className="grid gap-3 px-4 pb-4 sm:grid-cols-2 lg:grid-cols-3">
                  {ordenadas.map((r, i) => {
                    const accent = ACCENTS[i % ACCENTS.length];
                    const { Icon: TIcon, bg, color } = turnoEstilo(r.turno);
                    return (
                      <button key={r.id} onClick={() => verRegistro(r)} className="group relative flex items-center gap-3 rounded-xl border border-slate-200 bg-white p-3.5 text-left shadow-sm transition hover:-translate-y-0.5 hover:border-slate-300 hover:shadow-md" style={{ borderTopWidth: 3, borderTopColor: accent }}>
                        <div className="grid h-10 w-10 shrink-0 place-items-center rounded-full" style={{ backgroundColor: bg }}><TIcon size={17} style={{ color }} /></div>
                        <div className="min-w-0 flex-1">
                          <p className="text-[10px] font-semibold uppercase tracking-wide text-slate-400">{r.turno}</p>
                          <p className="text-sm font-bold text-slate-800">Sec {r.seccion}</p>
                          <p className="mt-0.5 flex items-center gap-1 text-[11px] text-slate-500"><Users size={11} /> {r.cantidad} {r.cantidad === 1 ? "estudiante" : "estudiantes"}</p>
                        </div>
                        {r.cantidad === 0 && <span onClick={(e) => eliminar(r, e)} className="rounded-md p-1 text-slate-300 hover:bg-rose-50 hover:text-rose-500" title="Eliminar aula vacía"><Trash2 size={14} /></span>}
                        <ChevronRight size={18} className="shrink-0 text-slate-300 group-hover:text-cyan-800" />
                      </button>
                    );
                  })}
                </div>
              )}
            </div>
          );
        })}
        {grupos.length === 0 && (
          <div className="rounded-2xl border border-dashed border-slate-300 bg-white p-10 text-center">
            <p className="text-sm text-slate-400">{(registros?.length ?? 0) === 0 ? "No hay registros de matrícula en este período." : "Ningún aula coincide con los filtros."}</p>
            <button onClick={() => { setView("crear"); setTurnoFiltro(""); setSeccionFiltro(""); }} className="mt-3 inline-flex items-center gap-2 rounded-xl bg-primary px-4 py-2 text-sm font-medium text-white shadow-glow hover:bg-primary-hover"><Plus size={15} /> Crear registro</button>
          </div>
        )}
      </div>
    </div>
  );
}

const ACCENTS = ["#3b82f6", "#f59e0b", "#ef4444", "#8b5cf6", "#10b981", "#06b6d4", "#ec4899"];

const TONES: Record<string, { bg: string; icon: string }> = {
  violet: { bg: "bg-violet-50", icon: "bg-violet-100 text-violet-600" },
  blue: { bg: "bg-cyan-50", icon: "bg-cyan-100 text-cyan-800" },
  emerald: { bg: "bg-emerald-50", icon: "bg-emerald-100 text-emerald-600" },
  amber: { bg: "bg-amber-50", icon: "bg-amber-100 text-amber-600" },
};

function FiltroIcon({ icon, label, children }: { icon: ReactNode; label: string; children: ReactNode }) {
  return (
    <div className="flex items-center gap-2 rounded-xl border border-slate-200 bg-slate-50/70 px-3 py-1.5">
      <span className="text-slate-400">{icon}</span>
      <div className="flex flex-col leading-tight">
        <span className="text-[10px] font-medium uppercase tracking-wide text-slate-400">{label}</span>
        {children}
      </div>
    </div>
  );
}

function Stat({ icon, value, label, tone }: { icon: ReactNode; value: number | string; label: string; tone: keyof typeof TONES }) {
  const t = TONES[tone];
  return (
    <div className={`flex items-center gap-2.5 rounded-xl ${t.bg} px-3.5 py-2.5`}>
      <div className={`grid h-9 w-9 shrink-0 place-items-center rounded-lg ${t.icon}`}>{icon}</div>
      <div className="leading-tight">
        <p className="text-base font-bold text-slate-800">{value}</p>
        <p className="text-[11px] text-slate-500">{label}</p>
      </div>
    </div>
  );
}

/** Ícono representativo de cada programa de estudios. */
function programaEstilo(nombre: string): { Icon: LucideIcon } {
  const n = nombre.toLowerCase();
  if (n.includes("soporte") || n.includes("computo")) return { Icon: Monitor };
  if (n.includes("panaderia") || n.includes("pasteleria")) return { Icon: ChefHat };
  if (n.includes("gastronomia")) return { Icon: Utensils };
  if (n.includes("apoyo") || n.includes("administrativo")) return { Icon: Briefcase };
  if (n.includes("corte") || n.includes("ensamblaje") || n.includes("peluqueria") || n.includes("barberia")) return { Icon: Scissors };
  if (n.includes("tejeduria") || n.includes("produccion")) return { Icon: Shirt };
  if (n.includes("electr")) return { Icon: Zap };
  if (n.includes("madera") || n.includes("carpinteria") || n.includes("fabricacion")) return { Icon: Hammer };
  if (n.includes("mecanica") || n.includes("motos") || n.includes("vehiculos")) return { Icon: Bike };
  if (n.includes("dibujo")) return { Icon: PenTool };
  return { Icon: GraduationCap };
}

/** Ícono y colores según el turno (mañana/tarde/noche). */
function turnoEstilo(turno: string): { Icon: LucideIcon; bg: string; color: string } {
  const t = turno.toUpperCase();
  if (t.includes("NOCHE")) return { Icon: Moon, bg: "#eef2ff", color: "#6366f1" };
  if (t.includes("TARDE")) return { Icon: Sunset, bg: "#fff7ed", color: "#f97316" };
  return { Icon: Sun, bg: "#fffbeb", color: "#f59e0b" };
}

function Celda({ k, v }: { k: string; v: string }) {
  return (
    <>
      <td className="border border-slate-400 bg-slate-50 px-2 py-1 align-top text-[9px] font-bold leading-tight text-slate-700">{k}</td>
      <td className="border border-slate-400 px-2 py-1 align-top text-[10px] leading-tight text-slate-800">{v}</td>
    </>
  );
}
function Th({ children, className = "" }: { children?: ReactNode; className?: string }) {
  return <th className={`border border-slate-400 px-1 py-1 ${className}`}>{children}</th>;
}
function Td({ children, className = "", colSpan }: { children?: ReactNode; className?: string; colSpan?: number }) {
  return <td colSpan={colSpan} className={`border border-slate-300 px-1 py-1 ${className}`}>{children}</td>;
}
