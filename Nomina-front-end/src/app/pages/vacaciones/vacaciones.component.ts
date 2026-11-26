import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { VacacionService } from '../../core/services/vacacion.service';
import { AuthService } from '../../core/services/auth.service';
import { BotonDashboardComponent } from '../../shared/components/boton-dashboard/boton-dashboard.component';

import {
  VacacionSolicitud,
  SaldoVacaciones,
  VacacionGuardar
} from '../../core/models/vacacion.model';



@Component({
  selector: 'app-vacaciones',
  standalone: true,
  imports: [ CommonModule,
  FormsModule,
  BotonDashboardComponent],
  templateUrl: './vacaciones.component.html',
  styleUrl: './vacaciones.component.css'
})
export class VacacionesComponent implements OnInit {

  usuario: any = null;
  rol = '';

  saldo: SaldoVacaciones | null = null;
  vacaciones: VacacionSolicitud[] = [];

  mensajeAlerta = '';
  tipoAlerta: 'exito' | 'error' = 'exito';
  cargando = false;

  filtroEstado = 'Todos';
  filtroDesde = '';
  filtroHasta = '';

  mostrarModalNueva = false;
  mostrarModalRevision = false;

  solicitudSeleccionada: VacacionSolicitud | null = null;

  fechaInicio = '';
  fechaFin = '';
  motivo = '';

  observacion = '';

  constructor(
    private vacacionService: VacacionService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.usuario = this.authService.obtenerUsuario();
    this.rol = this.usuario?.rol ?? '';

    this.cargarDatos();
  }

  // Permisos según el rol
  get esEmpleado(): boolean {
    return this.rol === 'Empleado';
  }

  get esJefatura(): boolean {
    return this.rol === 'Jefatura';
  }

  get esRRHH(): boolean {
    return this.rol === 'RRHH';
  }

  get puedeCrear(): boolean {
    return this.esEmpleado || this.esJefatura;
  }

  get puedeResolver(): boolean {
    return this.esJefatura;
  }

  // Carga inicial
  cargarDatos(): void {
  console.log('USUARIO:', this.usuario);
  console.log('ROL:', this.rol);

  this.cargarSaldo();
  this.cargarVacaciones();
  }

  cargarSaldo(): void {
    if (!this.usuario?.id) return;

    this.vacacionService.obtenerSaldo().subscribe({
      next: data => {
        this.saldo = data;
        console.log('SALDO ASIGNADO:', this.saldo);
        this.cdr.markForCheck();
      },
      error: error => {
        console.error('Error al cargar el saldo:', error);
        this.mostrarMensaje(
          this.obtenerError(error, 'No fue posible cargar el saldo de vacaciones.'),
          'error'
        );
        this.cdr.markForCheck();
      }
    });
  }

  cargarVacaciones(): void {
    this.cargando = true;

    this.vacacionService.obtenerVacaciones(
      undefined,
      this.filtroEstado,
      this.filtroDesde,
      this.filtroHasta
    ).subscribe({
      next: data => {
        console.log('TIPO DE RESPUESTA:', Array.isArray(data));
        console.log('CANTIDAD RECIBIDA:', data?.length);

        this.vacaciones = Array.isArray(data) ? data : [];
        this.cargando = false;

        console.log('VACACIONES ASIGNADAS:', this.vacaciones);
        this.cdr.markForCheck();
      },
      error: error => {
        console.error('Error al cargar las solicitudes:', error);
        this.vacaciones = [];
        this.cargando = false;

        this.mostrarMensaje(
          this.obtenerError(error, 'No fue posible cargar las solicitudes.'),
          'error'
        );

        this.cdr.markForCheck();
      }
    });
  }
  aplicarFiltros(): void {
    this.cargarVacaciones();
  }

  limpiarFiltros(): void {
    this.filtroEstado = 'Todos';
    this.filtroDesde = '';
    this.filtroHasta = '';
    this.cargarVacaciones();
  }

 abrirNuevaSolicitud(): void {
  this.solicitudSeleccionada = null;
  this.fechaInicio = '';
  this.fechaFin = '';
  this.motivo = '';
  this.mostrarModalNueva = true;
}

  cerrarModalNueva(): void {
  this.mostrarModalNueva = false;
  this.solicitudSeleccionada = null;
}

  guardarSolicitud(): void {
    if (!this.fechaInicio || !this.fechaFin) {
      this.mostrarMensaje('Debe indicar las fechas de inicio y fin.', 'error');
      return;
    }

    if (this.fechaFin < this.fechaInicio) {
      this.mostrarMensaje(
        'La fecha final no puede ser anterior a la fecha inicial.',
        'error'
      );
      return;
    }

    const solicitud: VacacionGuardar = {
      fechaInicio: this.fechaInicio,
      fechaFin: this.fechaFin,
      motivo: this.motivo.trim() || null
    };

    this.vacacionService.crearSolicitud(solicitud).subscribe({
      next: () => {
        this.mostrarModalNueva = false;
        this.mostrarMensaje('Solicitud de vacaciones creada correctamente.', 'exito');
        this.cargarDatos();
      },
      error: error => {
        this.mostrarMensaje(
          this.obtenerError(error, 'No fue posible crear la solicitud.'),
          'error'
        );
      }
    });
  }

