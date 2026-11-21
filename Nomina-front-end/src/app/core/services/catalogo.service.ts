import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { forkJoin, Observable } from 'rxjs';

export type TipoCatalogoApi =
  | 'departamentos'
  | 'feriados'
  | 'motivos-liquidacion'
  | 'tipos-hora-extra'
  | 'tipos-permiso'
  | 'parametros';

@Injectable({
  providedIn: 'root'
})
export class CatalogoService {
  private baseUrl = 'https://localhost:7296/api';

  constructor(private http: HttpClient) {}

  obtenerDepartamentos(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/departamentos`);
  }

  obtenerConfiguracionLey(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/configuracion-ley`);
  }

  guardarConfiguracionLey(dto: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/configuracion-ley`, dto);
  }

  obtenerPorTipo(tipo: TipoCatalogoApi, anio?: number): Observable<any> {
    if (tipo === 'feriados') {
      let params = new HttpParams();
      if (anio) {
        params = params.set('anio', anio.toString());
      }
      return this.http.get<any>(`${this.baseUrl}/feriados`, { params });
    }
    return this.http.get<any>(`${this.baseUrl}/${tipo}`);
  }

  obtenerTodosLosCatalogos(anio?: number): Observable<{
    departamentos: any;
    feriados: any;
    motivosLiquidacion: any;
    tiposHoraExtra: any;
    tiposPermiso: any;
    parametros: any;
  }> {
    return forkJoin({
      departamentos: this.obtenerPorTipo('departamentos'),
      feriados: this.obtenerPorTipo('feriados', anio),
      motivosLiquidacion: this.obtenerPorTipo('motivos-liquidacion'),
      tiposHoraExtra: this.obtenerPorTipo('tipos-hora-extra'),
      tiposPermiso: this.obtenerPorTipo('tipos-permiso'),
      parametros: this.obtenerPorTipo('parametros')
    });
  }

  crear(tipo: Exclude<TipoCatalogoApi, 'parametros'>, dto: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/${tipo}`, dto);
  }

  actualizar(tipo: 'departamentos' | 'feriados' | 'motivos-liquidacion' | 'tipos-hora-extra' | 'tipos-permiso', id: string, dto: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/${tipo}/${id}`, dto);
  }

  guardarParametro(clave: string, dto: { valor: string; descripcion?: string | null }): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/parametros/${clave}`, dto);
  }
}