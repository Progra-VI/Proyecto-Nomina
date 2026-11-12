import { Component } from '@angular/core';

@Component({
  selector: 'app-planilla',
  standalone: true,
  template: `
    <div class="pagina-contenido">
      <h2>💰 Módulo de Planilla</h2>
      <p class="descripcion">Cálculo mensual de salarios, deducciones y aportes.</p>
      <div class="caja-info">
        <strong>Próximamente:</strong> Procesamiento mensual, reportes y boletas de pago.
      </div>
    </div>
  `,
  styles: [`
    .pagina-contenido { padding: 1rem; }
    .descripcion { color: #6b7280; margin: 0.5rem 0 1.5rem; }
    .caja-info { background: #eff6ff; border-left: 4px solid #2b6cb0; padding: 1rem; border-radius: 0 8px 8px 0; }
  `]
})
export class PlanillaComponent {}
