
import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import {
  LiquidacionRespuesta,
  LiquidacionCrear,
  LiquidacionGuardar
} from '../../core/models/liquidacion.model';

import { LiquidacionService } from '../../core/services/liquidacion.service';
import { UsuarioService } from '../../core/services/usuario.service';
import { CatalogoService } from '../../core/services/catalogo.service';
import { BotonDashboardComponent } from '../../shared/components/boton-dashboard/boton-dashboard.component';

@Component({
  selector: 'app-liquidacion',
  standalone: true,
  imports: [CommonModule, FormsModule, BotonDashboardComponent],
  templateUrl: './liquidacion.component.html',
  styleUrls: ['./liquidacion.component.css']
})
export class LiquidacionComponent implements OnInit {

  liquidaciones: LiquidacionRespuesta[] = [];
  empleados: any[] = [];
  empleadosDisponibles: any[] = [];
  motivos: any[] = [];

  filtroEstado = '';
  cargando = false;
  guardando = false;

  mostrarFormulario = false;
  modoEdicion = false;
  liquidacionSeleccionada: LiquidacionRespuesta | null = null;

  formulario = {
    usuarioId: '',
    motivoLiquidacionId: '',
    fechaSalida: ''
  };

  constructor(
    private liquidacionService: LiquidacionService,
    private usuarioService: UsuarioService,
    private catalogoService: CatalogoService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.cargarDatos();
  }

  cargarDatos(): void {
    this.cargarLiquidaciones();
    this.cargarEmpleados();
    this.cargarMotivos();
  }

  private extraerLista(data: any): any[] {
    if (Array.isArray(data)) {
      return data;
    }

    if (data && Array.isArray(data.$values)) {
      return data.$values;
    }

    if (data && Array.isArray(data.resultado)) {
      return data.resultado;
    }

    return [];
  }

  cargarLiquidaciones(): void {
    this.cargando = true;

    this.liquidacionService.obtenerTodas(
      this.filtroEstado || undefined
    ).subscribe({
      next: (respuesta) => {
        this.liquidaciones = this.extraerLista(respuesta);
        this.actualizarEmpleadosDisponibles();
        this.cargando = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error al cargar liquidaciones:', error);
        alert(error.error?.mensaje || 'No fue posible cargar las liquidaciones.');
        this.cargando = false;
        this.cdr.detectChanges();
      }
    });
  }

  cargarEmpleados(): void {
    this.usuarioService.obtenerTodos().subscribe({
      next: (respuesta) => {
        this.empleados = this.extraerLista(respuesta);
        this.actualizarEmpleadosDisponibles();
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error al cargar empleados:', error);
        alert(error.error?.mensaje || 'No fue posible cargar los empleados.');
      }
    });
  }

  cargarMotivos(): void {
    this.catalogoService.obtenerPorTipo('motivos-liquidacion').subscribe({
      next: (respuesta: any) => {
        const lista = this.extraerLista(respuesta);

        this.motivos = lista.map((m: any) => ({
          id: m.id ?? m.Id,
          nombre: m.nombre ?? m.Nombre,
          descripcion: m.descripcion ?? m.Descripcion,
          pagaPreaviso: m.pagaPreaviso ?? m.PagaPreaviso,
          pagaCesantia: m.pagaCesantia ?? m.PagaCesantia
        }));

        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error al cargar motivos:', error);
        alert(error.error?.mensaje || 'No fue posible cargar los motivos de liquidación.');
      }
    });
  }

  actualizarEmpleadosDisponibles(): void {
    const usuariosLiquidados = new Set(
      this.liquidaciones.map(l => l.usuarioId)
    );

    this.empleadosDisponibles = this.empleados.filter((empleado: any) => {
      const id = empleado.id ?? empleado.Id;
      const estado = empleado.estado ?? empleado.Estado;

      return estado === 'Activo' && !usuariosLiquidados.has(id);
    });
  }

  aplicarFiltro(): void {
    this.cargarLiquidaciones();
  }

  abrirFormulario(): void {
    this.modoEdicion = false;
    this.liquidacionSeleccionada = null;

    this.formulario = {
      usuarioId: '',
      motivoLiquidacionId: '',
      fechaSalida: this.obtenerFechaActual()
    };

    this.mostrarFormulario = true;
  }

  editarLiquidacion(liquidacion: LiquidacionRespuesta): void {
    if (liquidacion.estado !== 'Borrador') {
      alert('Solo se pueden modificar liquidaciones en estado Borrador.');
      return;
    }

    this.modoEdicion = true;
    this.liquidacionSeleccionada = liquidacion;

    this.formulario = {
      usuarioId: liquidacion.usuarioId,
      motivoLiquidacionId: liquidacion.motivoId,
      fechaSalida: liquidacion.fechaSalida.substring(0, 10)
    };

    this.mostrarFormulario = true;
  }

  cerrarFormulario(): void {
    this.mostrarFormulario = false;
    this.liquidacionSeleccionada = null;
    this.modoEdicion = false;
  }

