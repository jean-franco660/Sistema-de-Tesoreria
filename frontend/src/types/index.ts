export interface AuthResponse {
  token: string;
  username: string;
  nombreCompleto: string;
  rol: string;
  expiraEnMinutos: number;
}

export interface Programa {
  id: number;
  nombre: string;
  estado: string;
  fechaCreacion: string;
}

export interface Turno {
  id: number;
  nombre: string;
}

export interface Servicio {
  id: number;
  nombre: string;
  precio: number;
  estado: string;
}

export interface ConsultaDni {
  dni: string;
  existe: boolean;
  encontradoReniec: boolean;
  alumnoId?: number;
  nombres: string;
  apellidos: string;
  nombreCompleto: string;
  programaId?: number;
  programa?: string;
  turnoId?: number;
  turno?: string;
  sexo?: string | null;
  fechaNacimiento?: string | null;
  edad?: number | null;
  celular?: string | null;
  seccion?: string | null;
}

export interface Alumno {
  id: number;
  dni: string;
  nombres: string;
  apellidos: string;
  nombreCompleto: string;
  programaId: number;
  programa: string;
  turnoId: number;
  turno: string;
  seccion: string;
  sexo?: string | null;
  fechaNacimiento?: string | null;
  edad?: number | null;
  celular?: string | null;
  estado: string;
  fechaRegistro: string;
}

export interface TicketDetalle {
  servicioId: number;
  servicio: string;
  importe: number;
}

export interface Ticket {
  id: number;
  numeroTicket: string;
  contador: string;
  fechaEmision: string;
  dni: string;
  alumnoNombre: string;
  programa: string;
  turno: string;
  total: number;
  totalEnLetras: string;
  usuario: string;
  estado: string;
  detalles: TicketDetalle[];
}

export interface TicketListItem {
  id: number;
  numeroTicket: string;
  contador: string;
  fechaEmision: string;
  dni: string;
  alumnoNombre: string;
  total: number;
  usuario: string;
  estado: string;
}

export interface ConteoItem {
  nombre: string;
  cantidad: number;
  monto: number;
}

export interface Dashboard {
  totalRecaudadoHoy: number;
  totalRecaudadoMes: number;
  ticketsHoy: number;
  ticketsMes: number;
  totalRecaudadoAyer: number;
  totalRecaudadoMesPasado: number;
  ticketsAyer: number;
  ticketsMesPasado: number;
  programasActivos: number;
  serviciosMasCobrados: ConteoItem[];
  programasConMasPagos: ConteoItem[];
  ultimosTickets: TicketListItem[];
  ingresosDiarios: { fecha: string; monto: number }[];
}

export interface Configuracion {
  id: number;
  nombreInstitucion: string;
  ciudad: string;
  codigoModular: string;
  direccion: string;
  baseLegal: string;
  tituloComprobante: string;
  tipoComprobante: string;
  logoBase64?: string | null;
}

export interface UsuarioSistema {
  id: number;
  username: string;
  nombreCompleto: string;
  rol: string;
  estado: string;
  fechaCreacion: string;
}

export interface Periodo {
  id: number;
  nombre: string;
  fechaInicio: string;
  fechaFin: string;
  estado: string;
  usuarioApertura?: string;
  fechaApertura: string;
  usuarioCierre?: string;
  fechaCierre?: string;
  observaciones?: string;
  tickets: number;
  ingresos: number;
  diasRestantes?: number | null;
}

export interface PeriodoResumen {
  id: number;
  nombre: string;
  fechaInicio: string;
  fechaFin: string;
  estado: string;
  tickets: number;
  estudiantes: number;
  ingresos: number;
  servicios: number;
  usuarioApertura?: string;
  usuarioCierre?: string;
  fechaCierre?: string;
}

export interface MatriculaItem {
  n: number;
  codigoMatricula: string;
  apellidosNombres: string;
  sexo?: string | null;
  fechaNacimiento?: string | null;
  edad?: number | null;
  celular?: string | null;
}

export interface Registro {
  id: number;
  periodoId: number;
  periodo: string;
  programaId: number;
  programa: string;
  turnoId: number;
  turno: string;
  seccion: string;
  profesor?: string | null;
  moduloFormativo?: string | null;
  cantidad: number;
  fechaCreacion: string;
}

