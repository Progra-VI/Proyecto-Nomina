import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { UsuarioLogin } from '../models/usuario.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private usuarioMock: UsuarioLogin = {
    id: '101',
    nombre: 'Natalia Arce',
    email: 'natalia.arce@nominapro.com',
    rol: 'RRHH', 
    departamento: 'TI',
    token: 'jwt_token_seguro_123456789'
  };

  login(email: string, pass: string): Observable<UsuarioLogin> {
    localStorage.setItem('usuario', JSON.stringify(this.usuarioMock));
    return of(this.usuarioMock);
  }

  obtenerUsuarioActual(): UsuarioLogin | null {
    const data = localStorage.getItem('usuario');
    return data ? JSON.parse(data) : null;
  }

  obtenerUsuario(): UsuarioLogin | null {
    return this.obtenerUsuarioActual();
  }

  cerrarSesion(): void {
    localStorage.removeItem('usuario');
  }
}