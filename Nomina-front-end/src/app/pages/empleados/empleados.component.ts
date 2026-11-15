import { Component } from '@angular/core';

@Component({
  selector: 'app-empleados',
  standalone: true,
  template: `
    <div class="pagina-contenido">
      <h2>👥 Mantenimiento de Empleados</h2>
      <p class="descripcion">Registro, edición y gestión de datos del personal.</p>
      <div class="caja-info">
        <strong>Próximamente:</strong> Ficha de empleado, historial y asignación de roles.
      </div>
    </div>
  `,
  styles: [`
    .pagina-contenido { padding: 1rem; }
    .descripcion { color: #6b7280; margin: 0.5rem 0 1.5rem; }
    .caja-info { background: #eff6ff; border-left: 4px solid #2b6cb0; padding: 1rem; border-radius: 0 8px 8px 0; }
  `]
})
export class EmpleadosComponent {}
