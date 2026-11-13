import { CanActivateFn, Router, ActivatedRouteSnapshot } from '@angular/router';
import { inject } from '@angular/core';

export const RolGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const router = inject(Router);
  const usuario = JSON.parse(localStorage.getItem('usuario') || '{}');
  const rolesPermitidos = route.data?.['roles'] as string[];

  if (!usuario.rol || !rolesPermitidos.includes(usuario.rol)) {
    router.navigate(['/dashboard']);
    return false;
  }
  return true;
};
