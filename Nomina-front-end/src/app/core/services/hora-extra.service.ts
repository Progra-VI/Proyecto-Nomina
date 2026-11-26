import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  HoraExtraGuardarRequest,
  HoraExtraRespuesta,
  ReporteHorasExtra,
  ResolverSolicitudRequest,
  TipoHoraExtra
} from '../models/hora-extra.model';

@Injectable({
  providedIn: 'root'
})
export class HoraExtraService {
  private readonly apiUrl = 'https://localhost:7296/api/horas-extras';
  private readonly catalogoUrl = 'https://localhost:7296/api/tipos-hora-extra';

  constructor(private http: HttpClient) {}

  obtenerTipos(): Observable<TipoHoraExtra[]> {
    return this.http.get<TipoHoraExtra[]>(this.catalogoUrl);
  }

  listar(filtros?: {
    usuarioId?: string;
    departamentoId?: string;
    estado?: string;
    tipoId?: string;
    desde?: string;
    hasta?: string;
  }): Observable<HoraExtraRespuesta[]> {
    let params = new HttpParams();
    if (filtros) {
      Object.entries(filtros).forEach(([key, value]) => {
        if (value && value !== 'Todos') {
          params = params.set(key, value);
        }
      });
    }
    return this.http.get<HoraExtraRespuesta[]>(this.apiUrl, { params });
  }

  obtenerReporte(filtros?: {
    usuarioId?: string;
    departamentoId?: string;
    desde?: string;
    hasta?: string;
  }): Observable<ReporteHorasExtra> {
    let params = new HttpParams();
    if (filtros) {
      Object.entries(filtros).forEach(([key, value]) => {
        if (value && value !== 'Todos') {
          params = params.set(key, value);
        }
      });
    }
    return this.http.get<ReporteHorasExtra>(`${this.apiUrl}/reporte`, { params });
  }

  crear(dto: HoraExtraGuardarRequest): Observable<HoraExtraRespuesta> {
    return this.http.post<HoraExtraRespuesta>(this.apiUrl, dto);
  }

  editar(id: string, dto: HoraExtraGuardarRequest): Observable<HoraExtraRespuesta> {
    return this.http.put<HoraExtraRespuesta>(`${this.apiUrl}/${id}`, dto);
  }

  cancelar(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/cancelar`, {});
  }

  aprobar(id: string, observacion?: string): Observable<void> {
    const body: ResolverSolicitudRequest = { observacion: observacion?.trim() || null };
    return this.http.patch<void>(`${this.apiUrl}/${id}/aprobar`, body);
  }

  rechazar(id: string, observacion: string): Observable<void> {
    const body: ResolverSolicitudRequest = { observacion: observacion.trim() };
    return this.http.patch<void>(`${this.apiUrl}/${id}/rechazar`, body);
  }
}