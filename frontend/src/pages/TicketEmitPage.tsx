import { useEffect, useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Printer, Plus, Trash2, Search } from "lucide-react";
import api, { apiError } from "../lib/api";
import type { ConsultaDni, Programa, Servicio, Ticket, Turno } from "../types";
import { Card, CardContent, CardHeader, CardTitle } from "../components/ui/card";
import { Button } from "../components/ui/button";
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";
import { Select } from "../components/ui/select";
import ThermalTicket from "../components/ThermalTicket";
import { soles } from "../lib/utils";

interface Linea {
  servicioId: number;
  importe: number;
}

export default function TicketEmitPage() {
  const [dni, setDni] = useState("");
  const [consulta, setConsulta] = useState<ConsultaDni | null>(null);

  // Datos del alumno (editables si no existe)
  const [nombres, setNombres] = useState("");
  const [apellidos, setApellidos] = useState("");
  const [programaId, setProgramaId] = useState<number | "">("");
  const [turnoId, setTurnoId] = useState<number | "">("");
  const [alumnoId, setAlumnoId] = useState<number | null>(null);

  const [lineas, setLineas] = useState<Linea[]>([]);
  const [mensaje, setMensaje] = useState("");
  const [ticket, setTicket] = useState<Ticket | null>(null);
  const [cargando, setCargando] = useState(false);

  const { data: programas } = useQuery({ queryKey: ["programas"], queryFn: async () => (await api.get<Programa[]>("/programas?soloActivos=true")).data });
  const { data: turnos } = useQuery({ queryKey: ["turnos"], queryFn: async () => (await api.get<Turno[]>("/turnos")).data });
  const { data: servicios } = useQuery({ queryKey: ["servicios"], queryFn: async () => (await api.get<Servicio[]>("/servicios?soloActivos=true")).data });

  // Consulta automática al completar 8 dígitos (sin botón).
  useEffect(() => {
    if (dni.length !== 8) {
      setConsulta(null);
      return;
    }
    let cancel = false;
    (async () => {
      try {
        const { data } = await api.get<ConsultaDni>(`/alumnos/consulta-dni/${dni}`);
        if (cancel) return;
        setConsulta(data);
        setNombres(data.nombres ?? "");
        setApellidos(data.apellidos ?? "");
        setAlumnoId(data.existe ? data.alumnoId ?? null : null);
        setProgramaId(data.programaId ?? "");
        setTurnoId(data.turnoId ?? "");
      } catch (e) {
        if (!cancel) setMensaje(apiError(e));
      }
    })();
    return () => {
      cancel = true;
    };
  }, [dni]);

  const total = useMemo(() => lineas.reduce((s, l) => s + (Number(l.importe) || 0), 0), [lineas]);

  const registrarAlumno = async () => {
    setMensaje("");
    setCargando(true);
    try {
      const { data } = await api.post<{ id: number }>("/alumnos", {
        dni, nombres, apellidos, programaId: Number(programaId), turnoId: Number(turnoId),
      });
      setAlumnoId(data.id);
      setConsulta((c) => (c ? { ...c, existe: true } : c));
    } catch (e) {
      setMensaje(apiError(e));
    } finally {
      setCargando(false);
    }
  };

  const emitir = async () => {
    setMensaje("");
    setCargando(true);
    try {
      const { data } = await api.post<Ticket>("/tickets", {
        alumnoId,
        detalles: lineas.filter((l) => l.servicioId && l.importe > 0),
      });
      setTicket(data);
    } catch (e) {
      setMensaje(apiError(e));
    } finally {
      setCargando(false);
    }
  };

  const limpiar = () => {
    setDni(""); setConsulta(null); setNombres(""); setApellidos("");
    setProgramaId(""); setTurnoId(""); setAlumnoId(null); setLineas([]); setTicket(null); setMensaje("");
  };

  // ----- Ticket emitido: vista imprimible -----
  if (ticket) {
    return (
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="font-heading text-2xl text-navy">Ticket emitido</h2>
          <div className="flex gap-2">
            <Button onClick={() => window.print()}><Printer size={16} /> Imprimir</Button>
            <Button variant="outline" onClick={limpiar}>Nuevo ticket</Button>
          </div>
        </div>
        <Card className="mx-auto w-fit p-4">
          <ThermalTicket ticket={ticket} />
        </Card>
      </div>
    );
  }

  const puedeEmitir = alumnoId && lineas.some((l) => l.servicioId && l.importe > 0);

  return (
    <div className="space-y-5">
      <h2 className="font-heading text-2xl text-navy">Emisión de Ticket</h2>

      {mensaje && <p className="rounded-md bg-brand-red/10 px-3 py-2 text-sm text-brand-red">{mensaje}</p>}

      {/* Datos del alumno */}
      <Card>
        <CardHeader><CardTitle>Estudiante</CardTitle></CardHeader>
        <CardContent className="space-y-4">
          <div className="max-w-xs">
            <Label htmlFor="dni">DNI (8 dígitos)</Label>
            <div className="relative">
              <Input
                id="dni"
                inputMode="numeric"
                value={dni}
                maxLength={8}
                placeholder="Escriba el DNI…"
                onChange={(e) => setDni(e.target.value.replace(/\D/g, "").slice(0, 8))}
                autoFocus
              />
              <Search className="pointer-events-none absolute right-3 top-2.5 text-slate-400" size={18} />
            </div>
            {dni.length === 8 && consulta && (
              <p className="mt-1 text-xs text-slate-500">
                {consulta.existe
                  ? "✓ Alumno registrado"
                  : consulta.encontradoReniec
                  ? "✓ Encontrado en RENIEC — seleccione programa y turno"
                  : "No encontrado — complete los datos"}
              </p>
            )}
          </div>

          {dni.length === 8 && consulta && (
            <div className="grid gap-3 sm:grid-cols-2">
              <div>
                <Label>Apellidos</Label>
                <Input value={apellidos} onChange={(e) => setApellidos(e.target.value)} disabled={consulta.existe} />
              </div>
              <div>
                <Label>Nombres</Label>
                <Input value={nombres} onChange={(e) => setNombres(e.target.value)} disabled={consulta.existe} />
              </div>
              <div>
                <Label>Programa de estudios</Label>
                <Select value={programaId} disabled={consulta.existe} onChange={(e) => setProgramaId(Number(e.target.value))}>
                  <option value="">— Seleccione —</option>
                  {programas?.map((p) => <option key={p.id} value={p.id}>{p.nombre}</option>)}
                </Select>
              </div>
              <div>
                <Label>Turno</Label>
                <Select value={turnoId} disabled={consulta.existe} onChange={(e) => setTurnoId(Number(e.target.value))}>
                  <option value="">— Seleccione —</option>
                  {turnos?.map((t) => <option key={t.id} value={t.id}>{t.nombre}</option>)}
                </Select>
              </div>
              {!consulta.existe && (
                <div className="sm:col-span-2">
                  <Button onClick={registrarAlumno} disabled={cargando || !nombres || !apellidos || !programaId || !turnoId}>
                    Registrar alumno
                  </Button>
                </div>
              )}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Servicios */}
      <Card>
        <CardHeader><CardTitle>Servicios a cobrar</CardTitle></CardHeader>
        <CardContent className="space-y-3">
          {lineas.map((linea, idx) => (
            <div key={idx} className="flex items-end gap-2">
              <div className="flex-1">
                <Label>Servicio</Label>
                <Select
                  value={linea.servicioId || ""}
                  onChange={(e) => setLineas((ls) => ls.map((l, i) => (i === idx ? { ...l, servicioId: Number(e.target.value) } : l)))}
                >
                  <option value="">— Seleccione —</option>
                  {servicios?.map((s) => <option key={s.id} value={s.id}>{s.nombre}</option>)}
                </Select>
              </div>
              <div className="w-32">
                <Label>Importe</Label>
                <Input
                  type="number" min="0" step="0.10" value={linea.importe || ""}
                  onChange={(e) => setLineas((ls) => ls.map((l, i) => (i === idx ? { ...l, importe: Number(e.target.value) } : l)))}
                />
              </div>
              <Button variant="ghost" size="icon" onClick={() => setLineas((ls) => ls.filter((_, i) => i !== idx))}>
                <Trash2 size={18} className="text-brand-red" />
              </Button>
            </div>
          ))}

          <Button variant="outline" size="sm" onClick={() => setLineas((ls) => [...ls, { servicioId: 0, importe: 0 }])}>
            <Plus size={16} /> Agregar servicio
          </Button>

          <div className="flex items-center justify-between border-t border-slate-200 pt-3">
            <span className="font-heading text-lg text-navy">TOTAL</span>
            <span className="text-xl font-semibold text-navy">{soles(total)}</span>
          </div>

          <Button className="w-full" disabled={!puedeEmitir || cargando} onClick={emitir}>
            {cargando ? "Emitiendo…" : "Emitir Ticket"}
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}