export interface Matricula {
  nombreInstitucion: string;
  codigoModular: string;
  direccion: string;
  ciudad: string;
  periodo: string;
  periodoInicio?: string;
  periodoFin?: string;
  programa: string;
  turno: string;
  seccion: string;
  dreGre: string;
  ugel: string;
  resolucionCreacion: string;
  resolucionAutorizacion: string;
  periodoLectivo: string;
  modalidadServicio: string;
  nivelFormativo: string;
  tipoPlan: string;
  profesor?: string | null;
  moduloFormativo?: string | null;
  cantidad: number;
  estudiantes: MatriculaItem[];
}

export interface MatriculaOpcion {
  periodoId?: number;
  periodo: string;
  programaId: number;
  programa: string;
  turnoId: number;
  turno: string;
  seccion: string;
  cantidad: number;
}

export interface PeriodoDetalle {
  periodo: PeriodoResumen;
  items: ReporteIngresoItem[];
  total: number;
  resumenPorServicio: ReporteResumen[];
  resumenPorPrograma: ReporteResumen[];
  resumenPorUsuario: ReporteResumen[];
}

export interface DashboardPeriodo {
  hayPeriodoActivo: boolean;
  nombre?: string;
  fechaInicio?: string;
  fechaFin?: string;
  ingresosAcumulados: number;
  tickets: number;
  estudiantes: number;
  servicios: number;
  diasRestantes: number;
}

export interface ReporteIngresoItem {
  fecha: string;
  numeroTicket: string;
  contador: string;
  dni: string;
  alumno: string;
  programa: string;
  servicio: string;
  importe: number;
  usuario: string;
}

export interface ReporteResumen {
  nombre: string;
  cantidad: number;
  monto: number;
}

export interface ReporteIngresos {
  desde?: string;
  hasta?: string;
  cantidadTickets: number;
  cantidadServicios: number;
  total: number;
  items: ReporteIngresoItem[];
  resumenPorServicio: ReporteResumen[];
  resumenPorPrograma: ReporteResumen[];
}

export interface AuditLog {
  id: number;
  usuario: string;
  fecha: string;
  ip: string;
  accion: string;
  detalle?: string;
}

// ── Egresos (comprobantes) y Comparativa ───────────────────────────────────
export interface ComprobanteProducto {
  descripcion?: string | null;
  cantidad?: number | null;
  precioUnitario?: number | null;
  importe?: number | null;
}

export interface ComprobanteListItem {
  id: number;
  proveedor?: string | null;
  ruc?: string | null;
  tipoDocumento?: string | null;
  numeroComprobante?: string | null;
  fechaEmision?: string | null;
  fechaRegistro: string;
  total: number;
  moneda: string;
  categoria?: string | null;
  concepto?: string | null;
  confianza: number;
  esDuplicadoProbable: boolean;
  usuario: string;
  estado: string;
}

export interface Comprobante extends ComprobanteListItem {
  horaEmision?: string | null;
  subtotal?: number | null;
  igv?: number | null;
  metodoPago?: string | null;
  observaciones?: string | null;
  imagenRuta?: string | null;
  imagenUrl?: string | null;
  imagenBase64?: string | null;
  productos: ComprobanteProducto[];
}

export interface ComparativaDia {
  fecha: string;
  ingresos: number;
  egresos: number;
}

export interface ComparativaGrupo {
  nombre: string;
  cantidad: number;
  monto: number;
}

export interface Movimiento {
  tipo: "Ingreso" | "Egreso";
  fecha: string;
  descripcion: string;
  detalle: string;
  monto: number;
  usuario: string;
}

export interface Comparativa {
  desde?: string;
  hasta?: string;
  totalIngresos: number;
  totalEgresos: number;
  balance: number;
  cantidadIngresos: number;
  cantidadEgresos: number;
  serie: ComparativaDia[];
  egresosPorCategoria: ComparativaGrupo[];
  ingresosPorConcepto: ComparativaGrupo[];
  movimientos: Movimiento[];
}
