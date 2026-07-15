import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class PermisosService {

  private permisos: any[] = [];
  private rol = '';

  constructor(private auth: AuthService) {

    const usuario = this.auth.obtenerUsuarioActual();

    if (usuario) {
      this.cargar(usuario);
    }
  }

  cargar(usuario: any) {

    this.rol = usuario.rol ?? '';
    this.permisos = usuario.permisos ?? [];
  }

  // ¿Puede ver el módulo?
  puedeVer(modulo: string): boolean {
    console.log('Rol:', this.rol);
    console.log('Permisos:', this.permisos);
    console.log('Módulo:', modulo);

    if (this.rol === 'Admin') {
      return true;
    }

    const p = this.permisos.find(p => p.modulo === modulo);
    console.log('Permiso encontrado:', p);
    if (!p) return false;
    return p.puedeVer ?? true;
  }

  // ¿Puede crear en el módulo?
  puedeCrear(modulo: string): boolean {
    if (this.rol === 'Admin') return true;
    const p = this.permisos.find(p => p.modulo === modulo);
    ;
    if (!p) return true;
    return p.puedeCrear ?? false;
  }

  // ¿Puede editar en el módulo?
  puedeEditar(modulo: string): boolean {
    if (this.rol === 'Admin') return true;
    const p = this.permisos.find(p => p.modulo === modulo);
    if (!p) return true;
    return p.puedeEditar ?? false;
  }

  // ¿Puede eliminar en el módulo?
  puedeEliminar(modulo: string): boolean {
    if (this.rol === 'Admin') return true;
    const p = this.permisos.find(p => p.modulo === modulo);
    if (!p) return true;
    return p.puedeEliminar ?? false;
  }
}