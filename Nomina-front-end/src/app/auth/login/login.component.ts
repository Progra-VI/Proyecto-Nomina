import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  email = '';
  password = '';
  error = '';
  cargando = false;

  constructor(private authService: AuthService, private router: Router) {}

  onLogin(): void {
    this.authService.login(this.email, this.password).subscribe({
      next: (user) => {
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.error = 'Credenciales inválidas. Verifique sus datos.';
      }
    });
  }

  
  iniciarComo(rol: 'Empleado' | 'Jefatura' | 'RRHH'): void {
    const mockUser = {
      id: '101',
      nombre: `Usuario ${rol}`,
      email: `${rol.toLowerCase()}@nominapro.com`,
      rol: rol,
      departamento: 'Operaciones',
      token: 'jwt_token_mock'
    };
    localStorage.setItem('usuario', JSON.stringify(mockUser));
    this.router.navigate(['/dashboard']);
  }
}