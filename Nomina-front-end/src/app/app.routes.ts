import { Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { RolGuard } from './core/guards/rol.guard';
import { LoginComponent } from './auth/login/login.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    canActivate: [AuthGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { 
        path: 'dashboard', 
        loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent) 
      },
      { 
        path: 'gestion-general', 
        loadComponent: () => import('./pages/gestion-general/gestion-general.component').then(m => m.GestionGeneralComponent), 
        canActivate: [RolGuard], 
        data: { roles: ['RRHH'] } 
      },
      { 
        path: 'asistencia', 
        loadComponent: () => import('./pages/asistencia/asistencia.component').then(m => m.AsistenciaComponent), 
        canActivate: [RolGuard], 
        data: { roles: ['Empleado', 'Jefatura', 'RRHH'] } 
      },
      { 
        path: 'permisos', 
        loadComponent: () => import('./pages/permisos/permisos.component').then(m => m.PermisosComponent), 
        canActivate: [RolGuard], 
        data: { roles: ['Empleado', 'Jefatura', 'RRHH'] } 
      },
      { 
        path: 'horas-extras', 
        loadComponent: () => import('./pages/horas-extras/horas-extras.component').then(m => m.HorasExtrasComponent), 
        canActivate: [RolGuard], 
        data: { roles: ['Empleado', 'Jefatura', 'RRHH'] } 
      },
      { 
        path: 'liquidacion', 
        loadComponent: () => import('./pages/liquidacion/liquidacion.component').then(m => m.LiquidacionComponent), 
        canActivate: [RolGuard], 
        data: { roles: ['RRHH'] } 
      },
      { 
        path: 'planilla', 
        loadComponent: () => import('./pages/planilla/planilla.component').then(m => m.PlanillaComponent), 
        canActivate: [RolGuard], 
        data: { roles: ['RRHH'] } 
      },
      { path: '**', redirectTo: 'dashboard' }
    ]
  }
];