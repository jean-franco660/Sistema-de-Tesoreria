import { useState } from "react";
import { NavLink, Outlet, useNavigate } from "react-router-dom";
import {
  LayoutDashboard, Wallet, GraduationCap, ClipboardList, Ticket, CalendarRange, BookUser,
  UserCog, Settings, ShieldCheck, Calendar, Sun, Bell, LogOut, Menu, X, Scale,
} from "lucide-react";
import { useAuth } from "../context/AuthContext";
import { useConfig, logoSrc } from "../lib/useConfig";
import { cn } from "../lib/utils";

const nav = [
  { to: "/", label: "Tesorería", icon: Wallet, end: true },
  { to: "/dashboard", label: "Dashboard", icon: LayoutDashboard, end: false },
  { to: "/programas", label: "Programas", icon: GraduationCap, end: false },
  { to: "/servicios", label: "Servicios", icon: ClipboardList, end: false },
  { to: "/tickets", label: "Tickets", icon: Ticket, end: false },
  { to: "/comparativa", label: "Comparativa", icon: Scale, end: false },
  { to: "/periodos", label: "Períodos", icon: CalendarRange, end: false },
  { to: "/matricula", label: "Registros", icon: BookUser, end: false },
  { to: "/usuarios", label: "Usuarios", icon: UserCog, end: false },
  { to: "/configuracion", label: "Configuración", icon: Settings, end: false },
  { to: "/auditoria", label: "Auditoría", icon: ShieldCheck, end: false },
];

export default function Layout() {
  const { user, logout } = useAuth();
  const config = useConfig();
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);
  const email = user ? `${user.username}@iestphzg.edu.pe` : "";
  const inicial = (user?.nombreCompleto || user?.username || "U").charAt(0).toUpperCase();
  const ahora = new Date();
  const fechaHora = `${ahora.toLocaleDateString("es-PE", { day: "numeric", month: "long", year: "numeric" })} · ${ahora.toLocaleTimeString("es-PE", { hour: "2-digit", minute: "2-digit" })}`;

  return (
    <div className="flex h-full">
      {/* Overlay (móvil) */}
      {open && <div className="fixed inset-0 z-30 bg-slate-900/40 backdrop-blur-sm lg:hidden" onClick={() => setOpen(false)} />}

      {/* ===== Sidebar / Drawer ===== */}
      <aside className={cn(
        "fixed inset-y-0 left-0 z-40 flex w-[244px] flex-col bg-gradient-to-b from-sidebar to-[#0d1120] text-slate-400 transition-transform duration-300 lg:static lg:translate-x-0",
        open ? "translate-x-0" : "-translate-x-full"
      )}>
        <div className="flex items-center gap-2.5 px-5 pb-4 pt-5">
          <img src={logoSrc(config)} alt="Logo institucional" className="h-11 w-auto max-w-[3.25rem] shrink-0 object-contain" />
          <div className="min-w-0 leading-tight">
            <p className="line-clamp-2 text-[11px] font-semibold tracking-tight text-white">{config?.nombreInstitucion || "Sistema de Tesorería"}</p>
            <p className="truncate text-[9px] tracking-[0.18em] text-slate-500">{config?.ciudad}</p>
          </div>
          <button onClick={() => setOpen(false)} className="ml-auto shrink-0 text-slate-500 hover:text-white lg:hidden"><X size={18} /></button>
        </div>

        <nav className="flex-1 space-y-0.5 overflow-y-auto px-3 py-2">
          {nav.map(({ to, label, icon: Icon, end }) => (
            <NavLink
              key={to}
              to={to}
              end={end}
              onClick={() => setOpen(false)}
              className={({ isActive }) =>
                cn(
                  "group flex items-center gap-3 rounded-xl px-3 py-2.5 text-[13px] transition-all",
                  isActive
                    ? "bg-primary text-white shadow-glow"
                    : "text-slate-400 hover:bg-white/[0.06] hover:text-white"
                )
              }
            >
              <Icon size={17} strokeWidth={2} />
              <span className="flex-1">{label}</span>
            </NavLink>
          ))}
        </nav>

        <div className="m-3 rounded-2xl bg-white/[0.04] p-2.5 ring-1 ring-white/[0.06]">
          <div className="flex items-center gap-2.5">
            <div className="grid h-9 w-9 place-items-center rounded-full bg-gradient-to-br from-primary to-violet-500 text-[13px] font-semibold text-white">
              {inicial}
            </div>
            <div className="min-w-0 flex-1">
              <p className="truncate text-[13px] font-medium text-white">{user?.nombreCompleto || user?.rol}</p>
              <p className="truncate text-[11px] text-slate-500">{email}</p>
            </div>
            <button onClick={() => { logout(); navigate("/login"); }} title="Cerrar sesión" className="text-slate-500 hover:text-white">
              <LogOut size={16} />
            </button>
          </div>
          <p className="mt-1.5 flex items-center gap-1.5 px-1 text-[11px] text-emerald-400">
            <span className="h-1.5 w-1.5 rounded-full bg-emerald-400 shadow-[0_0_6px_rgb(52_211_153)]" /> En línea
          </p>
        </div>
      </aside>

      {/* ===== Área principal ===== */}
      <div className="flex min-w-0 flex-1 flex-col">
        <header className="sticky top-0 z-20 flex items-center gap-2 border-b border-slate-200/60 bg-white/70 px-4 py-2.5 backdrop-blur-xl sm:gap-4 sm:px-6">
          <button onClick={() => setOpen(true)} className="grid h-9 w-9 shrink-0 place-items-center rounded-xl text-slate-600 hover:bg-slate-100 lg:hidden"><Menu size={20} /></button>

          <div className="hidden items-center gap-2 rounded-xl border border-slate-200/70 bg-white px-3 py-2 text-[13px] font-medium text-slate-600 shadow-card sm:flex">
            <Calendar size={15} className="text-primary" /> {fechaHora}
          </div>

          <div className="ml-auto flex items-center gap-1.5">
            <button className="grid h-9 w-9 place-items-center rounded-xl text-slate-500 transition-colors hover:bg-slate-100"><Sun size={17} /></button>
            <button className="relative grid h-9 w-9 place-items-center rounded-xl text-slate-500 transition-colors hover:bg-slate-100">
              <Bell size={17} />
              <span className="absolute right-1 top-1 grid h-3.5 w-3.5 place-items-center rounded-full bg-rose-500 text-[9px] font-bold text-white">2</span>
            </button>
            <div className="grid h-9 w-9 place-items-center rounded-full bg-gradient-to-br from-primary to-violet-500 text-[13px] font-semibold text-white">{inicial}</div>
          </div>
        </header>

        <main className="flex-1 overflow-auto px-4 py-4 sm:px-6 sm:py-5">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
