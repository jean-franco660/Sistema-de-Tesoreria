import { useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Plus, Pencil, Power, Trash2, X, GraduationCap, Users, CheckCircle2, XCircle } from "lucide-react";
import api, { apiError } from "../lib/api";
import type { Alumno, Programa } from "../types";
import { StatCard } from "../components/ui/stat";

export default function ProgramasPage() {
  const qc = useQueryClient();
  const { data, isLoading } = useQuery({ queryKey: ["programas-admin"], queryFn: async () => (await api.get<Programa[]>("/programas")).data });
  const { data: alumnos } = useQuery({ queryKey: ["alumnos-count"], queryFn: async () => (await api.get<Alumno[]>("/alumnos")).data });

  const activos = data?.filter((p) => p.estado === "Activo").length ?? 0;
  const inactivos = (data?.length ?? 0) - activos;

  const [modal, setModal] = useState<{ id?: number; nombre: string } | null>(null);
  const [msg, setMsg] = useState("");
  const [saving, setSaving] = useState(false);

  const refrescar = () => { qc.invalidateQueries({ queryKey: ["programas-admin"] }); qc.invalidateQueries({ queryKey: ["programas"] }); };

  const guardar = async () => {
    if (!modal) return;
    setSaving(true); setMsg("");
    try {
      if (modal.id) await api.put(`/programas/${modal.id}`, { id: modal.id, nombre: modal.nombre, activo: true });
      else await api.post("/programas", { nombre: modal.nombre });
      setModal(null); refrescar();
    } catch (e) { setMsg(apiError(e)); } finally { setSaving(false); }
  };

  const toggle = async (p: Programa) => {
    try { await api.put(`/programas/${p.id}`, { id: p.id, nombre: p.nombre, activo: p.estado !== "Activo" }); refrescar(); }
    catch (e) { setMsg(apiError(e)); }
  };

  const eliminar = async (p: Programa) => {
    if (!confirm(`¿Eliminar el programa "${p.nombre}"?`)) return;
    try { await api.delete(`/programas/${p.id}`); refrescar(); } catch (e) { setMsg(apiError(e)); }
  };

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight text-slate-800">Programas de estudio</h1>
          <p className="text-sm text-slate-500">Gestión de carreras técnico-productivas</p>
        </div>
        <button onClick={() => setModal({ nombre: "" })} className="flex items-center gap-2 rounded-lg bg-cyan-800 px-4 py-2 text-sm font-medium text-white hover:bg-cyan-800">
          <Plus size={16} /> Nuevo programa
        </button>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard icon={<GraduationCap className="text-white" size={20} />} color="bg-blue-500" label="Total programas" value={`${data?.length ?? 0}`} />
        <StatCard icon={<Users className="text-white" size={20} />} color="bg-violet-500" label="Estudiantes" value={`${alumnos?.length ?? 0}`} hint="Matriculados" />
        <StatCard icon={<CheckCircle2 className="text-white" size={20} />} color="bg-emerald-500" label="Programas activos" value={`${activos}`} />
        <StatCard icon={<XCircle className="text-white" size={20} />} color="bg-rose-500" label="Programas inactivos" value={`${inactivos}`} />
      </div>

      {msg && <p className="rounded-lg bg-rose-50 px-4 py-2 text-sm text-rose-600">{msg}</p>}

      <div className="overflow-x-auto rounded-2xl border border-slate-200 bg-white p-5 shadow-card">
        <table className="w-full text-sm">
          <thead>
            <tr className="text-left text-xs uppercase text-slate-400"><th className="pb-2">Programa</th><th>Estado</th><th className="text-right">Acciones</th></tr>
          </thead>
          <tbody>
            {isLoading && <tr><td colSpan={3} className="py-3 text-slate-400">Cargando…</td></tr>}
            {data?.map((p) => (
              <tr key={p.id} className="border-t border-slate-100">
                <td className="py-2.5 font-medium text-slate-700">
                  <span className="mr-2 inline-grid h-7 w-7 place-items-center rounded-lg bg-primary-soft align-middle text-cyan-800"><GraduationCap size={15} /></span>
                  {p.nombre}
                </td>
                <td>
                  <span className={`rounded-md px-2 py-0.5 text-xs font-medium ${p.estado === "Activo" ? "bg-emerald-50 text-emerald-600" : "bg-slate-100 text-slate-500"}`}>{p.estado}</span>
                </td>
                <td className="text-right">
                  <button onClick={() => setModal({ id: p.id, nombre: p.nombre })} className="rounded-md p-1.5 text-slate-500 hover:bg-slate-100" title="Editar"><Pencil size={15} /></button>
                  <button onClick={() => toggle(p)} className="rounded-md p-1.5 text-slate-500 hover:bg-slate-100" title="Activar/Desactivar"><Power size={15} /></button>
                  <button onClick={() => eliminar(p)} className="rounded-md p-1.5 text-rose-500 hover:bg-rose-50" title="Eliminar"><Trash2 size={15} /></button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {modal && (
        <div className="fixed inset-0 z-50 grid place-items-center bg-black/30 p-4" onClick={() => setModal(null)}>
          <div className="w-full max-w-md rounded-2xl bg-white p-6 shadow-xl" onClick={(e) => e.stopPropagation()}>
            <div className="mb-4 flex items-center justify-between">
              <h3 className="text-lg font-semibold text-slate-800">{modal.id ? "Editar programa" : "Nuevo programa"}</h3>
              <button onClick={() => setModal(null)} className="text-slate-400 hover:text-slate-600"><X size={18} /></button>
            </div>
            <label className="mb-1 block text-sm font-medium text-slate-600">Nombre</label>
            <input autoFocus value={modal.nombre} onChange={(e) => setModal({ ...modal, nombre: e.target.value })}
              className="h-10 w-full rounded-lg border border-slate-300 px-3 text-sm focus:outline-none focus:ring-2 focus:ring-cyan-800/40" />
            <div className="mt-5 flex justify-end gap-2">
              <button onClick={() => setModal(null)} className="rounded-lg border border-slate-300 px-4 py-2 text-sm hover:bg-slate-50">Cancelar</button>
              <button onClick={guardar} disabled={!modal.nombre.trim() || saving} className="rounded-lg bg-cyan-800 px-4 py-2 text-sm font-medium text-white hover:bg-cyan-800 disabled:opacity-50">
                {saving ? "Guardando…" : "Guardar"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
