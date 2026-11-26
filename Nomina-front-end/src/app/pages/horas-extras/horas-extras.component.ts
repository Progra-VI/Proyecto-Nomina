import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HoraExtraService } from '../../core/services/hora-extra.service';
import {
  HoraExtraGuardarRequest,
  HoraExtraRespuesta,
  ReporteHorasExtra,
  TipoHoraExtra
} from '../../core/models/hora-extra.model';

@Component({
  selector: 'app-horas-extras',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './horas-extras.component.html',
  styleUrl: './horas-extras.component.css'
})
export class HorasExtrasComponent implements OnInit {
  vistaActiva: 'solicitudes' | 'reportes' | 'periodo' = 'solicitudes';

  tipos: TipoHoraExtra[] = [];
  solicitudes: HoraExtraRespuesta[] = [];
  aprobadasPeriodo: HoraExtraRespuesta[] = [];
  reporte: ReporteHorasExtra | null = null;

  mensajeExito = '';
  mensajeError = '';
  cargando = false;

  filtroEstado = 'Todos';
  filtroTipoId = 'Todos';
  filtroDepartamentoId = '';
  filtroUsuarioId = '';
  filtroDesde = '';
  filtroHasta = '';

  mostrarModalFormulario = false;
  idEditando: string | null = null;
  formData: HoraExtraGuardarRequest = this.crearFormularioVacio();

  solicitudRevision: HoraExtraRespuesta | null = null;
  observacionRevision = '';

  constructor(private horaExtraService: HoraExtraService) {}

  ngOnInit(): void {
    const hoy = new Date();
    const primerDiaMes = new Date(hoy.getFullYear(), hoy.getMonth(), 1).toISOString().split('T')[0];
    const ultimoDiaMes = new Date(hoy.getFullYear(), hoy.getMonth() + 1, 0).toISOString().split('T')[0];

    this.filtroDesde = primerDiaMes;
    this.filtroHasta = ultimoDiaMes;

    this.cargarCatalogoTipos();
    this.cargarTodo();
  }

  private crearFormularioVacio(): HoraExtraGuardarRequest {
    const hoy = new Date().toISOString().split('T')[0];
    return {
      tipoHoraExtraId: this.tipos[0]?.id ?? '',
      fecha: hoy,
      horaInicio: '17:00',
      horaFin: '19:00',
      justificacion: ''
    };
  }

  cargarCatalogoTipos(): void {
    this.horaExtraService.obtenerTipos().subscribe({
      next: (data) => {
        this.tipos = data;
        if (!this.formData.tipoHoraExtraId && data.length > 0) {
          this.formData.tipoHoraExtraId = data[0].id;
        }
      },
      error: () => {
        this.tipos = [];
      }
    });
  }

  cargarTodo(): void {
    this.cargarSolicitudes();
    this.cargarReporte();
    this.cargarAprobadasPeriodo();
  }

  cargarSolicitudes(): void {
    this.cargando = true;
    this.limpiarAlertas();

    this.horaExtraService
      .listar({
        usuarioId: this.filtroUsuarioId.trim() || undefined,
        departamentoId: this.filtroDepartamentoId.trim() || undefined,
        estado: this.filtroEstado,
        tipoId: this.filtroTipoId,
        desde: this.filtroDesde || undefined,
        hasta: this.filtroHasta || undefined
      })
      .subscribe({
        next: (data) => {
          this.solicitudes = data;
          this.cargando = false;
        },
        error: (err) => {
          this.cargando = false;
          this.mensajeError = err?.error?.mensaje ?? 'No se pudieron cargar las solicitudes de horas extras.';
        }
      });
  }

  cargarReporte(): void {
    this.horaExtraService
      .obtenerReporte({
        usuarioId: this.filtroUsuarioId.trim() || undefined,
        departamentoId: this.filtroDepartamentoId.trim() || undefined,
        desde: this.filtroDesde || undefined,
        hasta: this.filtroHasta || undefined
      })
      .subscribe({
        next: (rep) => (this.reporte = rep),
        error: () => (this.reporte = null)
      });
  }

  cargarAprobadasPeriodo(): void {
    this.horaExtraService
      .listar({
        usuarioId: this.filtroUsuarioId.trim() || undefined,
        departamentoId: this.filtroDepartamentoId.trim() || undefined,
        estado: 'Aprobada',
        desde: this.filtroDesde || undefined,
        hasta: this.filtroHasta || undefined
      })
      .subscribe({
        next: (data) => (this.aprobadasPeriodo = data),
        error: () => (this.aprobadasPeriodo = [])
      });
  }

