import { type ReactNode } from "react";

/** Tarjeta de estadística (encabezado de las páginas de lista). */
export function StatCard({ icon, color, label, value, hint }: {
  icon: ReactNode; color: string; label: string; value: string; hint?: string;
}) {
  return (
    <div className="rounded-2xl border border-slate-200/70 bg-white p-3.5 shadow-card transition-shadow hover:shadow-soft">
      <div className="flex items-center gap-3">
        <div className={`grid h-10 w-10 shrink-0 place-items-center rounded-xl ${color} shadow-sm`}>{icon}</div>
        <div className="min-w-0">
          <p className="truncate text-[11px] font-medium text-slate-400">{label}</p>
          <p className="truncate text-base font-semibold text-slate-800">{value}</p>
          {hint && <p className="truncate text-[10px] text-slate-400">{hint}</p>}
        </div>
      </div>
    </div>
  );
}
