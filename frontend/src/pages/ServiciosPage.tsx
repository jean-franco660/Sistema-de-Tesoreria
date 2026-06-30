import { useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Plus, Pencil, Power, Trash2, X, ClipboardList, CheckCircle2, XCircle, TrendingUp } from "lucide-react";
import api, { apiError } from "../lib/api";
import type { Dashboard, Servicio } from "../types";
import { StatCard } from "../components/ui/stat";
import { soles } from "../lib/utils";

interface Form { id?: number; nombre: string; precio: number; }

export default function ServiciosPage() {
  const qc = useQueryClient();
  const { data, isLoading } = useQuery({ queryKey: ["servicios-admin"], queryFn: async () => (await api.get<Servicio[]>("/servicios")).data });
  const { data: dash } = useQuery({ queryKey: ["dashboard"], queryFn: async () => (await api.get<Dashboard>("/dashboard")).data });

  const [modal, setModal] = useState<Form | null>(null);
  const [msg, setMsg] = useState("");
  const [saving, setSaving] = useState(false);

  const activos = data?.filter((s) => s.estado === "Activo").length ?? 0;
  const inactivos = (data?.length ?? 0) - activos;
  const masCobrado = dash?.serviciosMasCobrados?.[0]?.nombre ?? "—";

  const refrescar = () => { qc.invalidateQueries({ queryKey: ["servicios-admin"] }); qc.invalidateQueries({ queryKey: ["servicios"] }); };

  const guardar = async () => {
    if (!modal) return;
    setSaving(true); setMsg("");
    try {
      if (modal.id) await api.put(`/servicios/${modal.id}`, { id: modal.id, nombre: modal.nombre, precio: modal.precio, activo: true });
      else await api.post("/servicios", { nombre: modal.nombre, precio: modal.precio });
      setModal(null); refrescar();
    } catch (e) { setMsg(apiError(e)); } finally { setSaving(false); }
  };

  const toggle = async (s: Servicio) => {
    try { await api.put(`/servicios/${s.id}`, { id: s.id, nombre: s.nombre, precio: s.precio, activo: s.estado !== "Activo" }); refrescar(); }
    catch (e) { setMsg(apiError(e)); }
  };

  const eliminar = async (s: Servicio) => {
    if (!confirm(`¿Eliminar el servicio "${s.nombre}"?`)) return;
    try { await api.delete(`/servicios/${s.id}`); refrescar(); } catch (e) { setMsg(apiError(e)); }
  };

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight text-slate-800">Servicios</h1>
          <p className="text-sm text-slate-500">Conceptos cobrables por tesorería</p>
        </div>
        <button onClick={() => { setModal({ nombre: "", precio: 0 }); setMsg(""); }} className="flex items-center gap-2 rounded-xl bg-cyan-800 px-4 py-2 text-sm font-medium text-white shadow-glow transition-colors hover:bg-cyan-800">
          <Plus size={16} /> Nuevo servicio
        </button>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard icon={<ClipboardList className="text-white" size={20} />} color="bg-blue-500" label="Total servicios" value={`${data?.length ?? 0}`} />
        <StatCard icon={<TrendingUp className="text-white" size={20} />} color="bg-amber-500" label="Más cobrado" value={masCobrado.length > 18 ? masCobrado.slice(0, 18) + "…" : masCobrado} hint="Por monto" />
        <StatCard icon={<CheckCircle2 className="text-white" size={20} />} color="bg-emerald-500" label="Servicios activos" value={`${activos}`} />
        <StatCard icon={<XCircle className="text-white" size={20} />} color="bg-rose-500" label="Servicios inactivos" value={`${inactivos}`} />
      </div>

      {msg && <p className="rounded-lg bg-rose-50 px-4 py-2 text-sm text-rose-600">{msg}</p>}

      <div className="overflow-x-auto rounded-2xl border border-slate-200/70 bg-white p-5 shadow-card">
        <table className="w-full text-sm">
          <thead>
            <tr className="text-left text-xs uppercase text-slate-400"><th className="pb-2">Servicio</th><th className="text-right">Precio</th><th>Estado</th><th className="text-right">Acciones</th></tr>
          </thead>
          <tbody>
            {isLoading && <tr><td colSpan={4} className="py-3 text-slate-400">Cargando…</td></tr>}
            {data?.map((s) => (
              <tr key={s.id} className="border-t border-slate-100">
                <td className="py-2.5 font-medium text-slate-700">
                  <span className="mr-2 inline-grid h-7 w-7 place-items-center rounded-lg bg-primary-soft align-middle text-cyan-800"><ClipboardList size={15} /></span>
                  {s.nombre}
                </td>
                <td className="text-right font-semibold text-slate-700">{soles(s.precio)}</td>
                <td><span className={`rounded-md px-2 py-0.5 text-xs font-medium ${s.estado === "Activo" ? "bg-emerald-50 text-emerald-600" : "bg-slate-100 text-slate-500"}`}>{s.estado}</span></td>
                <td className="text-right">
                  <button onClick={() => setModal({ id: s.id, nombre: s.nombre, precio: s.precio })} className="rounded-md p-1.5 text-slate-500 hover:bg-slate-100" title="Editar"><Pencil size={15} /></button>
                  <button onClick={() => toggle(s)} className="rounded-md p-1.5 text-slate-500 hover:bg-slate-100" title="Activar/Desactivar"><Power size={15} /></button>
                  <button onClick={() => eliminar(s)} className="rounded-md p-1.5 text-rose-500 hover:bg-rose-50" title="Eliminar"><Trash2 size={15} /></button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {modal && (
        <div className="fixed inset-0 z-50 grid place-items-center bg-slate-900/40 p-4 backdrop-blur-sm" onClick={() => setModal(null)}>
          <div className="w-full max-w-md rounded-2xl bg-white p-6 shadow-xl" onClick={(e) => e.stopPropagation()}>
            <div className="mb-4 flex items-center justify-between">
              <h3 className="text-lg font-semibold text-slate-800">{modal.id ? "Editar servicio" : "Nuevo servicio"}</h3>
              <button onClick={() => setModal(null)} className="text-slate-400 hover:text-slate-600"><X size={18} /></button>
            </div>
            {msg && <p className="mb-3 rounded-lg bg-rose-50 px-3 py-2 text-sm text-rose-600">{msg}</p>}
            <label className="mb-1 block text-sm font-medium text-slate-600">Nombre</label>
            <input autoFocus value={modal.nombre} onChange={(e) => setModal({ ...modal, nombre: e.target.value })}
              className="mb-3 h-10 w-full rounded-lg border border-slate-300 px-3 text-sm focus:outline-none focus:ring-2 focus:ring-cyan-800/40" />
            <label className="mb-1 block text-sm font-medium text-slate-600">Precio sugerido (S/)</label>
            <input type="number" min={0} step="0.10" value={modal.precio || ""} onChange={(e) => setModal({ ...modal, precio: Number(e.target.value) })}
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
