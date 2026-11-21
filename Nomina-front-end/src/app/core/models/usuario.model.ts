export interface Horario {
  horaEntrada: string;
  horaSalida: string;
  diasLaborales: number[];
}

export interface UsuarioRespuesta {
  id: string;
  identificacion: string;
  nombre: string;
  correo: string;
  rol: string;
  departamentoId: string;
  puesto: string;
  jefaturaDirectaId?: string;
  fechaIngreso: string;
  salarioBase: number;
  tipoJornada: string;
  horario: Horario;
  fotoUrl?: string;
  estado: string;
}

export interface UsuarioCrear {
  identificacion: string;
  nombre: string;
  correo: string;
  rol: string;
  departamentoId: string;
  puesto: string;
  jefaturaDirectaId?: string;
  fechaIngreso: string;
  salarioBase: number;
  tipoJornada: string;
  horario: Horario;
  fotoUrl?: string;
  password: string;
}

export interface UsuarioActualizar {
  identificacion: string;
  nombre: string;
  correo: string;
  rol: string;
  departamentoId: string;
  puesto: string;
  jefaturaDirectaId?: string;
  fechaIngreso: string;
  salarioBase: number;
  tipoJornada: string;
  horario: Horario;
  fotoUrl?: string;
}