  guardarLiquidacion(): void {
    if (!this.formulario.motivoLiquidacionId || !this.formulario.fechaSalida) {
      alert('Debe completar el motivo y la fecha de salida.');
      return;
    }

    this.guardando = true;

    if (this.modoEdicion && this.liquidacionSeleccionada) {
      const dto: LiquidacionGuardar = {
        motivoLiquidacionId: this.formulario.motivoLiquidacionId,
        fechaSalida: this.formulario.fechaSalida
      };

      this.liquidacionService.recalcular(
        this.liquidacionSeleccionada.id,
        dto
      ).subscribe({
        next: () => this.finalizarGuardado('Liquidación actualizada correctamente.'),
        error: (error) => this.manejarError(error)
      });

    } else {
      if (!this.formulario.usuarioId) {
        this.guardando = false;
        alert('Debe seleccionar un empleado.');
        return;
      }

      const dto: LiquidacionCrear = {
        usuarioId: this.formulario.usuarioId,
        motivoLiquidacionId: this.formulario.motivoLiquidacionId,
        fechaSalida: this.formulario.fechaSalida
      };

      this.liquidacionService.crear(dto).subscribe({
        next: () => this.finalizarGuardado('Liquidación creada correctamente.'),
        error: (error) => this.manejarError(error)
      });
    }
  }

  private finalizarGuardado(mensaje: string): void {
    this.guardando = false;
    this.cerrarFormulario();
    alert(mensaje);
    this.cargarDatos();
  }

  private manejarError(error: any): void {
    console.error('Error en liquidación:', error);
    this.guardando = false;
    this.cdr.detectChanges();
    alert(error.error?.mensaje || 'Ocurrió un error al procesar la liquidación.');
  }

  eliminarLiquidacion(liquidacion: LiquidacionRespuesta): void {
    if (liquidacion.estado !== 'Borrador') {
      alert('Solo se pueden eliminar liquidaciones en estado Borrador.');
      return;
    }

    if (!confirm(`¿Está seguro de eliminar la liquidación de ${liquidacion.nombreEmpleado}?`)) {
      return;
    }

    this.liquidacionService.eliminar(liquidacion.id).subscribe({
      next: () => {
        alert('Liquidación eliminada correctamente.');
        this.cargarDatos();
      },
      error: (error) => this.manejarError(error)
    });
  }

  aprobarLiquidacion(liquidacion: LiquidacionRespuesta): void {
    if (liquidacion.estado !== 'Borrador') {
      return;
    }

    const confirmar = confirm(
      `¿Desea aprobar la liquidación de ${liquidacion.nombreEmpleado}?\n\n` +
      'Esta acción generará el comprobante PDF y desactivará al empleado.'
    );

    if (!confirmar) {
      return;
    }

    this.liquidacionService.aprobar(liquidacion.id).subscribe({
      next: () => {
        alert('Liquidación aprobada correctamente.');
        this.cargarDatos();
      },
      error: (error) => this.manejarError(error)
    });
  }

  pagarLiquidacion(liquidacion: LiquidacionRespuesta): void {
    if (liquidacion.estado !== 'Aprobada') {
      return;
    }

    if (!confirm(`¿Confirmar el pago de la liquidación de ${liquidacion.nombreEmpleado}?`)) {
      return;
    }

    this.liquidacionService.pagar(liquidacion.id).subscribe({
      next: () => {
        alert('Pago registrado correctamente.');
        this.cargarLiquidaciones();
      },
      error: (error) => this.manejarError(error)
    });
  }

  verDetalle(liquidacion: LiquidacionRespuesta): void {
    this.liquidacionSeleccionada = liquidacion;
  }

  cerrarDetalle(): void {
    this.liquidacionSeleccionada = null;
  }

  descargarPdf(liquidacion: LiquidacionRespuesta): void {
    this.liquidacionService.obtenerPdf(liquidacion.id).subscribe({
      next: (archivo) => {
        const url = window.URL.createObjectURL(archivo);
        const enlace = document.createElement('a');

        enlace.href = url;
        enlace.download = `Liquidacion_${liquidacion.identificacion}.pdf`;
        enlace.click();

        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        console.error('Error al descargar PDF:', error);
        alert(error.error?.mensaje || 'No fue posible descargar el comprobante.');
      }
    });
  }

  obtenerFechaActual(): string {
    const fecha = new Date();
    const anio = fecha.getFullYear();
    const mes = String(fecha.getMonth() + 1).padStart(2, '0');
    const dia = String(fecha.getDate()).padStart(2, '0');

    return `${anio}-${mes}-${dia}`;
  }

  obtenerClaseEstado(estado: string): string {
    switch (estado) {
      case 'Borrador':
        return 'bg-warning text-dark';
      case 'Aprobada':
        return 'bg-primary';
      case 'Pagada':
        return 'bg-success';
      default:
        return 'bg-secondary';
    }
  }
}