  limpiarFiltros(): void {
    this.filtroEstado = 'Todos';
    this.filtroTipoId = 'Todos';
    this.filtroDepartamentoId = '';
    this.filtroUsuarioId = '';
    this.cargarTodo();
  }

  abrirNueva(): void {
    this.idEditando = null;
    this.formData = this.crearFormularioVacio();
    this.mostrarModalFormulario = true;
  }

  abrirEditar(item: HoraExtraRespuesta): void {
    if (!item.id) return;
    this.idEditando = item.id;
    this.formData = {
      tipoHoraExtraId: item.tipoHoraExtraId,
      fecha: item.fecha,
      horaInicio: item.horaInicio,
      horaFin: item.horaFin,
      justificacion: item.justificacion
    };
    this.mostrarModalFormulario = true;
  }

  cerrarFormulario(): void {
    this.mostrarModalFormulario = false;
    this.idEditando = null;
  }

  guardarSolicitud(): void {
    if (
      !this.formData.tipoHoraExtraId ||
      !this.formData.fecha ||
      !this.formData.horaInicio ||
      !this.formData.horaFin ||
      this.formData.justificacion.trim().length < 5
    ) {
      this.mensajeError = 'Completa todos los campos y asegúrate de que la justificación tenga al menos 5 caracteres.';
      return;
    }

    this.limpiarAlertas();
    const payload: HoraExtraGuardarRequest = {
      ...this.formData,
      justificacion: this.formData.justificacion.trim()
    };

    const peticion = this.idEditando
      ? this.horaExtraService.editar(this.idEditando, payload)
      : this.horaExtraService.crear(payload);

    peticion.subscribe({
      next: () => {
        this.mensajeExito = this.idEditando
          ? 'Solicitud de hora extra actualizada.'
          : 'Solicitud de hora extra registrada.';
        this.cerrarFormulario();
        this.cargarTodo();
      },
      error: (err) => {
        this.mensajeError = err?.error?.mensaje ?? 'No se pudo guardar la solicitud.';
      }
    });
  }

  cancelarSolicitud(item: HoraExtraRespuesta): void {
    if (!item.id) return;
    this.limpiarAlertas();

    this.horaExtraService.cancelar(item.id).subscribe({
      next: () => {
        this.mensajeExito = 'Solicitud cancelada.';
        this.cargarTodo();
      },
      error: (err) => {
        this.mensajeError = err?.error?.mensaje ?? 'No se pudo cancelar la solicitud.';
      }
    });
  }

  abrirRevision(item: HoraExtraRespuesta): void {
    this.solicitudRevision = item;
    this.observacionRevision = '';
    this.limpiarAlertas();
  }

  cerrarRevision(): void {
    this.solicitudRevision = null;
    this.observacionRevision = '';
  }

  resolverSolicitud(aprobar: boolean): void {
    if (!this.solicitudRevision?.id) return;

    if (!aprobar && !this.observacionRevision.trim()) {
      this.mensajeError = 'La observación es obligatoria para rechazar una solicitud.';
      return;
    }

    this.limpiarAlertas();
    const id = this.solicitudRevision.id;
    const obs = this.observacionRevision.trim();

    const peticion = aprobar
      ? this.horaExtraService.aprobar(id, obs || undefined)
      : this.horaExtraService.rechazar(id, obs);

    peticion.subscribe({
      next: () => {
        this.mensajeExito = aprobar
          ? 'Hora extra aprobada y validada contra asistencia.'
          : 'Solicitud de hora extra rechazada.';
        this.cerrarRevision();
        this.cargarTodo();
      },
      error: (err) => {
        this.mensajeError =
          err?.error?.mensaje ??
          'No se pudo resolver la solicitud (verifica asistencia y rol de jefatura).';
      }
    });
  }

  get totalHorasPeriodo(): number {
    return this.aprobadasPeriodo.reduce((acc, item) => acc + item.horas, 0);
  }

  get totalMontoPeriodo(): number {
    return this.aprobadasPeriodo.reduce((acc, item) => acc + item.monto, 0);
  }

  private limpiarAlertas(): void {
    this.mensajeExito = '';
    this.mensajeError = '';
  }
}