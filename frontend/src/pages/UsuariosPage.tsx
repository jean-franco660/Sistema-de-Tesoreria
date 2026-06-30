import { useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Plus, Power, Trash2, X, UserCog, ShieldCheck, Wallet, Users, XCircle } from "lucide-react";
import api, { apiError } from "../lib/api";
import type { UsuarioSistema } from "../types";
import { StatCard } from "../components/ui/stat";

interface Form { username: string; nombreCompleto: string; password: string; rol: string; }
const vacio: Form = { username: "", nombreCompleto: "", password: "", rol: "Finanzas" };

export default function UsuariosPage() {
  const qc = useQueryClient();
  const { data, isLoading, error } = useQuery({ queryKey: ["usuarios"], queryFn: async () => (await api.get<UsuarioSistema[]>("/usuarios")).data, retry: false });

  const [modal, setModal] = useState<Form | null>(null);
  const [msg, setMsg] = useState("");
  const [saving, setSaving] = useState(false);

  const refrescar = () => qc.invalidateQueries({ queryKey: ["usuarios"] });

  const admins = data?.filter((u) => u.rol === "Administrador").length ?? 0;
  const finanzas = data?.filter((u) => u.rol === "Finanzas").length ?? 0;
  const inactivos = data?.filter((u) => u.estado !== "Activo").length ?? 0;

  const crear = async () => {
    if (!modal) return;
    setSaving(true); setMsg("");
    try { await api.post("/usuarios", modal); setModal(null); refrescar(); }
    catch (e) { setMsg(apiError(e)); } finally { setSaving(false); }
  };

  const toggle = async (u: UsuarioSistema) => {
    try { await api.put(`/usuarios/${u.id}/estado`); refrescar(); } catch (e) { setMsg(apiError(e)); }
  };

  const eliminar = async (u: UsuarioSistema) => {
    if (!confirm(`¿Eliminar el usuario "${u.username}"?`)) return;
    try { await api.delete(`/usuarios/${u.id}`); refrescar(); } catch (e) { setMsg(apiError(e)); }
  };

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight text-slate-800">Usuarios</h1>
          <p className="text-sm text-slate-500">Cuentas de acceso al sistema</p>
        </div>
        <button onClick={() => { setModal({ ...vacio }); setMsg(""); }} className="flex items-center gap-2 rounded-lg bg-cyan-800 px-4 py-2 text-sm font-medium text-white hover:bg-cyan-700">
          <Plus size={16} /> Nuevo usuario
        </button>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard icon={<Users className="text-white" size={20} />} color="bg-blue-500" label="Total usuarios" value={`${data?.length ?? 0}`} hint="Activos e inactivos" />
        <StatCard icon={<ShieldCheck className="text-white" size={20} />} color="bg-cyan-800" label="Administradores" value={`${admins}`} hint="Acceso total" />
        <StatCard icon={<Wallet className="text-white" size={20} />} color="bg-emerald-500" label="Finanzas" value={`${finanzas}`} hint="Acceso limitado" />
        <StatCard icon={<XCircle className="text-white" size={20} />} color="bg-rose-500" label="Inactivos" value={`${inactivos}`} hint="Sin acceso" />
      </div>

      {(msg || error) && <p className="rounded-lg bg-rose-50 px-4 py-2 text-sm text-rose-600">{msg || apiError(error)}</p>}

      <div className="overflow-x-auto rounded-2xl border border-slate-200 bg-white p-5 shadow-card">
        <table className="w-full text-sm">
          <thead>
            <tr className="text-left text-xs uppercase text-slate-400"><th className="pb-2">Usuario</th><th>Nombre completo</th><th>Rol</th><th>Estado</th><th className="text-right">Acciones</th></tr>
          </thead>
          <tbody>
            {isLoading && <tr><td colSpan={5} className="py-3 text-slate-400">Cargando…</td></tr>}
            {data?.map((u) => (
              <tr key={u.id} className="border-t border-slate-100">
                <td className="py-2.5 font-mono font-medium text-slate-700">
                  <span className="mr-2 inline-grid h-7 w-7 place-items-center rounded-lg bg-primary-soft align-middle text-cyan-800"><UserCog size={15} /></span>{u.username}
                </td>
                <td className="text-slate-600">{u.nombreCompleto}</td>
                <td>
                  <span className={`inline-flex items-center gap-1 rounded-md px-2 py-0.5 text-xs font-medium ${u.rol === "Administrador" ? "bg-cyan-50 text-cyan-800" : "bg-blue-50 text-blue-600"}`}>
                    {u.rol === "Administrador" ? <ShieldCheck size={12} /> : <Wallet size={12} />} {u.rol}
                  </span>
                </td>
                <td><span className={`rounded-md px-2 py-0.5 text-xs font-medium ${u.estado === "Activo" ? "bg-emerald-50 text-emerald-600" : "bg-slate-100 text-slate-500"}`}>{u.estado}</span></td>
                <td className="text-right">
                  <button onClick={() => toggle(u)} className="rounded-md p-1.5 text-slate-500 hover:bg-slate-100" title="Activar/Desactivar"><Power size={15} /></button>
                  <button onClick={() => eliminar(u)} className="rounded-md p-1.5 text-rose-500 hover:bg-rose-50" title="Eliminar"><Trash2 size={15} /></button>
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
              <h3 className="text-lg font-semibold text-slate-800">Nuevo usuario</h3>
              <button onClick={() => setModal(null)} className="text-slate-400 hover:text-slate-600"><X size={18} /></button>
            </div>
            {msg && <p className="mb-3 rounded-lg bg-rose-50 px-3 py-2 text-sm text-rose-600">{msg}</p>}
            <div className="space-y-3">
              <Inp label="Usuario" value={modal.username} onChange={(v) => setModal({ ...modal, username: v })} />
              <Inp label="Nombre completo" value={modal.nombreCompleto} onChange={(v) => setModal({ ...modal, nombreCompleto: v })} />
              <Inp label="Contraseña" type="password" value={modal.password} onChange={(v) => setModal({ ...modal, password: v })} />
              <div>
                <label className="mb-1 block text-sm font-medium text-slate-600">Rol</label>
                <select value={modal.rol} onChange={(e) => setModal({ ...modal, rol: e.target.value })} className="h-10 w-full rounded-lg border border-slate-300 px-3 text-sm">
                  <option value="Finanzas">Finanzas</option>
                  <option value="Administrador">Administrador</option>
                </select>
              </div>
            </div>
            <div className="mt-5 flex justify-end gap-2">
              <button onClick={() => setModal(null)} className="rounded-lg border border-slate-300 px-4 py-2 text-sm hover:bg-slate-50">Cancelar</button>
              <button onClick={crear} disabled={!modal.username || !modal.nombreCompleto || modal.password.length < 6 || saving} className="rounded-lg bg-cyan-800 px-4 py-2 text-sm font-medium text-white hover:bg-cyan-700 disabled:opacity-50">
                {saving ? "Creando…" : "Crear usuario"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

function Inp({ label, value, onChange, type = "text" }: { label: string; value: string; onChange: (v: string) => void; type?: string }) {
  return (
    <div>
      <label className="mb-1 block text-sm font-medium text-slate-600">{label}</label>
      <input type={type} value={value} onChange={(e) => onChange(e.target.value)} className="h-10 w-full rounded-lg border border-slate-300 px-3 text-sm focus:outline-none focus:ring-2 focus:ring-cyan-800/40" />
    </div>
  );
}
