
export type EstadoLiquidacion = 'Borrador' | 'Aprobada' | 'Pagada';

// Rubros de ingresos y deducciones
export interface RubroLiquidacion {
  concepto: string;
  detalle: string;
  monto: number;
}

// Información que devuelve el backend
export interface LiquidacionRespuesta {
  id: string;

  usuarioId: string;
  nombreEmpleado: string;
  identificacion: string;
  departamento: string;
  puesto: string;

  fechaIngreso: string;
  fechaSalida: string;

  motivoId: string;
  motivo: string;

  antiguedadMeses: number;
  antiguedad: string;

  salarioBase: number;

  estado: EstadoLiquidacion;

  ingresos: RubroLiquidacion[];
  deducciones: RubroLiquidacion[];

  totalIngresos: number;
  totalDeducciones: number;
  totalNeto: number;

  tieneComprobante: boolean;
}

// Datos necesarios para crear una liquidación
export interface LiquidacionCrear {
  usuarioId: string;
  motivoLiquidacionId: string;
  fechaSalida: string;
}

// Datos necesarios para recalcular una liquidación
export interface LiquidacionGuardar {
  motivoLiquidacionId: string;
  fechaSalida: string;
}
