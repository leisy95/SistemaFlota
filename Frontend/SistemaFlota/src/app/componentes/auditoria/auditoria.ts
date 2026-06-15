import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuditoriaService } from '../../services/auditoria.service';
import { PermisosService }  from '../../services/permisos.service';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-auditoria',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './auditoria.html',
  styleUrls: ['./auditoria.scss']
})
export class AuditoriaComponent implements OnInit {

  registros:     any[] = [];
  estadisticas:  any   = null;
  cargando       = false;

  paginaActual   = 1;
  porPagina      = 10;
  totalRegistros = 0;
  totalPaginas   = 0;

  filtros = {
    usuario: '', modulo: '', accion: '',
    resultado: '', fechaDesde: '', fechaHasta: ''
  };

  readonly modulos = [
    'Auth','Inspecciones','Autorizaciones','Usuarios',
    'Documentos','Mantenimiento','Incidentes','Configuracion'
  ];

  readonly acciones = [
    'Login','Logout','Crear','Editar','Eliminar',
    'Firmar','Revisar','Subir','Finalizar',
    'RecuperarPassword','CambiarPassword','Rechazar'
  ];

  readonly resultados = ['Exitoso', 'Fallido'];

  // ── Permisos ─────────────────────────────────────────────────────────────────
  get puedeVer():      boolean { return this.permisosService.puedeVer('auditoria'); }
  get puedeEliminar(): boolean { return this.permisosService.puedeEliminar('auditoria'); }

  constructor(
    private auditoriaService: AuditoriaService,
    private permisosService:  PermisosService
  ) {}

  ngOnInit(): void {
    this.cargarEstadisticas();
    this.buscar();
  }

  cargarEstadisticas() {
    this.auditoriaService.obtenerEstadisticas().subscribe({
      next: (data) => this.estadisticas = data,
      error: (err)  => console.error(err)
    });
  }

  buscar() {
    this.cargando = true;
    this.auditoriaService.obtenerAuditorias({
      ...this.filtros, pagina: this.paginaActual, porPagina: this.porPagina
    }).subscribe({
      next: (data: any) => {
        this.registros      = data.datos;
        this.totalRegistros = data.total;
        this.totalPaginas   = data.totalPaginas;
        this.cargando       = false;
      },
      error: (err) => { console.error(err); this.cargando = false; }
    });
  }

  limpiarFiltros() {
    this.filtros = { usuario: '', modulo: '', accion: '', resultado: '', fechaDesde: '', fechaHasta: '' };
    this.paginaActual = 1;
    this.buscar();
  }

  paginaAnterior() {
    if (this.paginaActual > 1) { this.paginaActual--; this.buscar(); }
  }

  paginaSiguiente() {
    if (this.paginaActual < this.totalPaginas) { this.paginaActual++; this.buscar(); }
  }

  getBadgeAccion(accion: string): string {
    switch (accion) {
      case 'Login':    return 'badge-login';
      case 'Logout':   return 'badge-logout';
      case 'Crear':    return 'badge-crear';
      case 'Editar':   return 'badge-editar';
      case 'Eliminar': return 'badge-eliminar';
      case 'Firmar':   return 'badge-firmar';
      case 'Revisar':  return 'badge-revisar';
      case 'Subir':    return 'badge-subir';
      case 'Finalizar':return 'badge-finalizar';
      default:         return 'badge-otro';
    }
  }

  getBadgeResultado(resultado: string): string {
    return resultado === 'Exitoso' ? 'badge-exitoso' : 'badge-fallido';
  }

  exportarExcel() {
    const datos = this.registros.map(r => ({
      'ID':          r.id,
      'Fecha':       new Date(r.fecha).toLocaleString(),
      'Usuario':     r.usuario,
      'Rol':         r.rol,
      'Módulo':      r.modulo,
      'Acción':      r.accion,
      'Detalle':     r.detalle    || '-',
      'Registro ID': r.registroId || '-',
      'IP':          r.ipAddress  || '-',
      'Resultado':   r.resultado
    }));
    const hoja = XLSX.utils.json_to_sheet(datos);
    hoja['!cols'] = [
      {wch:6},{wch:20},{wch:20},{wch:15},{wch:18},
      {wch:15},{wch:50},{wch:12},{wch:15},{wch:10}
    ];
    const libro = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(libro, hoja, 'Auditoría');
    XLSX.writeFile(libro, `auditoria_${new Date().toISOString().slice(0,10)}.xlsx`);
  }

  limpiarRegistros() {
    if (!confirm('¿Eliminar registros de auditoría con más de 90 días?')) return;
    this.auditoriaService.limpiarRegistros(90).subscribe({
      next: (data: any) => {
        alert(`✅ Se eliminaron ${data.eliminados} registros antiguos`);
        this.buscar();
        this.cargarEstadisticas();
      },
      error: (err) => console.error(err)
    });
  }
}