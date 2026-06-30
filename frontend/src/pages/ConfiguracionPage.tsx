import { useEffect, useState, type ChangeEvent } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Building2, User, ShieldCheck, Upload, Save } from "lucide-react";
import api, { apiError } from "../lib/api";
import { useAuth } from "../context/AuthContext";
import { logoSrc } from "../lib/useConfig";
import type { Configuracion } from "../types";

const vacio: Configuracion = {
  id: 0, nombreInstitucion: "", ciudad: "", codigoModular: "", direccion: "",
  baseLegal: "", tituloComprobante: "", tipoComprobante: "", logoBase64: null,
};

export default function ConfiguracionPage() {
  const { user, isAdmin } = useAuth();
  const qc = useQueryClient();
  const { data } = useQuery({ queryKey: ["config"], queryFn: async () => (await api.get<Configuracion>("/configuracion")).data });

  const [form, setForm] = useState<Configuracion>(vacio);
  const [msg, setMsg] = useState("");
  const [ok, setOk] = useState(false);
  const [saving, setSaving] = useState(false);

  useEffect(() => { if (data) setForm(data); }, [data]);

  const onLogo = (e: ChangeEvent<HTMLInputElement>) => {
    const f = e.target.files?.[0];
    if (!f) return;
    if (f.size > 800 * 1024) { setMsg("El logo no debe superar 800 KB."); return; }
    const reader = new FileReader();
    reader.onload = () => setForm((s) => ({ ...s, logoBase64: reader.result as string }));
    reader.readAsDataURL(f);
  };

  const guardar = async () => {
    setSaving(true); setMsg(""); setOk(false);
    try {
      await api.put("/configuracion", form);
      qc.invalidateQueries({ queryKey: ["config"] });
      setOk(true);
    } catch (e) { setMsg(apiError(e)); } finally { setSaving(false); }
  };

  const campo = (label: string, key: keyof Configuracion) => (
    <div>
      <label className="mb-1 block text-xs font-medium text-slate-500">{label}</label>
      <input value={(form[key] as string) ?? ""} disabled={!isAdmin} onChange={(e) => setForm((s) => ({ ...s, [key]: e.target.value }))}
        className="h-10 w-full rounded-lg border border-slate-300 px-3 text-sm focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20 disabled:bg-slate-50 disabled:text-slate-500" />
    </div>
  );

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight text-slate-800">Configuración</h1>
          <p className="text-sm text-slate-500">Personaliza la institución, el logo y el comprobante (rebranding)</p>
        </div>
        {isAdmin && (
          <button onClick={guardar} disabled={saving || !form.nombreInstitucion} className="flex items-center gap-2 rounded-xl bg-gradient-to-r from-primary to-violet-600 px-4 py-2 text-sm font-medium text-white shadow-glow disabled:opacity-50">
            <Save size={16} /> {saving ? "Guardando…" : "Guardar cambios"}
          </button>
        )}
      </div>

      {msg && <p className="rounded-lg bg-rose-50 px-4 py-2 text-sm text-rose-600">{msg}</p>}
      {ok && <p className="rounded-lg bg-emerald-50 px-4 py-2 text-sm text-emerald-700">✓ Cambios guardados. El logo y los datos ya se reflejan en el sistema y en el ticket.</p>}
      {!isAdmin && <p className="rounded-lg bg-amber-50 px-4 py-2 text-sm text-amber-700">Solo un Administrador puede editar la configuración.</p>}

      <div className="grid gap-5 lg:grid-cols-3">
        {/* Logo */}
        <div className="rounded-2xl border border-slate-200/70 bg-white p-5 shadow-card">
          <p className="mb-3 flex items-center gap-2 text-sm font-semibold text-slate-700"><Building2 size={16} className="text-primary" /> Logo institucional</p>
          <div className="grid place-items-center rounded-xl border border-dashed border-slate-300 bg-slate-50 p-6">
            <img src={logoSrc(form)} alt="Logo" className="h-28 w-auto max-h-28 object-contain" />
          </div>
          {isAdmin && (
            <label className="mt-3 flex cursor-pointer items-center justify-center gap-2 rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm text-slate-600 hover:bg-slate-50">
              <Upload size={15} /> Cambiar logo
              <input type="file" accept="image/*" className="hidden" onChange={onLogo} />
            </label>
          )}
          <p className="mt-2 text-center text-[11px] text-slate-400">PNG/JPG/SVG · máx 800 KB</p>
        </div>

        {/* Datos */}
        <div className="rounded-2xl border border-slate-200/70 bg-white p-5 shadow-card lg:col-span-2">
          <p className="mb-3 flex items-center gap-2 text-sm font-semibold text-slate-700"><Building2 size={16} className="text-primary" /> Datos de la institución</p>
          <div className="grid gap-3 sm:grid-cols-2">
            <div className="sm:col-span-2">{campo("Nombre de la institución", "nombreInstitucion")}</div>
            {campo("Ciudad", "ciudad")}
            {campo("Código modular", "codigoModular")}
            {campo("Dirección", "direccion")}
            {campo("Base legal", "baseLegal")}
            <div className="sm:col-span-2">{campo("Título del comprobante", "tituloComprobante")}</div>
            {campo("Tipo de comprobante", "tipoComprobante")}
          </div>
        </div>
      </div>

      {/* Sesión */}
      <div className="rounded-2xl border border-slate-200/70 bg-white p-5 shadow-card lg:max-w-md">
        <p className="mb-3 flex items-center gap-2 text-sm font-semibold text-slate-700"><User size={16} className="text-primary" /> Sesión actual</p>
        <Row label="Usuario" value={user?.username ?? "—"} />
        <Row label="Nombre" value={user?.nombreCompleto ?? "—"} />
        <Row label="Rol" value={user?.rol ?? "—"} />
        <div className="mt-3 flex items-center gap-2 rounded-lg bg-emerald-50 p-3 text-sm text-emerald-700"><ShieldCheck size={16} /> Sesión protegida con JWT</div>
      </div>
    </div>
  );
}

function Row({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex justify-between border-b border-slate-100 py-2 text-sm last:border-0">
      <span className="text-slate-500">{label}</span>
      <span className="font-medium text-slate-700">{value}</span>
    </div>
  );
}
