
export interface VacacionSolicitud {
  id: string;
  usuarioId: string;
  nombreEmpleado: string;
  fotoUrl: string | null;
  fechaInicio: string;
  fechaFin: string;
  dias: number;
  motivo: string | null;
  estado: 'Pendiente' | 'Aprobada' | 'Rechazada' | 'Cancelada';
  aprobadorId: string | null;
  aprobadorNombre: string | null;
  observacion: string | null;
  fechaResolucion: string | null;
}

export interface SaldoVacaciones {
  usuarioId: string;
  nombreEmpleado: string;
  fechaIngreso: string;
  acumulado: number;
  disfrutado: number;
  pendiente: number;
  saldo: number;
  disponible: number;
  movimientos: VacacionSolicitud[];
}

export interface VacacionGuardar {
  fechaInicio: string;
  fechaFin: string;
  motivo: string | null;
}

export interface ResolverVacacion {
  observacion?: string;
}
