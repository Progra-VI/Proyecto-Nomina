import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UsuarioService } from '../../core/services/usuario.service';
import { UsuarioRespuesta } from '../../core/models/usuario.model';
import { CatalogoService, TipoCatalogoApi } from '../../core/services/catalogo.service';
import { BotonDashboardComponent } from '../../shared/components/boton-dashboard/boton-dashboard.component';

@Component({
  selector: 'app-gestion-general',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, BotonDashboardComponent],
  templateUrl: './gestion-general.component.html',
  styleUrls: ['./gestion-general.component.css']
})
export class GestionGeneralComponent implements OnInit {
  seccionActiva: 'usuarios' | 'empleados' | 'catalogos' = 'usuarios';
  catalogoActivo: TipoCatalogoApi = 'departamentos';

  mostrarModalUsuario: boolean = false;
  mostrarModalEmpleado: boolean = false;
  mostrarModalCatalogo: boolean = false;

  usuarios: any[] = [];
  empleados: any[] = [];
  catalogos: any[] = [];
  departamentos: any[] = [];
  configuracionLey: any = null;
tiposCatalogoFiltro: { key: TipoCatalogoApi; label: string }[] = [
  { key: 'departamentos', label: 'Departamentos' },
  { key: 'tipos-permiso', label: 'Tipos de permiso' },
  { key: 'tipos-hora-extra', label: 'Tipos de horas extra' },
  { key: 'motivos-liquidacion', label: 'Motivos de liquidación' },
  { key: 'feriados', label: 'Feriados' },
  { key: 'parametros', label: 'Parámetros' }
];  jefaturas: UsuarioRespuesta[] = [];

  editandoCatalogoId: string | null = null;
  editandoTipoKey: TipoCatalogoApi | null = null;

  usuarioForm!: FormGroup;
  empleadoForm!: FormGroup;
  catalogoForm!: FormGroup;

  constructor(
    private usuarioService: UsuarioService,
    private catalogoService: CatalogoService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.inicializarFormularios();
    this.cargarDepartamentos();
    this.cargarUsuarios();
    this.cargarJefaturas();
  }

  inicializarFormularios(): void {
    this.usuarioForm = this.fb.group({
      identificacion: ['', Validators.required],
      nombre: ['', Validators.required],
      correo: ['', [Validators.required, Validators.email]],
      rol: ['Empleado', Validators.required],
      departamentoId: ['', Validators.required],
      puesto: ['', Validators.required],
      jefaturaDirectaId: [null],
      fechaIngreso: ['', Validators.required],
      salarioBase: [0, [Validators.required, Validators.min(0.01)]],
      tipoJornada: ['Diurna', Validators.required],
      fotoUrl: [''],
      password: ['', [Validators.required, Validators.minLength(8)]],
      horario: this.fb.group({
        horaEntrada: ['08:00', Validators.required],
        horaSalida: ['17:00', Validators.required],
        diasLaborales: [[1, 2, 3, 4, 5], Validators.required]
      })
    });

    this.empleadoForm = this.fb.group({
      usuarioId: ['', Validators.required],
      numeroExpediente: ['', Validators.required],
      tipoContrato: ['Indefinido', Validators.required],
      cuentaBancaria: ['', Validators.required]
    });

    this.catalogoForm = this.fb.group({
      tipoCatalogo: ['departamentos', Validators.required],
      nombre: ['', Validators.required],
      descripcion: [''],
      fecha: [''],
      recargo: [1.5],
      pagaPreaviso: [false],
      pagaCesantia: [false],
      clave: [''],
      valor: ['']
    });

    // Validación dinámica de la jefatura directa según el rol.
    const rolControl = this.usuarioForm.get('rol');
    const jefaturaControl = this.usuarioForm.get('jefaturaDirectaId');

    const actualizarValidacionJefatura = (rol: string) => {
      if (rol === 'Empleado') {
        jefaturaControl?.setValidators([Validators.required]);
      } else {
        jefaturaControl?.clearValidators();
      }

      jefaturaControl?.updateValueAndValidity();
    };

    // Aplicar validación inicial.
    actualizarValidacionJefatura(rolControl?.value);

    // Actualizar cuando cambie el rol.
    rolControl?.valueChanges.subscribe(rol => {
      actualizarValidacionJefatura(rol);
    });
  }

