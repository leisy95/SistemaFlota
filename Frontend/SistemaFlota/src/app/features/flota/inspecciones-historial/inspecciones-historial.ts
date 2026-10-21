import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import * as XLSX from 'xlsx';
import { PermisosService } from '../../../core/services/permisos.service';
import { InspeccionesService } from '../../../core/services/inspecciones.service';

@Component({
  selector: 'app-inspecciones-historial',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './inspecciones-historial.html',
  styleUrls: ['./inspecciones-historial.scss']
})
export class InspeccionesHistorialComponent implements OnInit {

  //  Propiedades — reemplaza las existentes
  historial: any[] = [];
  historialFiltrado: any[] = [];
  historialPaginado: any[] = [];
  detalleSeleccionado: any = null;
  paginaActual = 1;
  itemsPorPagina = 10;
  totalRegistros = 0;
  totalPaginas_ = 0;

  urlBase = 'https://api.gecobagsci.com';

  filtroBusqueda = '';
  filtroFechaDesde = '';
  filtroFechaHasta = '';

  get puedeVer(): boolean {
    return this.permisosService.puedeVer('inspecciones');
  }

  constructor(
    private inspeccionesService: InspeccionesService,
    private permisosService: PermisosService
  ) { }

  ngOnInit(): void {
    this.obtenerHistorial();
  }


  obtenerHistorial(): void {
    this.inspeccionesService.obtenerHistorial({
      pagina: this.paginaActual,
      porPagina: this.itemsPorPagina,
      buscar: this.filtroBusqueda.trim() || undefined,
      fechaDesde: this.filtroFechaDesde || undefined,
      fechaHasta: this.filtroFechaHasta || undefined
    }).subscribe({
      next: (res: any) => {
        this.historial = res.data ?? [];
        this.historialFiltrado = this.historial;
        this.historialPaginado = this.historial;

        this.totalRegistros = res.total ?? 0;
        this.totalPaginas_ = res.totalPaginas ?? 1;
      },
      error: (err: any) => {
        console.error('Error obteniendo historial:', err);
      }
    });
  }


  aplicarFiltros(): void {
    this.paginaActual = 1;
    this.obtenerHistorial();
  }

  limpiarFiltros(): void {
    this.filtroBusqueda = '';
    this.filtroFechaDesde = '';
    this.filtroFechaHasta = '';
    this.paginaActual = 1;
    this.obtenerHistorial();
  }

  private actualizarPaginado() {
    const inicio = (this.paginaActual - 1) * this.itemsPorPagina;
    this.historialPaginado = this.historialFiltrado.slice(inicio, inicio + this.itemsPorPagina);
  }

  get totalPaginas(): number { return this.totalPaginas_; }

  paginaSiguiente() {
    if (this.paginaActual < this.totalPaginas) {
      this.paginaActual++;
      this.obtenerHistorial();
    }
  }

  paginaAnterior() {
    if (this.paginaActual > 1) {
      this.paginaActual--;
      this.obtenerHistorial();
    }
  }

  verDetalle(id: number) {
    this.inspeccionesService.obtenerDetalle(id).subscribe({
      next: (data: any) => this.detalleSeleccionado = data,
      error: (err: any) => { console.error(err); alert('Error cargando detalle'); }
    });
  }

  private cargarImagen(src: string): Promise<HTMLImageElement | null> {
    return new Promise(resolve => {
      const img = new Image();
      img.src = src;
      img.onload = () => resolve(img);
      img.onerror = () => resolve(null);
    });
  }


  descargarPDF(inspeccion: any): void {
    this.inspeccionesService.descargarPdf(inspeccion.id).subscribe({
      next: (pdf: Blob) => {
        const url = window.URL.createObjectURL(pdf);
        const enlace = document.createElement('a');

        enlace.href = url;
        enlace.download = `inspeccion-${inspeccion.id}.pdf`;
        enlace.click();

        window.URL.revokeObjectURL(url);
      },
      error: (err: any) => {
        console.error('Error descargando el PDF:', err);
        alert('No fue posible generar o descargar el PDF de la inspección.');
      }
    });
  }

  exportandoPdf = false;

  exportarPdfGeneral(): void {
    if (this.exportandoPdf || this.historialFiltrado.length === 0) {
      return;
    }

    this.exportandoPdf = true;

    this.inspeccionesService.exportarPdfGeneral({
      buscar: this.filtroBusqueda,
      fechaDesde: this.filtroFechaDesde,
      fechaHasta: this.filtroFechaHasta
    }).subscribe({
      next: (pdf: Blob) => {
        const url = window.URL.createObjectURL(pdf);
        const enlace = document.createElement('a');

        enlace.href = url;
        enlace.download = 'historial-inspecciones.pdf';
        enlace.click();

        window.URL.revokeObjectURL(url);
        this.exportandoPdf = false;
      },
      error: (err: any) => {
        console.error('Error generando PDF general:', err);
        alert('No fue posible generar el PDF general.');
        this.exportandoPdf = false;
      }
    });
  }

  exportarExcel() {
    const datos = this.historialFiltrado.map(i => ({
      'ID': i.id,
      'Fecha': new Date(i.fecha).toLocaleString(),
      'Conductor': i.conductor?.nombre ?? '-',
      'Vehículo': i.vehiculo?.placa ?? '-',
      'Kilometraje': `${i.kilometraje} km`,
    }));
    const hoja = XLSX.utils.json_to_sheet(datos);
    hoja['!cols'] = [{ wch: 8 }, { wch: 20 }, { wch: 25 }, { wch: 15 }, { wch: 15 }];
    const libro = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(libro, hoja, 'Inspecciones');
    XLSX.writeFile(libro, `inspecciones_${new Date().toISOString().slice(0, 10)}.xlsx`);
  }
}