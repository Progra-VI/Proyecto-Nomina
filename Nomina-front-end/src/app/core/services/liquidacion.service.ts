
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  LiquidacionRespuesta,
  LiquidacionCrear,
  LiquidacionGuardar
} from '../models/liquidacion.model';

@Injectable({
  providedIn: 'root'
})
export class LiquidacionService {

  private apiUrl = 'https://localhost:7296/api/liquidaciones';

  constructor(private http: HttpClient) {}

  // Obtener todas las liquidaciones o filtrar por estado y empleado
  obtenerTodas(
    estado?: string,
    usuarioId?: string
  ): Observable<LiquidacionRespuesta[]> {

    let params = new HttpParams();

    if (estado) {
      params = params.set('estado', estado);
    }

    if (usuarioId) {
      params = params.set('usuarioId', usuarioId);
    }

    return this.http.get<LiquidacionRespuesta[]>(
      this.apiUrl,
      { params }
    );
  }

  // Obtener una liquidación por su ID
  obtenerPorId(id: string): Observable<LiquidacionRespuesta> {
    return this.http.get<LiquidacionRespuesta>(
      `${this.apiUrl}/${id}`
    );
  }

  // Crear una nueva liquidación
  crear(dto: LiquidacionCrear): Observable<LiquidacionRespuesta> {
    return this.http.post<LiquidacionRespuesta>(
      this.apiUrl,
      dto
    );
  }

  // Recalcular una liquidación en estado Borrador
  recalcular(
    id: string,
    dto: LiquidacionGuardar
  ): Observable<LiquidacionRespuesta> {
    return this.http.put<LiquidacionRespuesta>(
      `${this.apiUrl}/${id}`,
      dto
    );
  }

  // Eliminar una liquidación en estado Borrador
  eliminar(id: string): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${id}`
    );
  }

  // Aprobar una liquidación
  aprobar(id: string): Observable<void> {
    return this.http.patch<void>(
      `${this.apiUrl}/${id}/aprobar`,
      {}
    );
  }

  // Registrar el pago de una liquidación aprobada
  pagar(id: string): Observable<void> {
    return this.http.patch<void>(
      `${this.apiUrl}/${id}/pagar`,
      {}
    );
  }

  // Descargar el comprobante PDF
  obtenerPdf(id: string): Observable<Blob> {
    return this.http.get(
      `${this.apiUrl}/${id}/pdf`,
      {
        responseType: 'blob'
      }
    );
  }
}
