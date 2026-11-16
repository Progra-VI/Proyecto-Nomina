export interface UsuarioLogin {
  id: string;
  nombre: string;
  email: string;
  rol: 'Empleado' | 'Jefatura' | 'RRHH';
  departamento: string;
  token?: string;
}