import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { PermisoSolicitud } from '../models/permiso.model';

@Injectable({
  providedIn: 'root'
})
export class PermisoService {
  private permisos: PermisoSolicitud[] = [
    {
      id: 'p1',
      empleadoId: '101',
      nombreEmpleado: 'Natalia Arce',
      avatar: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&h=150&fit=crop&crop=faces',
      departamento: 'Operaciones',
      tipo: 'Médico',
      fechaInicio: '2026-11-20',
      fechaFin: '2026-11-20',
      horaInicio: '08:00',
      horaFin: '12:00',
      motivo: 'Cita de control en la CCSS',
      conGoceSalario: true,
      estado: 'Aprobado',
      aprobadoPor: 'Javier (Jefatura)',
      observacionJefatura: 'Justificante médico presentado previamente.',
      aplicaDeduccionPlanilla: false
    }
  ];

  obtenerPermisos(filtroEstado?: string, filtroTipo?: string, filtroEmp?: string): Observable<PermisoSolicitud[]> {
    let resultado = [...this.permisos];
    if (filtroEstado && filtroEstado !== 'Todos') {
      resultado = resultado.filter(p => p.estado === filtroEstado);
    }
    if (filtroTipo && filtroTipo !== 'Todos') {
      resultado = resultado.filter(p => p.tipo === filtroTipo);
    }
    if (filtroEmp && filtroEmp.trim() !== '') {
      resultado = resultado.filter(p => p.nombreEmpleado.toLowerCase().includes(filtroEmp.toLowerCase()));
    }
    return of(resultado);
  }

  crearSolicitud(nuevo: {
    empleadoId: string;
    nombreEmpleado: string;
    avatar: string;
    departamento: string;
    tipo: PermisoSolicitud['tipo'];
    fechaInicio: string;
    fechaFin: string;
    horaInicio: string;
    horaFin: string;
    motivo: string;
    conGoceSalario: boolean;
  }): { exito: boolean; mensaje: string } {
    
    
    const traslape = this.permisos.some(p => 
      p.empleadoId === nuevo.empleadoId &&
      p.estado === 'Aprobado' &&
      !(nuevo.fechaFin < p.fechaInicio || nuevo.fechaInicio > p.fechaFin)
    );

    if (traslape) {
      return { exito: false, mensaje: 'Error: Ya existe un permiso aprobado en ese rango de fechas.' };
    }

    const solicitudCompleta: PermisoSolicitud = {
      ...nuevo,
      id: 'p_' + Date.now(),
      estado: 'Pendiente',
      aprobadoPor: null,
      observacionJefatura: null,
      aplicaDeduccionPlanilla: !nuevo.conGoceSalario
    };

    this.permisos.unshift(solicitudCompleta);
    return { exito: true, mensaje: 'Solicitud de permiso enviada con éxito.' };
  }

  resolverSolicitud(id: string, aprobado: boolean, observacion: string, usuarioJefatura: string): void {
    const p = this.permisos.find(item => item.id === id);
    if (p) {
      p.estado = aprobado ? 'Aprobado' : 'Rechazado';
      p.aprobadoPor = usuarioJefatura;
      p.observacionJefatura = observacion;
    }
  }
}