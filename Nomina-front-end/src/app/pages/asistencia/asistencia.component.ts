import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AsistenciaService } from '../../core/services/asistencia.service';
import { AsistenciaRegistro, ResumenMensualAsistencia } from '../../core/models/asistencia.model';
import { BotonDashboardComponent } from '../../shared/components/boton-dashboard/boton-dashboard.component';

@Component({
  selector: 'app-asistencia',
  standalone: true,
  imports: [CommonModule, FormsModule, BotonDashboardComponent],
  templateUrl: './asistencia.component.html',
  styleUrl: './asistencia.component.css'
})
export class AsistenciaComponent implements OnInit, OnDestroy {
  fechaActual = '';
  horaActual = '';
  mensajeAlerta = '';
  vistaActiva: 'marcas' | 'resumen' = 'marcas';

  filtroEmpleado = '';
  filtroDepartamento = 'Todos';
  filtroFechaInicio = '';
  filtroFechaFin = '';

  registros: AsistenciaRegistro[] = [];
  resumenMensual: ResumenMensualAsistencia[] = [];

  registroEditando: AsistenciaRegistro | null = null;
  editEntrada = '';
  editSalida = '';
  editEstado: AsistenciaRegistro['estado'] = 'Normal';
  editJustificacion = '';

  private relojInterval?: ReturnType<typeof setInterval>;

  constructor(private asistenciaService: AsistenciaService) {}

  ngOnInit(): void {
    this.actualizarReloj();
    this.relojInterval = setInterval(() => this.actualizarReloj(), 1000);
    this.cargarDatos();
  }

  ngOnDestroy(): void {
    if (this.relojInterval) clearInterval(this.relojInterval);
  }

  private actualizarReloj(): void {
    const ahora = new Date();
    this.fechaActual = ahora.toLocaleDateString('es-CR', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
    this.horaActual = ahora.toTimeString().slice(0, 8);
  }

  cargarDatos(): void {
    this.asistenciaService
      .obtenerRegistros(this.filtroDepartamento, this.filtroEmpleado, this.filtroFechaInicio, this.filtroFechaFin)
      .subscribe(data => (this.registros = data));

    this.asistenciaService
      .obtenerResumenMensual()
      .subscribe(data => (this.resumenMensual = data));
  }

  limpiarFiltros(): void {
    this.filtroEmpleado = '';
    this.filtroDepartamento = 'Todos';
    this.filtroFechaInicio = '';
    this.filtroFechaFin = '';
    this.cargarDatos();
  }

  marcar(tipo: 'entrada' | 'salida'): void {
    // Reemplaza estos datos fijos por los del usuario autenticado cuando conectes Auth
    this.asistenciaService
      .registrarMarca('103', 'Sebastian', 'TI', tipo)
      .subscribe(() => {
        this.mensajeAlerta = tipo === 'entrada'
          ? 'Entrada registrada correctamente.'
          : 'Salida registrada correctamente.';
        this.cargarDatos();
      });
  }

  abrirAjuste(reg: AsistenciaRegistro): void {
    this.registroEditando = reg;
    this.editEntrada = reg.horaEntrada ?? '';
    this.editSalida = reg.horaSalida ?? '';
    this.editEstado = reg.estado;
    this.editJustificacion = reg.justificacion ?? '';
  }

  cerrarAjuste(): void {
    this.registroEditando = null;
    this.editJustificacion = '';
  }

  guardarAjuste(): void {
    if (!this.registroEditando || !this.editJustificacion.trim()) return;

    this.asistenciaService
      .ajustarMarcaRRHH(
        this.registroEditando.id,
        this.editEntrada || null,
        this.editSalida || null,
        this.editEstado,
        this.editJustificacion.trim(),
        'Admin RRHH'
      )
      .subscribe(() => {
        this.mensajeAlerta = 'Ajuste de asistencia guardado.';
        this.cerrarAjuste();
        this.cargarDatos();
      });
  }
}