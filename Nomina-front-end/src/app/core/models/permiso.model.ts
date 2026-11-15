export interface PermisoSolicitud {
  id: string;
  empleadoId: string;
  nombreEmpleado: string;
  avatar: string;
  departamento: string;
  tipo: 'Médico' | 'Personal' | 'Estudio' | 'Luto' | 'Maternidad/Paternidad' | 'Otro';
  fechaInicio: string;
  fechaFin: string;
  horaInicio: string;
  horaFin: string;
  motivo: string;
  conGoceSalario: boolean;
  estado: 'Pendiente' | 'Aprobado' | 'Rechazado';
  aprobadoPor: string | null;
  observacionJefatura: string | null;
  aplicaDeduccionPlanilla: boolean;
}