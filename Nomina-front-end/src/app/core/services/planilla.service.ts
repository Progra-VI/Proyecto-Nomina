import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Boleta, PlanillaCrearRequest, PlanillaDetalle, PlanillaRespuesta } from '../models/planilla.model';

@Injectable({
  providedIn: 'root'
})
export class PlanillaService {
  private readonly apiUrl = 'https://localhost:7296/api/planillas';

  constructor(private http: HttpClient) {}

  listar(anio?: number): Observable<PlanillaRespuesta[]> {
    let params = new HttpParams();
    if (anio) params = params.set('anio', anio);
    return this.http.get<PlanillaRespuesta[]>(this.apiUrl, { params });
  }

  obtenerDetalle(id: string): Observable<PlanillaDetalle> {
    return this.http.get<PlanillaDetalle>(`${this.apiUrl}/${id}`);
  }

  crear(dto: PlanillaCrearRequest): Observable<PlanillaRespuesta> {
    return this.http.post<PlanillaRespuesta>(this.apiUrl, dto);
  }

  calcular(id: string): Observable<PlanillaRespuesta> {
    return this.http.post<PlanillaRespuesta>(`${this.apiUrl}/${id}/calcular`, {});
  }

  aprobar(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/aprobar`, {});
  }

  pagar(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/pagar`, {});
  }

  obtenerBoletasMias(): Observable<Boleta[]> {
    return this.http.get<Boleta[]>(`${this.apiUrl}/mias`);
  }
}