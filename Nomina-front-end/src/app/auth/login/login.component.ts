import { Component } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  loginForm;
  error = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      correo: ['', [Validators.required, Validators.email]],
      contrasena: ['', Validators.required]
    });
  }

  ingresar() {
    if (this.loginForm.invalid) return;

    const { correo, contrasena } = this.loginForm.value;
    if (!correo || !contrasena) return;

    let rolAsignado: 'Empleado' | 'Jefatura' | 'RRHH' = 'Empleado';
    
    if (correo?.includes('rrhh') || correo?.includes('rh')) {
      rolAsignado = 'RRHH';
    } else if (correo?.includes('jefatura') || correo?.includes('jefe')) {
      rolAsignado = 'Jefatura';
    }

    this.authService.guardarSesion('token-simulado', {
      id: '1',
      nombre: correo?.split('@')[0] || 'Usuario',
      correo,
      rol: rolAsignado,
      departamento: rolAsignado === 'RRHH' ? 'Recursos Humanos' : 'Operaciones'
    });

    this.router.navigate(['/dashboard']);
  }
}
