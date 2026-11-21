import { Component, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  loginForm;
  error = '';
  cargando = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private cdRef: ChangeDetectorRef
  ) {
    this.loginForm = this.fb.group({
      correo: ['', [Validators.required, Validators.email]],
      contrasena: ['', Validators.required]
    });
  }

  ingresar() {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.cargando = true;
    this.error = '';

    const { correo, contrasena } = this.loginForm.value;
    if (!correo || !contrasena) {
      this.cargando = false;
      return;
    }

    const credentials = {
      correo: correo,
      password: contrasena
    };

    this.authService.login(credentials).pipe(
      finalize(() => {
        this.cargando = false;
        this.cdRef.detectChanges(); 
      })
    ).subscribe({
      next: (respuesta: any) => {
        console.log('Login exitoso:', respuesta);
        this.router.navigate(['/dashboard']);
      },
      error: (err: any) => {
        this.error = err.error?.mensaje || err.error?.Mensaje || 'Correo o contraseña incorrectos.';
        this.cdRef.detectChanges(); 
        console.error('Error de login:', err);
      }
    });
  }
}