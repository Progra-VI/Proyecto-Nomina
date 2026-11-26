export interface PlanillaCrearRequest {
  anio: number;
  mes: number;
  quincena?: number | null;
}

export interface PlanillaRespuesta {
  id: string | null;
  anio: number;
  mes: number;
  quincena?: number | null;
  desde: string;
  hasta: string;
  estado: string; // Borrador | Calculada | Aprobada | Pagada
  empleados: number;
  totalIngresos: number;
  totalDeducciones: number;
  totalNeto: number;
  fechaCalculo?: string | null;
}

export interface LineaCarga {
  nombre: string;
  porcentaje: number;
  monto: number;
}

export interface Boleta {
  id: string | null;
  planillaId: string;
  estadoPlanilla: string;
  anio: number;
  mes: number;
  quincena?: number | null;
  desde: string;
  hasta: string;
  usuarioId: string;
  nombreEmpleado: string;
  identificacion: string;
  departamento: string;
  puesto: string;
  salarioBase: number;
  diasLaborados: number;
  diasVacaciones: number;
  ausenciasInjustificadas: number;
  ausenciasJustificadas: number;
  horasExtra: number;
  ingresoSalario: number;
  ingresoVacaciones: number;
  ingresoHorasExtra: number;
  totalIngresos: number;
  deduccionAusencias: number;
  deduccionPermisos: number;
  cargasSociales: LineaCarga[];
  totalCargas: number;
  impuestoRenta: number;
  totalDeducciones: number;
  salarioNeto: number;
}

export interface PlanillaDetalle {
  planilla: PlanillaRespuesta;
  boletas: Boleta[];
}