  // Editar solicitud pendiente
  abrirEdicion(solicitud: VacacionSolicitud): void {
    this.solicitudSeleccionada = solicitud;
    this.fechaInicio = solicitud.fechaInicio;
    this.fechaFin = solicitud.fechaFin;
    this.motivo = solicitud.motivo ?? '';
    this.mostrarModalNueva = true;
  }

  guardarEdicion(): void {
    if (!this.solicitudSeleccionada) return;

    if (!this.fechaInicio || !this.fechaFin || this.fechaFin < this.fechaInicio) {
      this.mostrarMensaje('Verifique el rango de fechas seleccionado.', 'error');
      return;
    }

    const solicitud: VacacionGuardar = {
      fechaInicio: this.fechaInicio,
      fechaFin: this.fechaFin,
      motivo: this.motivo.trim() || null
    };

    this.vacacionService.actualizarSolicitud(
      this.solicitudSeleccionada.id,
      solicitud
    ).subscribe({
      next: () => {
        this.mostrarModalNueva = false;
        this.solicitudSeleccionada = null;
        this.mostrarMensaje('Solicitud actualizada correctamente.', 'exito');
        this.cargarDatos();
      },
      error: error => {
        this.mostrarMensaje(
          this.obtenerError(error, 'No fue posible actualizar la solicitud.'),
          'error'
        );
      }
    });
  }

  // Cancelar solicitud
  cancelarSolicitud(solicitud: VacacionSolicitud): void {
    const confirmar = confirm(
      '¿Está seguro de que desea cancelar esta solicitud?'
    );

    if (!confirmar) return;

    this.vacacionService.cancelarSolicitud(solicitud.id).subscribe({
      next: () => {
        this.mostrarMensaje('Solicitud cancelada correctamente.', 'exito');
        this.cargarDatos();
      },
      error: error => {
        this.mostrarMensaje(
          this.obtenerError(error, 'No fue posible cancelar la solicitud.'),
          'error'
        );
      }
    });
  }

  // Revisión por jefatura
  abrirRevision(solicitud: VacacionSolicitud): void {
    this.solicitudSeleccionada = solicitud;
    this.observacion = '';
    this.mostrarModalRevision = true;
  }

  cerrarModalRevision(): void {
    this.mostrarModalRevision = false;
    this.solicitudSeleccionada = null;
    this.observacion = '';
  }

  aprobarSolicitud(): void {
    if (!this.solicitudSeleccionada) return;

    this.vacacionService.aprobarSolicitud(
      this.solicitudSeleccionada.id,
      { observacion: this.observacion.trim() || undefined }
    ).subscribe({
      next: () => {
        this.mostrarMensaje('Solicitud aprobada correctamente.', 'exito');
        this.cerrarModalRevision();
        this.cargarDatos();
      },
      error: error => {
        this.mostrarMensaje(
          this.obtenerError(error, 'No fue posible aprobar la solicitud.'),
          'error'
        );
      }
    });
  }

  rechazarSolicitud(): void {
    if (!this.solicitudSeleccionada) return;

    if (!this.observacion.trim()) {
      this.mostrarMensaje(
        'Debe indicar una observación para rechazar la solicitud.',
        'error'
      );
      return;
    }

    this.vacacionService.rechazarSolicitud(
      this.solicitudSeleccionada.id,
      { observacion: this.observacion.trim() }
    ).subscribe({
      next: () => {
        this.mostrarMensaje('Solicitud rechazada correctamente.', 'exito');
        this.cerrarModalRevision();
        this.cargarDatos();
      },
      error: error => {
        this.mostrarMensaje(
          this.obtenerError(error, 'No fue posible rechazar la solicitud.'),
          'error'
        );
      }
    });
  }

  // Utilidades
  esPropietario(solicitud: VacacionSolicitud): boolean {
    return solicitud.usuarioId === this.usuario?.id;
  }

  puedeEditar(solicitud: VacacionSolicitud): boolean {
    return this.puedeCrear &&
      this.esPropietario(solicitud) &&
      solicitud.estado === 'Pendiente';
  }

  puedeCancelar(solicitud: VacacionSolicitud): boolean {
    return this.esPropietario(solicitud) &&
      solicitud.estado === 'Pendiente';
  }

  puedeAprobar(solicitud: VacacionSolicitud): boolean {
    return this.puedeResolver &&
      !this.esPropietario(solicitud) &&
      solicitud.estado === 'Pendiente';
  }

  mostrarMensaje(
    mensaje: string,
    tipo: 'exito' | 'error'
  ): void {
    this.mensajeAlerta = mensaje;
    this.tipoAlerta = tipo;
  }

  private obtenerError(error: any, mensajePredeterminado: string): string {
    return error?.error?.message ||
      error?.error?.mensaje ||
      error?.error?.title ||
      mensajePredeterminado;
  }

  get nombreRol(): string {
    if (this.esRRHH) return 'Recursos Humanos';
    if (this.esJefatura) return 'Jefatura';
    return 'Colaborador';
  }
}
