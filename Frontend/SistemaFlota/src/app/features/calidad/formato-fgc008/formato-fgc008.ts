import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PermisosService } from '../../../core/services/permisos.service';
import { OrdenesProduccionService } from '../../../core/services/ordenes-produccion.service';
import { FormatoFGC008Service } from '../../../core/services/calidad/formato-fgc008.service';


@Component({
  selector: 'app-formato-fgc008',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './formato-fgc008.html',
  styleUrls: ['./formato-fgc008.scss']
})
export class FormatoFGC008Component implements OnInit {
  vista: 'lista' | 'nuevo' | 'ver' | 'editar' = 'lista';
  registroSeleccionado: any = null;
  editandoId: number | null = null;
  editandoEsBodega = false;
  registros: any[] = [];
  baseUrl = environment.fotosUrl;
  filtroDesde = '';
  filtroHasta = '';
  filtroOP = '';
  filtroOPSincronizar = '';
  sincronizando = false;
  guardando = false;
  generandoPdf = false;

  // Fotos (máximo 5 por registro, contando la antigua si existe)
  fotosPendientes: { file: File; preview: string }[] = [];
  fotosActuales: { id?: number; archivo: string; antigua: boolean }[] = [];
  readonly maxFotos = 5;

  get totalFotos(): number { return this.fotosActuales.length + this.fotosPendientes.length; }
  get cupoFotos(): number { return Math.max(0, this.maxFotos - this.totalFotos); }

  // Paginación y resumen (los calcula el servidor)
  pagina = 1;
  porPagina = 25;
  totalFiltro = 0;
  totalPaginas = 1;
  resumen = { total: 0, conDefectos: 0, pendientesDespacho: 0, porcentajeCompletado: 0 };

  fotoAmpliada: string | null = null;
  mostrarModalImportar = false;
  form = {
    ordenProduccion: '', cliente: '', referencia: '', etiquetasSI: false, embalajeSI: false,
    defectosSI: false, cantidadOP: 0, cantidadReal: 0,
    listoBodega: false, despachado: '', accionesTomadas: ''
  };

  archivoImportar: File | null = null;
  importando = false;
  mensajeImportar = '';

  firmaDataUrl: string | null = null;
  firmaExistente: string | null = null;
  cargoFirma: string = 'Bodega';
  logoUrl: string = '';
  opExistente: any = null;
  clienteDetectado: string | null = null;
  referenciaDetectada: string | null = null;
  despachado: boolean = false;

  get puedeVer(): boolean { return this.permisosService.puedeVer('calidad-formatos'); }
  get puedeCrear(): boolean { return this.permisosService.puedeCrear('calidad-formatos'); }
  get puedeEliminar(): boolean { return this.permisosService.puedeEliminar('calidad-formatos'); }
  get usuario(): string {
    const u = JSON.parse(sessionStorage.getItem('user') || '{}');
    return u.username ?? '';
  }

  constructor(
    private service: FormatoFGC008Service,
    private permisosService: PermisosService,
    private ordenesService: OrdenesProduccionService
  ) { }

  ngOnInit(): void { this.cargar(); this.cargarLogo(); }

  // ── Importar órdenes ──
  seleccionarArchivoImportar(event: any) {
    this.archivoImportar = event.target.files[0];
  }

  importarOrdenes() {
    if (!this.archivoImportar) { alert('Seleccione un archivo primero'); return; }
    this.importando = true;
    this.mensajeImportar = '';
    this.ordenesService.importar(this.archivoImportar).subscribe({
      next: (res: any) => {
        this.mensajeImportar = res.mensaje;
        this.importando = false;
        this.archivoImportar = null;
      },
      error: (err: any) => {
        this.mensajeImportar = 'Error: ' + (err.error?.mensaje ?? 'no se pudo importar');
        this.importando = false;
      }
    });
  }

  // ── Carga ──
  cargar(volverAPrimera = false) {
    if (volverAPrimera) this.pagina = 1;

    this.service.getPaginado(this.filtroDesde, this.filtroHasta, this.filtroOP, this.pagina, this.porPagina).subscribe({
      next: (d: any) => {
        this.registros = d.items;
        this.totalFiltro = d.total;
        this.totalPaginas = Math.max(1, d.totalPaginas);
        // Si la página quedó vacía (por ejemplo, al borrar la última fila), vuelve a la última que existe
        if (this.registros.length === 0 && this.pagina > 1) {
          this.pagina = this.totalPaginas;
          this.cargar();
        }
      },
      error: e => {
        console.error(e);
        this.registros = [];
        alert('No se pudo cargar la lista: ' + (e.error?.error || e.error?.mensaje || e.statusText || 'error del servidor'));
      }
    });

    this.service.getResumen(this.filtroDesde, this.filtroHasta, this.filtroOP).subscribe({
      next: (r: any) => this.resumen = r,
      error: e => console.error(e)
    });
  }