  cambiarSeccion(seccion: 'usuarios' | 'empleados' | 'catalogos'): void {
    this.seccionActiva = seccion;
    
    if (seccion === 'usuarios') this.cargarUsuarios();
    if (seccion === 'empleados') this.cargarEmpleados();
    if (seccion === 'catalogos') this.cargarCatalogos();
  }

  abrirModal(): void {
    if (this.seccionActiva === 'usuarios') {
      
      this.mostrarModalUsuario = true;
      this.usuarioForm.reset({
        rol: 'Empleado',
        tipoJornada: 'Diurna',
        salarioBase: 0,
        horario: {
          horaEntrada: '08:00',
          horaSalida: '17:00',
          diasLaborales: [1, 2, 3, 4, 5]
        }
      });
    } else if (this.seccionActiva === 'empleados') {
      this.mostrarModalEmpleado = true;
      this.empleadoForm.reset({ tipoContrato: 'Indefinido' });
    } else if (this.seccionActiva === 'catalogos') {
      this.editandoCatalogoId = null;
      this.editandoTipoKey = null;
      this.mostrarModalCatalogo = true;
      this.catalogoForm.reset({
        tipoCatalogo: this.catalogoActivo,
        nombre: '',
        descripcion: '',
        fecha: '',
        recargo: 1.5,
        pagaPreaviso: false,
        pagaCesantia: false,
        clave: '',
        valor: ''
      });
    }
  }

 editarCatalogo(cat: any): void {
  this.editandoCatalogoId = cat.id ?? null;
  this.editandoTipoKey = cat.tipoKey ?? null;
  this.catalogoActivo = cat.tipoKey;

  const raw = cat.raw || {};

    this.catalogoForm.patchValue({
      tipoCatalogo: cat.tipoKey || 'departamentos',
      nombre: raw.nombre ?? raw.Nombre ?? cat.nombre ?? '',
      descripcion:
        raw.descripcion ??
        raw.Descripcion ??
        (cat.descripcion && cat.descripcion !== '—' ? cat.descripcion : ''),
      fecha: raw.fecha ?? raw.Fecha ?? '',
      recargo: raw.recargo ?? raw.Recargo ?? 1.5,
      pagaPreaviso: Boolean(raw.pagaPreaviso ?? raw.PagaPreaviso),
      pagaCesantia: Boolean(raw.pagaCesantia ?? raw.PagaCesantia),
      clave: raw.clave ?? raw.Clave ?? cat.id ?? '',
      valor: raw.valor ?? raw.Valor ?? ''
    });

    this.mostrarModalCatalogo = true;
  }

  cerrarModal(): void {
    this.mostrarModalUsuario = false;
    this.mostrarModalEmpleado = false;
    this.mostrarModalCatalogo = false;
    this.editandoCatalogoId = null;
    this.editandoTipoKey = null;
  }

  private extraerLista(data: any): any[] {
    let lista = data;
    if (data && Array.isArray(data.$values)) {
      lista = data.$values;
    } else if (data && Array.isArray(data.resultado)) {
      lista = data.resultado;
    }
    return Array.isArray(lista) ? lista : [];
  }

