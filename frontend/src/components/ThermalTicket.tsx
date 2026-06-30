import type { Ticket } from "../types";
import { soles } from "../lib/utils";
import { useConfig, logoSrc } from "../lib/useConfig";

/**
 * Representación del ticket optimizada para papel térmico de 80 mm.
 * El id "ticket-print" es usado por las reglas @media print de index.css.
 */
export default function ThermalTicket({ ticket }: { ticket: Ticket }) {
  const config = useConfig();
  const fecha = new Date(ticket.fechaEmision);
  return (
    <div
      id="ticket-print"
      className="mx-auto bg-white p-3 font-mono text-[11px] leading-tight text-black"
      style={{ width: "80mm" }}
    >
      <div className="text-center">
        {/* Logo institucional */}
        <img src={logoSrc(config)} alt="Logo institucional" className="mx-auto mb-1 h-16 w-auto max-h-16 object-contain" />
        <p className="font-bold uppercase leading-tight">{config?.nombreInstitucion || 'INSTITUTO DE EDUCACIÓN SUPERIOR TECNOLÓGICO PRODUCTIVO "HORACIO ZEBALLOS GÁMEZ"'}</p>
        <p>{config?.ciudad || "JULIACA"}</p>
        <div className="my-1 border-t border-dashed border-black" />
        <p className="font-bold uppercase">{config?.tituloComprobante || "Ingreso por Recursos Propios y Actividades Productivas"}</p>
        <p>{config?.baseLegal || "D.S. N° 028-2007-ED"}</p>
      </div>

      <div className="my-1 border-t border-dashed border-black" />

      <div className="space-y-0.5">
        <Row k="Ticket N°" v={ticket.numeroTicket} />
        <Row k="Contador" v={ticket.contador} />
        <Row k="Fecha" v={fecha.toLocaleDateString("es-PE")} />
        <Row k="Hora" v={fecha.toLocaleTimeString("es-PE")} />
        <Row k="DNI" v={ticket.dni} />
        <Row k="Nombre" v={ticket.alumnoNombre} />
        <Row k="Programa" v={ticket.programa} />
        <Row k="Turno" v={ticket.turno} />
      </div>

      <div className="my-1 border-t border-dashed border-black" />

      <table className="w-full">
        <thead>
          <tr className="border-b border-black">
            <th className="text-left">Servicio</th>
            <th className="text-right">Importe</th>
          </tr>
        </thead>
        <tbody>
          {ticket.detalles.map((d, i) => (
            <tr key={i}>
              <td className="pr-1 align-top">{d.servicio}</td>
              <td className="text-right align-top">{soles(d.importe)}</td>
            </tr>
          ))}
        </tbody>
      </table>

      <div className="my-1 border-t border-dashed border-black" />
      <div className="flex justify-between font-bold">
        <span>TOTAL</span>
        <span>{soles(ticket.total)}</span>
      </div>
      <p className="mt-1">SON: {ticket.totalEnLetras}</p>

      <div className="my-2 border-t border-dashed border-black" />
      <p>Usuario: TESORERÍA ({ticket.usuario})</p>
      <div className="mt-6 text-center">
        <div className="mx-auto w-40 border-t border-black" />
        <p>Firma del Tesorero</p>
      </div>
    </div>
  );
}

function Row({ k, v }: { k: string; v: string }) {
  return (
    <div className="flex justify-between gap-2">
      <span className="text-slate-700">{k}:</span>
      <span className="text-right font-medium">{v}</span>
    </div>
  );
}