  irAPagina(p: number) {
    if (p < 1 || p > this.totalPaginas || p === this.pagina) return;
    this.pagina = p;
    this.cargar();
  }

  cargarLogo() {
    const config = JSON.parse(sessionStorage.getItem('configuracion') || '{}');
    if (config.logo) this.logoUrl = this.baseUrl + '/config/' + config.logo;
  }

  // ── Ayudas para la tabla ──
  esUnidades(r: any): boolean {
    return (r.unidadReal || '').toUpperCase() === 'UN';
  }

  puedeEliminarFila(r: any): boolean {
    return this.puedeEliminar && r.origen !== 'Bodega';
  }

  estadoRevision(r: any): 'proceso' | 'cambio' | 'revisada' | 'sin' {
    if (r.enProceso) return 'proceso';
    if (r.cambioDespuesDeRevisar) return 'cambio';
    if (r.revisadoPor && !String(r.revisadoPor).startsWith('Sistema')) return 'revisada';
    return 'sin';
  }

  diferenciaPct(r: any): number | null {
    if (!r.cantidadOP || r.cantidadOP <= 0) return null;
    return Math.round(((r.cantidadReal - r.cantidadOP) / r.cantidadOP) * 1000) / 10;
  }

  // Todas las fotos de una fila: la antigua (si existe) y las nuevas
  fotosDe(r: any): string[] {
    const lista: string[] = [];
    if (r.fotoEvidencia) lista.push(r.fotoEvidencia);
    (r.fotosEvidencia || []).forEach((x: any) => lista.push(x.archivo));
    return lista;
  }

  // ── Firma ──
  iniciarCanvas() {
    setTimeout(() => {
      const canvas = document.getElementById('firmaCanvas') as HTMLCanvasElement;
      if (!canvas) return;
      const ctx = canvas.getContext('2d')!;
      let dibujando = false;
      canvas.addEventListener('mousedown', e => { dibujando = true; ctx.beginPath(); ctx.moveTo(e.offsetX, e.offsetY); });
      canvas.addEventListener('mousemove', e => { if (!dibujando) return; ctx.lineTo(e.offsetX, e.offsetY); ctx.stroke(); });
      canvas.addEventListener('mouseup', () => { dibujando = false; this.firmaDataUrl = canvas.toDataURL(); });
      canvas.addEventListener('touchstart', e => { e.preventDefault(); dibujando = true; const t = e.touches[0]; const r = canvas.getBoundingClientRect(); ctx.beginPath(); ctx.moveTo(t.clientX - r.left, t.clientY - r.top); });
      canvas.addEventListener('touchmove', e => { e.preventDefault(); if (!dibujando) return; const t = e.touches[0]; const r = canvas.getBoundingClientRect(); ctx.lineTo(t.clientX - r.left, t.clientY - r.top); ctx.stroke(); });
      canvas.addEventListener('touchend', () => { dibujando = false; this.firmaDataUrl = (document.getElementById('firmaCanvas') as HTMLCanvasElement).toDataURL(); });
    }, 300);
  }

  limpiarFirma() {
    const canvas = document.getElementById('firmaCanvas') as HTMLCanvasElement;
    if (canvas) canvas.getContext('2d')!.clearRect(0, 0, canvas.width, canvas.height);
    this.firmaDataUrl = this.firmaExistente;
  }

  // ── Sincronizar ──
  sincronizarConProduccion() {
    if (!this.filtroOPSincronizar) { alert('Ingrese el número de orden'); return; }
    this.sincronizando = true;
    this.service.sincronizar(this.filtroOPSincronizar).subscribe({
      next: (res: any) => {
        this.sincronizando = false;
        if (res?.resultado?.entradasLeidas === 0 && res?.mensaje) alert(res.mensaje);
        this.filtroOPSincronizar = '';
        this.cargar();
      },
      error: (e) => {
        this.sincronizando = false;
        alert(e.error?.mensaje || 'No se pudo sincronizar esa orden');
      }
    });
  }

  buscarOP() {
    if (!this.form.ordenProduccion) return;
    this.service.buscarOP(this.form.ordenProduccion).subscribe({
      next: (data) => { this.opExistente = data; this.form.cantidadOP = data.cantidadOP; },
      error: () => {
        this.opExistente = null;
        // No está en el historial propio, buscamos en el catálogo importado
        this.ordenesService.buscar(this.form.ordenProduccion).subscribe({
          next: (data: any) => {
            this.opExistente = { cantidadOP: data.cantidadOP, totalEntradas: 0, totalReal: 0 };
            this.form.cantidadOP = data.cantidadOP;
            this.form.cliente = data.cliente;
            this.form.referencia = data.referencia;
            this.clienteDetectado = data.cliente;
            this.referenciaDetectada = data.referencia;
          },
          error: () => { this.clienteDetectado = null; this.referenciaDetectada = null; }
        });
      }
    });
  }