  cargarDepartamentos(): void {
    this.catalogoService.obtenerDepartamentos().subscribe({
      next: (data: any) => {
        this.departamentos = this.extraerLista(data).map((d: any) => ({
          id: d.id || d.Id,
          nombre: d.nombre || d.Nombre,
          descripcion: d.descripcion || d.Descripcion || ''
        }));
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Error al cargar departamentos:', err)
    });
  }

  cargarUsuarios(): void {
    this.usuarioService.obtenerTodos().subscribe({
      next: (data: any) => {
        this.usuarios = this.extraerLista(data).map((u: any) => ({
          id: u.id || u.Id,
          identificacion: u.identificacion || u.Identificacion,
          nombre: u.nombre || u.Nombre,
          correo: u.correo || u.Correo,
          rol: u.rol || u.Rol,
          estado: u.estado || u.Estado
        }));

        console.log('Usuarios procesados para la tabla:', this.usuarios);
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error al cargar usuarios:', err);
      }
    });
  }

  cargarEmpleados(): void {
    this.empleados = [];
  }

  cargarJefaturas(): void {
  this.usuarioService.obtenerJefaturasDisponibles().subscribe({
    next: (data: any) => {
      this.jefaturas = this.extraerLista(data);
      this.cdr.detectChanges();
      console.log('Jefaturas disponibles:', this.jefaturas);
    },
    error: (err) => {
      console.error('Error al cargar jefaturas:', err);
      this.jefaturas = [];
    }
  });
}

get jefaturasFiltradas(): UsuarioRespuesta[] {
  const rol = this.usuarioForm.get('rol')?.value;
  const departamentoId = this.usuarioForm.get('departamentoId')?.value;

  if (rol === 'Empleado') {
    return this.jefaturas.filter(
      j => j.departamentoId === departamentoId
    );
  }

  return this.jefaturas;
}

  cargarCatalogos(): void {
    this.catalogoService.obtenerTodosLosCatalogos().subscribe({
      next: (res) => {
        const deptos = this.extraerLista(res.departamentos).map((d: any) => ({
          id: d.id || d.Id,
          tipoKey: 'departamentos' as TipoCatalogoApi,
          tipoCatalogo: 'Departamento',
          nombre: d.nombre || d.Nombre,
          descripcion: d.descripcion || d.Descripcion || '—',
          raw: d
        }));

        this.departamentos = deptos.map(({ id, nombre, descripcion }) => ({
          id,
          nombre,
          descripcion
        }));

        const feriados = this.extraerLista(res.feriados).map((f: any) => ({
          id: f.id || f.Id,
          tipoKey: 'feriados' as TipoCatalogoApi,
          tipoCatalogo: 'Feriado',
          nombre: f.nombre || f.Nombre,
          descripcion: `Fecha: ${f.fecha || f.Fecha}`,
          raw: f
        }));

        const motivos = this.extraerLista(res.motivosLiquidacion).map((m: any) => ({
          id: m.id || m.Id,
          tipoKey: 'motivos-liquidacion' as TipoCatalogoApi,
          tipoCatalogo: 'Motivo de liquidación',
          nombre: m.nombre || m.Nombre,
          descripcion: `${m.descripcion || m.Descripcion || 'Sin descripción'} | Preaviso: ${(m.pagaPreaviso ?? m.PagaPreaviso) ? 'Sí' : 'No'}, Cesantía: ${(m.pagaCesantia ?? m.PagaCesantia) ? 'Sí' : 'No'}`,
          raw: m
        }));

        const horasExtra = this.extraerLista(res.tiposHoraExtra).map((h: any) => ({
          id: h.id || h.Id,
          tipoKey: 'tipos-hora-extra' as TipoCatalogoApi,
          tipoCatalogo: 'Tipo de hora extra',
          nombre: h.nombre || h.Nombre,
          descripcion: `${h.descripcion || h.Descripcion || 'Sin descripción'} | Recargo: ${h.recargo ?? h.Recargo}`,
          raw: h
        }));

        const permisos = this.extraerLista(res.tiposPermiso).map((p: any) => ({
          id: p.id || p.Id,
          tipoKey: 'tipos-permiso' as TipoCatalogoApi,
          tipoCatalogo: 'Tipo de permiso',
          nombre: p.nombre || p.Nombre,
          descripcion: p.descripcion || p.Descripcion || '—',
          raw: p
        }));

        const parametros = this.extraerLista(res.parametros).map((pa: any) => ({
          id: pa.clave || pa.Clave,
          tipoKey: 'parametros' as TipoCatalogoApi,
          tipoCatalogo: 'Parámetro',
          nombre: `${pa.clave || pa.Clave} (${pa.valor ?? pa.Valor})`,
          descripcion: pa.descripcion || pa.Descripcion || '—',
          raw: pa
        }));

        this.catalogos = [
          ...deptos,
          ...feriados,
          ...motivos,
          ...horasExtra,
          ...permisos,
          ...parametros
        ];

        this.cdr.detectChanges();
      },
      error: (err) => console.error('Error al cargar catálogos:', err)
    });
  }

  guardarUsuario(): void {
    if (this.usuarioForm.invalid) {
    this.usuarioForm.markAllAsTouched();

      const nombresCampos: { [key: string]: string } = {
        identificacion: 'Identificación',
        nombre: 'Nombre',
        correo: 'Correo electrónico',
        rol: 'Rol',
        departamentoId: 'Departamento',
        puesto: 'Puesto',
        jefaturaDirectaId: 'Jefatura directa',
        fechaIngreso: 'Fecha de ingreso',
        salarioBase: 'Salario base',
        password: 'Contraseña',
        tipoJornada: 'Tipo de jornada'
      };

      const camposInvalidos = Object.keys(this.usuarioForm.controls)
        .filter(campo => this.usuarioForm.get(campo)?.invalid)
        .map(campo => nombresCampos[campo] || campo);

      alert('Revisa los siguientes campos: ' + camposInvalidos.join(', '));

    return;
  }

    const raw = this.usuarioForm.value;
    const payload = {
      ...raw,
      jefaturaDirectaId: raw.jefaturaDirectaId?.toString().trim()
        ? raw.jefaturaDirectaId.toString().trim()
        : null,
      fotoUrl: raw.fotoUrl?.toString().trim() ? raw.fotoUrl.toString().trim() : null,
      salarioBase: Number(raw.salarioBase)
    };

    this.usuarioService.crear(payload).subscribe({
      next: () => {
        alert('¡Usuario creado con éxito!');
        this.cerrarModal();
        this.cargarUsuarios();
        this.cargarJefaturas();
      },
      error: (err) => {
        console.error('Error al crear usuario:', err);
        alert(err.error?.mensaje || 'Error al guardar el usuario');
      }
    });
  }

  guardarEmpleado(): void {
    if (this.empleadoForm.invalid) {
      this.empleadoForm.markAllAsTouched();
      return;
    }
    alert('¡Empleado guardado con éxito!');
    this.cerrarModal();
  }

  guardarCatalogo(): void {
    const formVal = this.catalogoForm.value;
    const tipo: TipoCatalogoApi = this.editandoTipoKey || formVal.tipoCatalogo;

    if (tipo === 'parametros') {
      const clave = (this.editandoCatalogoId || formVal.clave || '').trim().toLowerCase();
      if (!clave || !formVal.valor?.toString().trim()) {
        alert('La clave y el valor son obligatorios para parámetros.');
        return;
      }

      this.catalogoService
        .guardarParametro(clave, {
          valor: String(formVal.valor).trim(),
          descripcion: formVal.descripcion?.trim() || null
        })
        .subscribe({
          next: () => {
            alert('¡Parámetro guardado con éxito!');
            this.cerrarModal();
            this.cargarCatalogos();
          },
          error: (err) => alert(err.error?.mensaje || 'Error al guardar parámetro')
        });
      return;
    }

    let payload: any;
    switch (tipo) {
      case 'feriados':
        if (!formVal.fecha || !formVal.nombre?.trim()) {
          alert('La fecha y el nombre son obligatorios para feriados.');
          return;
        }
        payload = {
          fecha: formVal.fecha,
          nombre: formVal.nombre.trim()
        };
        break;

      case 'motivos-liquidacion':
        if (!formVal.nombre?.trim()) {
          alert('El nombre es obligatorio.');
          return;
        }
        payload = {
          nombre: formVal.nombre.trim(),
          descripcion: formVal.descripcion?.trim() || null,
          pagaPreaviso: Boolean(formVal.pagaPreaviso),
          pagaCesantia: Boolean(formVal.pagaCesantia)
        };
        break;

      case 'tipos-hora-extra':
        if (!formVal.nombre?.trim()) {
          alert('El nombre es obligatorio.');
          return;
        }
        payload = {
          nombre: formVal.nombre.trim(),
          descripcion: formVal.descripcion?.trim() || null,
          recargo: Number(formVal.recargo)
        };
        break;

      case 'departamentos':
      case 'tipos-permiso':
      default:
        if (!formVal.nombre?.trim()) {
          alert('El nombre es obligatorio.');
          return;
        }
        payload = {
          nombre: formVal.nombre.trim(),
          descripcion: formVal.descripcion?.trim() || null
        };
        break;
    }

    const peticion = this.editandoCatalogoId
      ? this.catalogoService.actualizar(
          tipo as
            | 'departamentos'
            | 'feriados'
            | 'motivos-liquidacion'
            | 'tipos-hora-extra'
            | 'tipos-permiso',
          this.editandoCatalogoId,
          payload
        )
      : this.catalogoService.crear(tipo, payload);

    peticion.subscribe({
      next: () => {
        alert(
          this.editandoCatalogoId
            ? '¡Registro actualizado con éxito!'
            : '¡Registro creado con éxito!'
        );
        this.cerrarModal();
        this.cargarCatalogos();
        if (tipo === 'departamentos') {
          this.cargarDepartamentos();
        }
      },
      error: (err) => alert(err.error?.mensaje || 'Error al guardar el registro')
    });
  }

  inactivarUsuario(id: string): void {
    if (confirm('¿Desea inactivar este registro?')) {
      this.usuarioService.inactivar(id).subscribe({
        next: () => this.cargarUsuarios(),
        error: (err) => alert(err.error?.mensaje || 'Error al inactivar')
      });
    }
  }
}