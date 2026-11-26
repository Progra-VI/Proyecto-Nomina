import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UsuarioRespuesta, UsuarioCrear, UsuarioActualizar } from '../models/usuario.model';

@Injectable({
  providedIn: 'root'
})
export class UsuarioService {
  private apiUrl = 'https://localhost:7296/api/usuarios';

  constructor(private http: HttpClient) {}

  obtenerTodos(): Observable<UsuarioRespuesta[]> {
    return this.http.get<UsuarioRespuesta[]>(this.apiUrl);
  }

  obtenerPorId(id: string): Observable<UsuarioRespuesta> {
    return this.http.get<UsuarioRespuesta>(`${this.apiUrl}/${id}`);
  }

  crear(usuario: UsuarioCrear): Observable<UsuarioRespuesta> {
    return this.http.post<UsuarioRespuesta>(this.apiUrl, usuario);
  }

  actualizar(id: string, usuario: UsuarioActualizar): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, usuario);
  }

  inactivar(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/inactivar`, {});
  }

  obtenerJefaturasDisponibles(): Observable<UsuarioRespuesta[]> {
  return this.http.get<UsuarioRespuesta[]>(
    `${this.apiUrl}/jefaturas-disponibles`
  );
}
}