export interface TipoHoraExtra {
  id: string;
  nombre: string; 
  recargo: number; 
}

export interface HoraExtraGuardarRequest {
  tipoHoraExtraId: string;
  fecha: string; 
  horaInicio: string; 
  horaFin: string; 
  justificacion: string;
}

export interface HoraExtraRespuesta {
  id: string | null;
  usuarioId: string;
  nombreEmpleado: string;
  fotoUrl?: string | null;
  tipoHoraExtraId: string;
  tipoHoraExtra: string;
  fecha: string;
  horaInicio: string;
  horaFin: string;
  horas: number;
  justificacion: string;
  valorHora: number;
  recargo: number;
  monto: number;
  estado: string; 
  aprobadorId?: string | null;
  aprobadorNombre?: string | null;
  observacion?: string | null;
  fechaResolucion?: string | null;
}

export interface ReporteLinea {
  id: string;
  nombre: string;
  horas: number;
  monto: number;
  solicitudes: number;
}

export interface ReporteHorasExtra {
  desde?: string | null;
  hasta?: string | null;
  totalHoras: number;
  totalMonto: number;
  porEmpleado: ReporteLinea[];
  porDepartamento: ReporteLinea[];
}

export interface ResolverSolicitudRequest {
  observacion?: string | null;
}