  // ── Fotos ──
  // Reduce la foto a máximo 1600 px y calidad 80 %: pesa unos 200–400 KB en vez de varios MB
  private comprimirImagen(file: File, maxLado = 1600, calidad = 0.8): Promise<File> {
    return new Promise((resolve) => {
      const reader = new FileReader();
      reader.onerror = () => resolve(file);
      reader.onload = () => {
        const img = new Image();
        img.onerror = () => resolve(file);
        img.onload = () => {
          const escala = Math.min(1, maxLado / Math.max(img.width, img.height));
          const w = Math.round(img.width * escala);
          const h = Math.round(img.height * escala);
          const canvas = document.createElement('canvas');
          canvas.width = w;
          canvas.height = h;
          const ctx = canvas.getContext('2d');
          if (!ctx) { resolve(file); return; }
          ctx.drawImage(img, 0, 0, w, h);
          canvas.toBlob(blob => {
            if (!blob) { resolve(file); return; }
            const nombre = file.name.replace(/\.[^.]+$/, '') + '.jpg';
            resolve(new File([blob], nombre, { type: 'image/jpeg' }));
          }, 'image/jpeg', calidad);
        };
        img.src = reader.result as string;
      };
      reader.readAsDataURL(file);
    });
  }

  async seleccionarFotos(event: any) {
    const archivos: File[] = Array.from(event.target.files || []);
    event.target.value = '';   // permite elegir la misma foto otra vez
    if (archivos.length === 0) return;

    if (archivos.length > this.cupoFotos) {
      alert('Solo puede agregar ' + this.cupoFotos + ' foto(s) más (máximo ' + this.maxFotos + ' por registro)');
    }

    for (const f of archivos.slice(0, this.cupoFotos)) {
      const comprimida = await this.comprimirImagen(f);
      this.fotosPendientes.push({ file: comprimida, preview: URL.createObjectURL(comprimida) });
    }
  }

  quitarPendiente(i: number) {
    URL.revokeObjectURL(this.fotosPendientes[i].preview);
    this.fotosPendientes.splice(i, 1);
  }

  eliminarFotoGuardada(f: { id?: number; archivo: string; antigua: boolean }) {
    if (!f.id || !this.editandoId) return;
    if (!confirm('¿Eliminar esta foto?')) return;
    this.service.eliminarFoto(this.editandoId, f.id).subscribe({
      next: () => {
        this.fotosActuales = this.fotosActuales.filter(x => x.id !== f.id);
        this.cargar();
      },
      error: e => alert(e.error?.mensaje || 'No se pudo eliminar la foto')
    });
  }

  private limpiarFotos() {
    this.fotosPendientes.forEach(p => URL.revokeObjectURL(p.preview));
    this.fotosPendientes = [];
    this.fotosActuales = [];
  }

  // Sube las fotos de a una: cada una pesa poco y no se depende del límite de tamaño de Nginx
  private async subirPendientes(id: number): Promise<boolean> {
    for (const p of this.fotosPendientes) {
      try {
        await firstValueFrom(this.service.subirFotos(id, [p.file]));
      } catch (e: any) {
        alert('El registro se guardó, pero una foto no se pudo subir: ' + (e?.error?.mensaje || 'error al subirla'));
        return false;
      }
    }
    return true;
  }

  // ── Nuevo, guardar, ver, editar ──
  nuevoRegistro() {
    this.limpiarFotos();
    this.firmaDataUrl = null;
    this.firmaExistente = null;
    this.opExistente = null;
    this.clienteDetectado = null;
    this.referenciaDetectada = null;
    this.despachado = false;
    this.vista = 'nuevo';
    this.iniciarCanvas();
  }

  guardar() {
    if (this.guardando) return;
    if (!this.form.ordenProduccion) { alert('Ingrese la orden de produccion'); return; }
    this.guardando = true;
    const fd = new FormData();
    fd.append('OrdenProduccion', this.form.ordenProduccion);
    fd.append('Cliente', this.form.cliente || '');
    fd.append('Referencia', this.form.referencia || '');
    if (this.firmaDataUrl) fd.append('FirmaDigital', this.firmaDataUrl);
    fd.append('EtiquetasSI', this.form.etiquetasSI.toString());
    fd.append('EmbalajeSI', this.form.embalajeSI.toString());
    fd.append('DefectosSI', this.form.defectosSI.toString());
    fd.append('CantidadOP', this.form.cantidadOP.toString());
    fd.append('CantidadReal', this.form.cantidadReal.toString());
    fd.append('ListoBodega', this.form.listoBodega.toString());
    if (this.form.despachado) fd.append('Despachado', this.form.despachado);
    if (this.form.accionesTomadas) fd.append('AccionesTomadas', this.form.accionesTomadas);
    this.service.crearRegistro(fd).subscribe({
      next: async (res: any) => {
        await this.subirPendientes(res.id);
        this.limpiarFotos();
        this.guardando = false;
        this.vista = 'lista';
        this.form = { ordenProduccion: '', cliente: '', referencia: '', etiquetasSI: false, embalajeSI: false, defectosSI: false, cantidadOP: 0, cantidadReal: 0, listoBodega: false, despachado: '', accionesTomadas: '' };
        this.opExistente = null; this.despachado = false;
        this.firmaDataUrl = null; this.firmaExistente = null;
        this.cargar();
      },
      error: e => {
        this.guardando = false;
        console.error(e);
        alert(e.error?.mensaje || 'Error guardando registro');
      }
    });
  }

