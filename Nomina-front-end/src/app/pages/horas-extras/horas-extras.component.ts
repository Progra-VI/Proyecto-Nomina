import { Component } from '@angular/core';

@Component({
  selector: 'app-horas-extras',
  standalone: true,
  template: `
    <div class="pagina-contenido">
      <h2>⚡ Módulo de Horas Extras</h2>
      <p class="descripcion">Registro y aprobación de horas trabajadas adicionales.</p>
      <div class="caja-info">
        <strong>Próximamente:</strong> Formulario de reporte y cálculo de recargos.
      </div>
    </div>
  `,
  styles: [`
    .pagina-contenido { padding: 1rem; }
    .descripcion { color: #6b7280; margin: 0.5rem 0 1.5rem; }
    .caja-info { background: #eff6ff; border-left: 4px solid #2b6cb0; padding: 1rem; border-radius: 0 8px 8px 0; }
  `]
})
export class HorasExtrasComponent {}
