import { Component, OnInit } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PermisoService } from '../../core/services/permiso.service';
import { PermisoSolicitud } from '../../core/models/permiso.model';

@Component({
  selector: 'app-permisos',
  standalone: true,
  imports: [CommonModule, NgClass, FormsModule],
  templateUrl: './permisos.component.html',
  styleUrl: './permisos.component.css'
})
export class PermisosComponent implements OnInit {
  permisos: PermisoSolicitud[] = [];

  // Filtros
  filtroEstado = 'Todos';
  filtroTipo = 'Todos';
  filtroEmpleado = '';

  // Formulario de nueva solicitud
  mostrarModalNueva = false;
  nuevoTipo: PermisoSolicitud['tipo'] = 'Médico';
  nuevaFechaInicio = '';
  nuevaFechaFin = '';
  nuevaHoraInicio = '08:00';
  nuevaHoraFin = '17:00';
  nuevoMotivo = '';
  nuevoConGoce = true;

  // Modal de revisión de jefatura
  permisoRevision: PermisoSolicitud | null = null;
  obsJefatura = '';
  mensajeAlerta = '';

  constructor(private permisoService: PermisoService) {}

  ngOnInit(): void {
    this.cargarPermisos();
  }

  cargarPermisos(): void {
    this.permisoService.obtenerPermisos(this.filtroEstado, this.filtroTipo, this.filtroEmpleado)
      .subscribe(data => this.permisos = data);
  }

  abrirNuevaSolicitud(): void {
    const hoy = new Date().toISOString().split('T')[0];
    this.nuevaFechaInicio = hoy;
    this.nuevaFechaFin = hoy;
    this.nuevoMotivo = '';
    this.mostrarModalNueva = true;
  }

  guardarSolicitud(): void {
    if (!this.nuevoMotivo.trim()) return;

    const resultado = this.permisoService.crearSolicitud({
      empleadoId: '101',
      nombreEmpleado: 'Natalia Arce',
      avatar: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=100&h=100&fit=crop&crop=faces',
      departamento: 'Operaciones',
      tipo: this.nuevoTipo,
      fechaInicio: this.nuevaFechaInicio,
      fechaFin: this.nuevaFechaFin,
      horaInicio: this.nuevaHoraInicio,
      horaFin: this.nuevaHoraFin,
      motivo: this.nuevoMotivo,
      conGoceSalario: this.nuevoConGoce
    });

    this.mostrarAlerta(resultado.mensaje);
    if (resultado.exito) {
      this.mostrarModalNueva = false;
      this.cargarPermisos();
    }
  }

  abrirRevision(p: PermisoSolicitud): void {
    this.permisoRevision = p;
    this.obsJefatura = '';
  }

  resolver(aprobado: boolean): void {
    if (!this.permisoRevision) return;

    this.permisoService.resolverSolicitud(
      this.permisoRevision.id, 
      aprobado, 
      this.obsJefatura || (aprobado ? 'Aprobado por jefatura.' : 'Rechazado.'), 
      'Paul Morales (Jefatura)'
    );

    this.mostrarAlerta(aprobado ? 'Permiso aprobado correctamente.' : 'Permiso rechazado.');
    this.permisoRevision = null;
    this.cargarPermisos();
  }

  private mostrarAlerta(msg: string): void {
    this.mensajeAlerta = msg;
    setTimeout(() => this.mensajeAlerta = '', 4000);
  }
}