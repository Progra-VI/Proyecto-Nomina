import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { PermisoSolicitud } from '../models/permiso.model';

@Injectable({
  providedIn: 'root'
})
export class PermisoService {

  obtenerPermisos(estado?: string, tipo?: string, empleado?: string): Observable<PermisoSolicitud[]> {
    return of([]);
  }

  crearPermiso(permiso: any): { exito: boolean; mensaje: string } {
    return { exito: true, mensaje: 'Permiso creado exitosamente' };
  }

  crearSolicitud(permiso: any): { exito: boolean; mensaje: string } {
    return this.crearPermiso(permiso);
  }

  actualizarEstadoPermiso(id: string, estado: string, observacion: string, usuario: string): Observable<boolean> {
    return of(true);
  }


  resolverSolicitud(id: any, aprobado: boolean, observacion: any, usuario: any): any {
    return { exito: true, mensaje: 'Solicitud actualizada correctamente' };
  }
}