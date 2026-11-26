
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  VacacionSolicitud,
  SaldoVacaciones,
  VacacionGuardar,
  ResolverVacacion
} from '../models/vacacion.model';

@Injectable({
  providedIn: 'root'
})
export class VacacionService {

  private apiUrl = 'https://localhost:7296/api/Vacaciones';

  constructor(private http: HttpClient) {}

  obtenerSaldo(usuarioId?: string): Observable<SaldoVacaciones> {
    let params = new HttpParams();

    if (usuarioId) {
      params = params.set('usuarioId', usuarioId);
    }

    return this.http.get<SaldoVacaciones>(
      `${this.apiUrl}/saldo`,
      { params }
    );
  }

  obtenerVacaciones(
    usuarioId?: string,
    estado?: string,
    desde?: string,
    hasta?: string
  ): Observable<VacacionSolicitud[]> {

    let params = new HttpParams();

    if (usuarioId) params = params.set('usuarioId', usuarioId);
    if (estado && estado !== 'Todos') params = params.set('estado', estado);
    if (desde) params = params.set('desde', desde);
    if (hasta) params = params.set('hasta', hasta);

    return this.http.get<VacacionSolicitud[]>(
      this.apiUrl,
      { params }
    );
  }

  obtenerPorId(id: string): Observable<VacacionSolicitud> {
    return this.http.get<VacacionSolicitud>(
      `${this.apiUrl}/${id}`
    );
  }

  crearSolicitud(
    solicitud: VacacionGuardar
  ): Observable<VacacionSolicitud> {
    return this.http.post<VacacionSolicitud>(
      this.apiUrl,
      solicitud
    );
  }

  actualizarSolicitud(
    id: string,
    solicitud: VacacionGuardar
  ): Observable<VacacionSolicitud> {
    return this.http.put<VacacionSolicitud>(
      `${this.apiUrl}/${id}`,
      solicitud
    );
  }

  cancelarSolicitud(id: string): Observable<any> {
    return this.http.patch(
      `${this.apiUrl}/${id}/cancelar`,
      {}
    );
  }

  aprobarSolicitud(
    id: string,
    resolucion?: ResolverVacacion
  ): Observable<any> {
    return this.http.patch(
      `${this.apiUrl}/${id}/aprobar`,
      resolucion ?? {}
    );
  }

  rechazarSolicitud(
    id: string,
    resolucion: ResolverVacacion
  ): Observable<any> {
    return this.http.patch(
      `${this.apiUrl}/${id}/rechazar`,
      resolucion
    );
  }
}
