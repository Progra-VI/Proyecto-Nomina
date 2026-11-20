import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common'; 

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, CommonModule], 
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent {
  usuario: any;
  mostrarModal: boolean = true;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    this.usuario = this.authService.obtenerUsuario();
  }

  tieneRol(roles: string[]): boolean {
    return roles.includes(this.usuario?.rol || '');
  }

  cerrarModal() {
    this.mostrarModal = false; 
  }

  salir() {
    this.authService.cerrarSesion();
    this.router.navigate(['/login']);
  }
}