import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { AsistenciaRegistro, ResumenMensualAsistencia } from '../models/asistencia.model';

@Injectable({
  providedIn: 'root'
})
export class AsistenciaService {
  private registros: AsistenciaRegistro[] = [
    {
      id: '1',
      empleadoId: '101',
      nombreEmpleado: 'Natalia',
      departamento: 'Operaciones',
      fecha: '2026-11-15',
      horaEntrada: '08:15',
      horaSalida: '17:00',
      horarioAsignadoEntrada: '08:00',
      horarioAsignadoSalida: '17:00',
      estado: 'Tardía',
      minutosTardia: 15,
      minutosSalidaAnticipada: 0,
      horasExtra: 0,
      justificacion: null,
      ajustadoPor: null
    },
    {
      id: '2',
      empleadoId: '102',
      nombreEmpleado: 'Steven',
      departamento: 'Soporte técnico',
      fecha: '2026-11-15',
      horaEntrada: '08:00',
      horaSalida: '16:30',
      horarioAsignadoEntrada: '08:00',
      horarioAsignadoSalida: '17:00',
      estado: 'Salida Anticipada',
      minutosTardia: 0,
      minutosSalidaAnticipada: 30,
      horasExtra: 0,
      justificacion: 'Cita médica CCSS',
      ajustadoPor: 'Admin RRHH'
    },
    {
      id: '3',
      empleadoId: '103',
      nombreEmpleado: 'Sebastian',
      departamento: 'TI',
      fecha: '2026-11-14',
      horaEntrada: null,
      horaSalida: null,
      horarioAsignadoEntrada: '08:00',
      horarioAsignadoSalida: '17:00',
      estado: 'Ausencia Injustificada',
      minutosTardia: 0,
      minutosSalidaAnticipada: 0,
      horasExtra: 0,
      justificacion: null,
      ajustadoPor: null
    },
    {
      id: '4',
      empleadoId: '101',
      nombreEmpleado: 'Bryan',
      departamento: 'TI',
      fecha: '2026-11-14',
      horaEntrada: '07:55',
      horaSalida: '19:00',
      horarioAsignadoEntrada: '08:00',
      horarioAsignadoSalida: '17:00',
      estado: 'Normal',
      minutosTardia: 0,
      minutosSalidaAnticipada: 0,
      horasExtra: 2,
      justificacion: 'Cierre operativo mensual',
      ajustadoPor: null
    }
  ];

  obtenerRegistros(filtroDep?: string, filtroEmp?: string, fechaInicio?: string, fechaFin?: string): Observable<AsistenciaRegistro[]> {
    let resultado = [...this.registros];

    if (filtroDep && filtroDep !== 'Todos') {
      resultado = resultado.filter(r => r.departamento === filtroDep);
    }
    if (filtroEmp && filtroEmp.trim() !== '') {
      resultado = resultado.filter(r => r.nombreEmpleado.toLowerCase().includes(filtroEmp.toLowerCase()));
    }
    if (fechaInicio) {
      resultado = resultado.filter(r => r.fecha >= fechaInicio);
    }
    if (fechaFin) {
      resultado = resultado.filter(r => r.fecha <= fechaFin);
    }

    return of(resultado.sort((a, b) => b.fecha.localeCompare(a.fecha)));
  }

  registrarMarca(empleadoId: string, nombreEmpleado: string, departamento: string, tipo: 'entrada' | 'salida'): Observable<AsistenciaRegistro> {
    const hoy = new Date().toISOString().split('T')[0];
    const ahora = new Date();
    const horaActual = ahora.toTimeString().slice(0, 5); // HH:mm

    let reg = this.registros.find(r => r.empleadoId === empleadoId && r.fecha === hoy);

    if (!reg) {
      const evalEntrada = this.evaluarHorario(horaActual, null, '08:00', '17:00');
      reg = {
        id: String(Date.now()),
        empleadoId,
        nombreEmpleado,
        departamento,
        fecha: hoy,
        horaEntrada: tipo === 'entrada' ? horaActual : null,
        horaSalida: tipo === 'salida' ? horaActual : null,
        horarioAsignadoEntrada: '08:00',
        horarioAsignadoSalida: '17:00',
        estado: evalEntrada.estado,
        minutosTardia: evalEntrada.minutosTardia,
        minutosSalidaAnticipada: 0,
        horasExtra: 0,
        justificacion: null,
        ajustadoPor: null
      };
      this.registros.unshift(reg);
    } else {
      if (tipo === 'entrada') reg.horaEntrada = horaActual;
      if (tipo === 'salida') reg.horaSalida = horaActual;

      const evalCompleta = this.evaluarHorario(reg.horaEntrada, reg.horaSalida, reg.horarioAsignadoEntrada, reg.horarioAsignadoSalida);
      reg.estado = evalCompleta.estado;
      reg.minutosTardia = evalCompleta.minutosTardia;
      reg.minutosSalidaAnticipada = evalCompleta.minutosSalidaAnticipada;
      reg.horasExtra = evalCompleta.horasExtra;
    }

    return of(reg);
  }

