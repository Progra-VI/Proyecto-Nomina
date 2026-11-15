import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AsistenciaService } from '../../core/services/asistencia.service';
import { AsistenciaRegistro, ResumenMensualAsistencia } from '../../core/models/asistencia.model';

@Component({
  selector: 'app-asistencia',
  standalone: true,
  imports: [CommonModule, NgClass, FormsModule],
  templateUrl: './asistencia.component.html',
  styleUrl: './asistencia.component.css'
})
export class AsistenciaComponent implements OnInit, OnDestroy {
  registros: AsistenciaRegistro[] = [];
  resumenMensual: ResumenMensualAsistencia[] = [];

  horaActual = '';
  fechaActual = '';
  private timer: any;

  filtroDepartamento = 'Todos';
  filtroEmpleado = '';
  filtroFechaInicio = '';
  filtroFechaFin = '';

  vistaActiva: 'marcas' | 'resumen' = 'marcas';

  usuarioActual: any = { nombre: 'Natalia Arce', rol: 'RRHH', departamento: 'Operaciones' };

  registroEditando: AsistenciaRegistro | null = null;
  editEntrada = '';
  editSalida = '';
  editEstado: AsistenciaRegistro['estado'] = 'Normal';
  editJustificacion = '';
  mensajeAlerta = '';

  constructor(private asistenciaService: AsistenciaService) {}

  ngOnInit(): void {
    const guardado = localStorage.getItem('usuario') || localStorage.getItem('user_session');
    if (guardado) {
      const parsed = JSON.parse(guardado);
      this.usuarioActual = {
        nombre: parsed.nombre || 'Natalia Arce',
        rol: parsed.rol || 'RRHH',
        departamento: parsed.departamento || 'Operaciones'
      };
    }

    this.actualizarReloj();
    this.timer = setInterval(() => this.actualizarReloj(), 1000);
    this.cargarDatos();
  }

  ngOnDestroy(): void {
    if (this.timer) clearInterval(this.timer);
  }

  actualizarReloj(): void {
    const ahora = new Date();
    this.horaActual = ahora.toLocaleTimeString('es-CR', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
    this.fechaActual = ahora.toLocaleDateString('es-CR', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });
  }

  cargarDatos(): void {
    this.asistenciaService
      .obtenerRegistros(this.filtroDepartamento, this.filtroEmpleado, this.filtroFechaInicio, this.filtroFechaFin)
      .subscribe(data => (this.registros = data));

    this.asistenciaService.obtenerResumenMensual().subscribe(res => (this.resumenMensual = res));
  }

  marcar(tipo: 'entrada' | 'salida'): void {
    this.asistenciaService
      .registrarMarca('101', this.usuarioActual.nombre, this.usuarioActual.departamento, tipo)
      .subscribe(reg => {
        const horaMarca = tipo === 'entrada' ? reg.horaEntrada : reg.horaSalida;
        this.mostrarAlerta(`Marca de ${tipo.toUpperCase()} registrada a las ${horaMarca} (${reg.estado})`);
        this.cargarDatos();
      });
  }

  limpiarFiltros(): void {
    this.filtroDepartamento = 'Todos';
    this.filtroEmpleado = '';
    this.filtroFechaInicio = '';
    this.filtroFechaFin = '';
    this.cargarDatos();
  }

  abrirAjuste(reg: AsistenciaRegistro): void {
    this.registroEditando = { ...reg };
    this.editEntrada = reg.horaEntrada || '';
    this.editSalida = reg.horaSalida || '';
    this.editEstado = reg.estado;
    this.editJustificacion = reg.justificacion || '';
  }

  cerrarAjuste(): void {
    this.registroEditando = null;
  }

  guardarAjuste(): void {
    if (!this.registroEditando || !this.editJustificacion.trim()) return;

    this.asistenciaService
      .ajustarMarcaRRHH(
        this.registroEditando.id,
        this.editEntrada,
        this.editSalida,
        this.editEstado,
        this.editJustificacion,
        this.usuarioActual.nombre
      )
      .subscribe(() => {
        this.mostrarAlerta('Registro actualizado y auditado por RR.HH.');
        this.cerrarAjuste();
        this.cargarDatos();
      });
  }

  private mostrarAlerta(msg: string): void {
    this.mensajeAlerta = msg;
    setTimeout(() => (this.mensajeAlerta = ''), 4000);
  }
}