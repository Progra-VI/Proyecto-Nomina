import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PermisoService } from '../../core/services/permiso.service';
import { PermisoSolicitud } from '../../core/models/permiso.model';

@Component({
  selector: 'app-permisos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './permisos.component.html',
  styleUrl: './permisos.component.css'
})
export class PermisosComponent implements OnInit {
  permisos: PermisoSolicitud[] = [];
  mensajeAlerta = '';

  filtroEmpleado = '';
  filtroEstado = 'Todos';
  filtroTipo = 'Todos';

  mostrarModalNueva = false;
  nuevoTipo: PermisoSolicitud['tipo'] = 'Médico';
  nuevaFechaInicio = '';
  nuevaFechaFin = '';
  nuevaHoraInicio = '08:00';
  nuevaHoraFin = '17:00';
  nuevoConGoce = true;
  nuevoMotivo = '';

  permisoRevision: PermisoSolicitud | null = null;
  obsJefatura = '';

  constructor(private permisoService: PermisoService) {}

  ngOnInit(): void {
    this.cargarPermisos();
  }

  cargarPermisos(): void {
    this.permisoService
      .obtenerPermisos(this.filtroEstado, this.filtroTipo, this.filtroEmpleado)
      .subscribe(data => {
        this.permisos = data;
      });
  }

  abrirNuevaSolicitud(): void {
    const hoy = new Date().toISOString().split('T')[0];
    this.nuevoTipo = 'Médico';
    this.nuevaFechaInicio = hoy;
    this.nuevaFechaFin = hoy;
    this.nuevaHoraInicio = '08:00';
    this.nuevaHoraFin = '17:00';
    this.nuevoConGoce = true;
    this.nuevoMotivo = '';
    this.mostrarModalNueva = true;
  }

  guardarSolicitud(): void {
    if (!this.nuevoMotivo.trim()) return;

    const payload = {
      tipo: this.nuevoTipo,
      fechaInicio: this.nuevaFechaInicio,
      fechaFin: this.nuevaFechaFin,
      horaInicio: this.nuevaHoraInicio,
      horaFin: this.nuevaHoraFin,
      conGoceSalario: this.nuevoConGoce,
      motivo: this.nuevoMotivo.trim()
    };

    const res = this.permisoService.crearPermiso(payload);
    this.mensajeAlerta = res.mensaje;
    this.mostrarModalNueva = false;
    this.cargarPermisos();
  }

  abrirRevision(permiso: PermisoSolicitud): void {
    this.permisoRevision = permiso;
    this.obsJefatura = '';
  }

  resolver(aprobado: boolean): void {
    if (!this.permisoRevision) return;

    const nuevoEstado = aprobado ? 'Aprobado' : 'Rechazado';
    this.permisoService
      .actualizarEstadoPermiso(
        this.permisoRevision.id,
        nuevoEstado,
        this.obsJefatura.trim(),
        'Jefatura'
      )
      .subscribe(() => {
        this.mensajeAlerta = `Solicitud ${nuevoEstado.toLowerCase()}.`;
        this.permisoRevision = null;
        this.obsJefatura = '';
        this.cargarPermisos();
      });
  }
}