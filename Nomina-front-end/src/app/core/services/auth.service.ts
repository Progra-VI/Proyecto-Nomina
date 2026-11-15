import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class AuthService {
  guardarSesion(token: string, usuario: any) {
    localStorage.setItem('token', token);
    localStorage.setItem('usuario', JSON.stringify(usuario));
  }

  cerrarSesion() {
    localStorage.removeItem('token');
    localStorage.removeItem('usuario');
  }

  estaAutenticado(): boolean {
    return !!localStorage.getItem('token');
  }

  obtenerUsuario() {
    return JSON.parse(localStorage.getItem('usuario') || 'null');
  }
}
