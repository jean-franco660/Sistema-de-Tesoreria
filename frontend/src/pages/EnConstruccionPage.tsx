import { Hammer } from "lucide-react";

export default function EnConstruccionPage({ titulo }: { titulo: string }) {
  return (
    <div className="space-y-6">
      <h1 className="text-xl font-semibold tracking-tight text-slate-800">{titulo}</h1>
      <div className="grid place-items-center rounded-2xl border border-dashed border-slate-300 bg-white p-16 text-center shadow-card">
        <div className="grid h-14 w-14 place-items-center rounded-2xl bg-primary-soft">
          <Hammer className="text-cyan-800" size={26} />
        </div>
        <p className="mt-4 text-lg font-semibold text-slate-700">Sección en construcción</p>
        <p className="mt-1 text-sm text-slate-500">Este módulo estará disponible próximamente.</p>
      </div>
    </div>
  );
}
