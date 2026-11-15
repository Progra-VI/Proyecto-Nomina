export interface AsistenciaRegistro {
  id: string;
  empleadoId: string;
  nombreEmpleado: string;
  departamento: string;
  fecha: string; // YYYY-MM-DD
  horaEntrada: string | null;
  horaSalida: string | null;
  horarioAsignadoEntrada: string; 
  horarioAsignadoSalida: string;  
  estado: 'Normal' | 'Tardía' | 'Salida Anticipada' | 'Ausencia Justificada' | 'Ausencia Injustificada';
  minutosTardia: number;
  minutosSalidaAnticipada: number;
  horasExtra: number;
  justificacion: string | null;
  ajustadoPor: string | null;
}

export interface ResumenMensualAsistencia {
  empleadoId: string;
  nombreEmpleado: string;
  departamento: string;
  diasLaborados: number;
  tardias: number;
  ausenciasJustificadas: number;
  ausenciasInjustificadas: number;
  horasTrabajadas: number;
  horasExtraValidadas: number;
  aplicaDeduccionPlanilla: boolean;
}