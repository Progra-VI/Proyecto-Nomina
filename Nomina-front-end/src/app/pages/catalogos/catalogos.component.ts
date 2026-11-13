import { Component } from '@angular/core';

@Component({
  selector: 'app-catalogos',
  standalone: true,
  template: `
    <div class="pagina-contenido">
      <h2>📊 Catálogos del Sistema</h2>
      <p class="descripcion">Configuración de conceptos, puestos, departamentos y parámetros.</p>
      <div class="caja-info">
        <strong>Próximamente:</strong> Administración de tablas y valores maestros.
      </div>
    </div>
  `,
  styles: [`
    .pagina-contenido { padding: 1rem; }
    .descripcion { color: #6b7280; margin: 0.5rem 0 1.5rem; }
    .caja-info { background: #eff6ff; border-left: 4px solid #2b6cb0; padding: 1rem; border-radius: 0 8px 8px 0; }
  `]
})
export class CatalogosComponent {}