  ver(r: any) { this.registroSeleccionado = r; this.vista = 'ver'; }

  editar(r: any) {
    this.limpiarFotos();
    this.editandoId = r.id;
    this.editandoEsBodega = r.origen === 'Bodega';
    this.firmaExistente = r.firmaDigital ?? null;
    this.firmaDataUrl = r.firmaDigital ?? null;

    const fotos: { id?: number; archivo: string; antigua: boolean }[] = [];
    if (r.fotoEvidencia) fotos.push({ archivo: r.fotoEvidencia, antigua: true });
    (r.fotosEvidencia || []).forEach((x: any) => fotos.push({ id: x.id, archivo: x.archivo, antigua: false }));
    this.fotosActuales = fotos;

    this.form = {
      ordenProduccion: r.ordenProduccion, cliente: r.cliente ?? '', referencia: r.referencia ?? '',
      etiquetasSI: r.etiquetasSI,
      embalajeSI: r.embalajeSI, defectosSI: r.defectosSI,
      cantidadOP: r.cantidadOP, cantidadReal: r.cantidadReal,
      listoBodega: r.listoBodega, despachado: r.despachado ?? '',
      accionesTomadas: r.accionesTomadas ?? ''
    };
    this.vista = 'editar';
    this.iniciarCanvas();
  }

  guardarEdicion() {
    if (this.guardando) return;
    if (!this.form.ordenProduccion) { alert('Ingrese la orden de produccion'); return; }
    this.guardando = true;
    const id = this.editandoId!;
    const fd = new FormData();
    fd.append('OrdenProduccion', this.form.ordenProduccion);
    fd.append('Cliente', this.form.cliente || '');
    fd.append('Referencia', this.form.referencia || '');
    fd.append('FirmaDigital', this.firmaDataUrl || '');
    fd.append('EtiquetasSI', this.form.etiquetasSI.toString());
    fd.append('EmbalajeSI', this.form.embalajeSI.toString());
    fd.append('DefectosSI', this.form.defectosSI.toString());
    fd.append('CantidadOP', this.form.cantidadOP.toString());
    fd.append('CantidadReal', this.form.cantidadReal.toString());
    fd.append('ListoBodega', this.form.listoBodega.toString());
    if (this.form.despachado) fd.append('Despachado', this.form.despachado);
    if (this.form.accionesTomadas) fd.append('AccionesTomadas', this.form.accionesTomadas);
    this.service.editarRegistro(id, fd).subscribe({
      next: async () => {
        await this.subirPendientes(id);
        this.limpiarFotos();
        this.guardando = false;
        this.firmaExistente = null;
        this.vista = 'lista';
        this.editandoId = null;
        this.cargar();
      },
      error: e => {
        this.guardando = false;
        console.error(e);
        alert(e.error?.mensaje || 'Error editando registro');
      }
    });
  }

  eliminar(id: number) {
    if (!confirm('Eliminar este registro?')) return;
    this.service.eliminarRegistro(id).subscribe({ next: () => this.cargar() });
  }

  // ── Lightbox de foto ampliada ──
  abrirFotoAmpliada(url: string) {
    this.fotoAmpliada = url;
  }

  cerrarFotoAmpliada() {
    this.fotoAmpliada = null;
  }

  generandoPdfBackend = false;
  exportarPDFBackend(): void {
    this.generandoPdfBackend = true;

    this.service.generarPdf(
      this.filtroDesde || undefined,
      this.filtroHasta || undefined,
      this.filtroOP?.trim() || undefined
    ).subscribe({
      next: (pdf: Blob) => {
        const url = window.URL.createObjectURL(pdf);
        const enlace = document.createElement('a');

        enlace.href = url;
        enlace.download = `F-GC-008_${new Date().getTime()}.pdf`;

        enlace.click();

        window.URL.revokeObjectURL(url);
        this.generandoPdfBackend = false;
      },
      error: (error) => {
        console.error('Error al generar PDF desde backend:', error);
        this.generandoPdfBackend = false;
      }
    });
  }
}