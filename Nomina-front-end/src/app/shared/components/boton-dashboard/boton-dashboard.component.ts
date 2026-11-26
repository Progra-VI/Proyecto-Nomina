import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-boton-dashboard',
  standalone: true,
  templateUrl: './boton-dashboard.component.html',
  styleUrl: './boton-dashboard.component.css'
})
export class BotonDashboardComponent {

  constructor(private router: Router) {}

  volverDashboard(): void {
    this.router.navigate(['/dashboard']);
  }
}