  ajustarMarcaRRHH(
    id: string,
    nuevaEntrada: string | null,
    nuevaSalida: string | null,
    estadoManual: AsistenciaRegistro['estado'],
    justificacion: string,
    usuarioRRHH: string
  ): Observable<boolean> {
    const reg = this.registros.find(r => r.id === id);
    if (reg) {
      reg.horaEntrada = nuevaEntrada || null;
      reg.horaSalida = nuevaSalida || null;
      reg.estado = estadoManual;
      reg.justificacion = justificacion;
      reg.ajustadoPor = `${usuarioRRHH} (${new Date().toLocaleDateString()})`;

      if (estadoManual === 'Normal') {
        reg.minutosTardia = 0;
        reg.minutosSalidaAnticipada = 0;
      }
    }
    return of(true);
  }

  obtenerResumenMensual(): Observable<ResumenMensualAsistencia[]> {
    const mapa = new Map<string, ResumenMensualAsistencia>();

    this.registros.forEach(r => {
      if (!mapa.has(r.empleadoId)) {
        mapa.set(r.empleadoId, {
          empleadoId: r.empleadoId,
          nombreEmpleado: r.nombreEmpleado,
          departamento: r.departamento,
          diasLaborados: 0,
          tardias: 0,
          ausenciasJustificadas: 0,
          ausenciasInjustificadas: 0,
          horasTrabajadas: 0,
          horasExtraValidadas: 0,
          aplicaDeduccionPlanilla: false
        });
      }

      const resumen = mapa.get(r.empleadoId)!;

      if (r.estado === 'Ausencia Justificada') {
        resumen.ausenciasJustificadas++;
      } else if (r.estado === 'Ausencia Injustificada') {
        resumen.ausenciasInjustificadas++;
        resumen.aplicaDeduccionPlanilla = true;
      } else {
        resumen.diasLaborados++;
        if (r.estado === 'Tardía') resumen.tardias++;
        resumen.horasTrabajadas += this.calcularHorasDia(r.horaEntrada, r.horaSalida);
        resumen.horasExtraValidadas += r.horasExtra;
      }
    });

    return of(Array.from(mapa.values()));
  }

  private evaluarHorario(entrada: string | null, salida: string | null, hEnt: string, hSal: string) {
    let minutosTardia = 0;
    let minutosSalidaAnticipada = 0;
    let horasExtra = 0;
    let estado: AsistenciaRegistro['estado'] = 'Normal';

    if (entrada) {
      const diffEnt = this.aMinutos(entrada) - this.aMinutos(hEnt);
      if (diffEnt > 0) {
        minutosTardia = diffEnt;
        estado = 'Tardía';
      }
    }

    if (salida) {
      const diffSal = this.aMinutos(hSal) - this.aMinutos(salida);
      if (diffSal > 0) {
        minutosSalidaAnticipada = diffSal;
        if (estado === 'Normal') estado = 'Salida Anticipada';
      } else if (diffSal <= -60) {
        horasExtra = Math.floor(Math.abs(diffSal) / 60);
      }
    }

    return { estado, minutosTardia, minutosSalidaAnticipada, horasExtra };
  }

  private aMinutos(hora: string): number {
    const [h, m] = hora.split(':').map(Number);
    return h * 60 + m;
  }

  private calcularHorasDia(entrada: string | null, salida: string | null): number {
    if (!entrada || !salida) return 8;
    const diff = (this.aMinutos(salida) - this.aMinutos(entrada)) / 60;
    return diff > 0 ? Math.round(diff * 10) / 10 : 0;
  }
}