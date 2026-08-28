import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';

export interface PermisoUsuario {
  modulo: string;
  puedeVer: boolean;
  puedeCrear: boolean;
  puedeEditar: boolean;
  puedeEliminar: boolean;
  puedeEnviar: boolean;
  esInicio: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class PermisosService {
  private permisos: PermisoUsuario[] = [];
  private rol = '';

  constructor(private auth: AuthService) {
    const usuario = this.auth.obtenerUsuarioActual();
    if (usuario) this.cargar(usuario);
  }

  cargar(usuario: any): void {
    this.rol = usuario?.rol ?? '';
    this.permisos = usuario?.permisos ?? [];
  }

  puedeVer(modulo: string): boolean {
    if (this.rol === 'Admin') return true;
    const permiso = this.permisos.find(p => p.modulo === modulo);
    return permiso?.puedeVer === true;
  }

  puedeCrear(modulo: string): boolean {
    if (this.rol === 'Admin') return true;
    const permiso = this.permisos.find(p => p.modulo === modulo);
    return permiso?.puedeCrear === true;
  }

  puedeEditar(modulo: string): boolean {
    if (this.rol === 'Admin') return true;
    const permiso = this.permisos.find(p => p.modulo === modulo);
    return permiso?.puedeEditar === true;
  }

  puedeEliminar(modulo: string): boolean {
    if (this.rol === 'Admin') return true;
    const permiso = this.permisos.find(p => p.modulo === modulo);
    return permiso?.puedeEliminar === true;
  }

  puedeEnviar(modulo: string): boolean {
    if (this.rol === 'Admin') return true
    const permiso = this.permisos.find(p => p.modulo === modulo);
    return permiso?.puedeEnviar === true;
  }

  obtenerPermisos(): PermisoUsuario[] {
    return this.permisos;
  }

  limpiar(): void {
    this.permisos = [];
    this.rol = '';
  }
}