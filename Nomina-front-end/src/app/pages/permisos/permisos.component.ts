import { Component } from '@angular/core';

@Component({
  selector: 'app-permisos',
  standalone: true,
  template: `
    <div class="pagina-contenido">
      <h2>📝 Módulo de Permisos</h2>
      <p class="descripcion">Solicitudes de permiso con y sin goce de salario.</p>
      <div class="caja-info">
        <strong>Próximamente:</strong> Formulario de solicitud y listado de aprobaciones.
      </div>
    </div>
  `,
  styles: [`
    .pagina-contenido { padding: 1rem; }
    .descripcion { color: #6b7280; margin: 0.5rem 0 1.5rem; }
    .caja-info { background: #eff6ff; border-left: 4px solid #2b6cb0; padding: 1rem; border-radius: 0 8px 8px 0; }
  `]
})
export class PermisosComponent {}
