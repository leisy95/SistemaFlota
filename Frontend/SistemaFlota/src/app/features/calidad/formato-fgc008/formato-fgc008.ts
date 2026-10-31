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

  // ── PDF ──
  private readonly pdfCw = [27, 21, 9, 9, 9, 9, 9, 9, 18, 16, 14, 9, 9, 27];
  private readonly pdfRowH = 9;

  private pdfNum(v: any, dec = 0): string {
    return (Number(v) || 0).toLocaleString('es-CO', { minimumFractionDigits: dec, maximumFractionDigits: dec });
  }

  private pdfFecha(d: any): string {
    return d ? new Date(d).toLocaleDateString('es-CO') : '-';
  }

  private pdfDia(r: any): string {
    return this.pdfFecha(r.origen === 'Bodega' ? r.diaEntrada : r.fecha);
  }

  // 'und' o 'kg' en las filas de Bodega; vacío en las manuales, porque no se sabe la unidad
  private pdfUnidad(r: any): string {
    return r.origen === 'Bodega' ? (this.esUnidades(r) ? 'und' : 'kg') : '';
  }

  private pdfCortar(t: string | null | undefined, n: number): string {
    const s = t ?? '';
    return s.length > n ? s.slice(0, n - 1) + '…' : s;
  }

  // Descarga una imagen y la reduce para el PDF: máximo 700 px, fondo blanco, calidad 70 %
  urlToBase64(url: string, maxLado = 700, calidad = 0.7): Promise<{ data: string; w: number; h: number } | null> {
    return new Promise((resolve) => {
      const img = new Image();
      img.crossOrigin = 'anonymous';
      img.onload = () => {
        const escala = Math.min(1, maxLado / Math.max(img.width, img.height));
        const w = Math.max(1, Math.round(img.width * escala));
        const h = Math.max(1, Math.round(img.height * escala));
        const canvas = document.createElement('canvas');
        canvas.width = w;
        canvas.height = h;
        const ctx = canvas.getContext('2d');
        if (!ctx) { resolve(null); return; }
        ctx.fillStyle = '#ffffff';
        ctx.fillRect(0, 0, w, h);
        ctx.drawImage(img, 0, 0, w, h);
        resolve({ data: canvas.toDataURL('image/jpeg', calidad), w, h });
      };
      img.onerror = () => resolve(null);
      img.src = url + (url.includes('?') ? '&' : '?') + 't=' + Date.now();
    });
  }

  async exportarPDF() {
    if (this.generandoPdf) return;
    this.generandoPdf = true;
    try {
      // El PDF lleva TODAS las filas del filtro, no solo la página que se está viendo
      const registros: any[] = await firstValueFrom(
        this.service.getRegistros(this.filtroDesde, this.filtroHasta, this.filtroOP)
      );
      if (registros.length === 0) { alert('No hay registros para exportar'); return; }

      // Fotos y logo se descargan antes de armar el documento (de 6 en 6)
      const archivos = Array.from(new Set(registros.flatMap((r: any) => this.fotosDe(r))));
      const fotos: { [archivo: string]: { data: string; w: number; h: number } | null } = {};
      for (let i = 0; i < archivos.length; i += 6) {
        const lote = archivos.slice(i, i + 6);
        const res = await Promise.all(lote.map(a => this.urlToBase64(this.baseUrl + '/formatos/' + a)));
        lote.forEach((a, k) => { fotos[a] = res[k]; });
      }
      const logo = this.logoUrl ? await this.urlToBase64(this.logoUrl, 300, 0.9) : null;

      // Un grupo por OP, con sus días en orden cronológico
      const grupos: { [op: string]: any[] } = {};
      registros.forEach((r: any) => {
        if (!grupos[r.ordenProduccion]) grupos[r.ordenProduccion] = [];
        grupos[r.ordenProduccion].push(r);
      });
      const tiempo = (r: any) => new Date(r.diaEntrada || r.fecha).getTime();
      Object.values(grupos).forEach(g => g.sort((a, b) => tiempo(a) - tiempo(b)));

      const doc = new jsPDF('p', 'mm', 'letter');

      let y = this.pdfFormatoOficial(doc, registros, grupos, logo);
      y = this.pdfAnexo(doc, grupos, fotos, logo);

      if (y + 12 > 262) { doc.addPage(); y = 10; }
      doc.setFont('helvetica', 'bold'); doc.setFontSize(10); doc.setTextColor(0, 0, 0);
      doc.text('FIN DEL DOCUMENTO', 108, y + 6, { align: 'center' });

      const paginas = doc.getNumberOfPages();
      for (let p = 1; p <= paginas; p++) {
        doc.setPage(p);
        doc.setFont('helvetica', 'normal'); doc.setFontSize(6); doc.setTextColor(0, 0, 0);
        doc.text('Página ' + p + ' de ' + paginas, 206, 272, { align: 'right' });
      }
      doc.save('F-GC-008_' + new Date().toISOString().slice(0, 10) + '.pdf');
    } catch (e) {
      console.error(e);
      alert('No se pudo generar el PDF');
    } finally {
      this.generandoPdf = false;
    }
  }

  private pdfEncabezado(doc: jsPDF, yy: number, logo: { data: string; w: number; h: number } | null): number {
    const W = 216, M = 10;
    doc.setDrawColor(0); doc.setLineWidth(0.4);
    doc.rect(M, yy, W - M * 2, 24);
    doc.rect(M, yy, 24, 24);
    if (logo) {
      try {
        const esc = Math.min(22 / logo.w, 22 / logo.h);
        doc.addImage(logo.data, 'JPEG', M + 1, yy + 1, logo.w * esc, logo.h * esc);
      } catch (e) { }
    }
    doc.setFontSize(10); doc.setTextColor(0, 0, 0); doc.setFont('helvetica', 'bold');
    doc.text('EMPAQUES PLASTICOS S.A.S', W / 2 + 5, yy + 8, { align: 'center' });
    doc.setFontSize(7); doc.setFont('helvetica', 'normal');
    doc.text('NIT:816000992-1', W / 2 + 5, yy + 13, { align: 'center' });
    doc.setFontSize(9); doc.setFont('helvetica', 'bold');
    doc.text('FORMATO: CONTROL BODEGA PRODUCTO TERMINADO', W / 2 + 5, yy + 19, { align: 'center' });
    doc.rect(W - M - 42, yy, 42, 8); doc.rect(W - M - 42, yy + 8, 42, 8); doc.rect(W - M - 42, yy + 16, 42, 8);
    doc.setFontSize(7); doc.setFont('helvetica', 'normal');
    doc.text('Codigo: F-GC-008', W - M - 40, yy + 5);
    doc.text('Version: 001', W - M - 40, yy + 13);
    doc.text('Fecha: 15/09/2024', W - M - 40, yy + 21);
    return yy + 24;
  }

  private pdfColumnas(doc: jsPDF, yy: number): number {
    const M = 10;
    const cw = this.pdfCw;
    doc.setDrawColor(0); doc.setLineWidth(0.4);
    doc.setTextColor(0, 0, 0); doc.setFont('helvetica', 'bold'); doc.setFontSize(5.5);
    let x = M;
    doc.rect(x, yy, cw[0], 18, 'D'); doc.text('ORDEN DE', x + cw[0] / 2, yy + 7, { align: 'center' }); doc.text('PRODUCCION', x + cw[0] / 2, yy + 11, { align: 'center' }); x += cw[0];
    doc.rect(x, yy, cw[1], 18, 'D'); doc.text('Fecha', x + cw[1] / 2, yy + 7, { align: 'center' }); doc.text('entrada', x + cw[1] / 2, yy + 11, { align: 'center' }); x += cw[1];
    doc.setFontSize(4.3);
    doc.rect(x, yy, cw[2] + cw[3], 10, 'D'); doc.text('Etiquetas', x + (cw[2] + cw[3]) / 2, yy + 3, { align: 'center' }); doc.text('completas,', x + (cw[2] + cw[3]) / 2, yy + 5.5, { align: 'center' }); doc.text('visibles y legibles', x + (cw[2] + cw[3]) / 2, yy + 8, { align: 'center' }); x += cw[2] + cw[3];
    doc.rect(x, yy, cw[4] + cw[5], 10, 'D'); doc.text('Embalaje', x + (cw[4] + cw[5]) / 2, yy + 3, { align: 'center' }); doc.text('uniforme,', x + (cw[4] + cw[5]) / 2, yy + 5.5, { align: 'center' }); doc.text('sellado y adecuado', x + (cw[4] + cw[5]) / 2, yy + 8, { align: 'center' }); x += cw[4] + cw[5];
    doc.rect(x, yy, cw[6] + cw[7], 10, 'D'); doc.text('Defectos visibles', x + (cw[6] + cw[7]) / 2, yy + 3, { align: 'center' }); doc.text('en el empaque', x + (cw[6] + cw[7]) / 2, yy + 5.5, { align: 'center' }); doc.text('o producto', x + (cw[6] + cw[7]) / 2, yy + 8, { align: 'center' }); x += cw[6] + cw[7];
    doc.setFontSize(5.5);
    doc.rect(x, yy, cw[8] + cw[9] + cw[10], 10, 'D');
    doc.text('Concuerda cantidad OP', x + (cw[8] + cw[9] + cw[10]) / 2, yy + 4.5, { align: 'center', maxWidth: cw[8] + cw[9] + cw[10] - 1 });
    doc.text('con cantidad empacada', x + (cw[8] + cw[9] + cw[10]) / 2, yy + 8, { align: 'center', maxWidth: cw[8] + cw[9] + cw[10] - 1 });
    x += cw[8] + cw[9] + cw[10];
    doc.rect(x, yy, cw[11] + cw[12], 10, 'D'); doc.text('Listo para', x + (cw[11] + cw[12]) / 2, yy + 4.5, { align: 'center' }); doc.text('entrada Bodega', x + (cw[11] + cw[12]) / 2, yy + 8, { align: 'center', maxWidth: cw[11] + cw[12] - 1 }); x += cw[11] + cw[12];
    doc.rect(x, yy, cw[13], 18, 'D'); doc.text('Despachado', x + cw[13] / 2, yy + 10, { align: 'center' });

    x = M + cw[0] + cw[1];
    ['SI', 'NO', 'SI', 'NO', 'SI', 'NO'].forEach((t, i) => {
      doc.rect(x, yy + 10, cw[2 + i], 8, 'D'); doc.text(t, x + cw[2 + i] / 2, yy + 14.5, { align: 'center' }); x += cw[2 + i];
    });
    doc.rect(x, yy + 10, cw[8], 8, 'D'); doc.text('Cant. OP', x + cw[8] / 2, yy + 14.5, { align: 'center', maxWidth: cw[8] - 1 }); x += cw[8];
    doc.rect(x, yy + 10, cw[9], 8, 'D'); doc.text('Cant.', x + cw[9] / 2, yy + 14.5, { align: 'center' }); x += cw[9];
    doc.rect(x, yy + 10, cw[10], 8, 'D'); doc.text('SI/NO', x + cw[10] / 2, yy + 14.5, { align: 'center' }); x += cw[10];
    ['SI', 'NO'].forEach((t, i) => {
      doc.rect(x, yy + 10, cw[11 + i], 8, 'D'); doc.text(t, x + cw[11 + i] / 2, yy + 14.5, { align: 'center' }); x += cw[11 + i];
    });
    doc.setFont('helvetica', 'normal'); doc.setFontSize(6);
    return yy + 18;
  }

  private pdfFila(doc: jsPDF, r: any, yy: number, cantOP: number): void {
    const M = 10;
    const cw = this.pdfCw;
    const rowH = this.pdfRowH;
    const unidad = this.pdfUnidad(r);
    const dec = unidad === 'kg' ? 2 : 0;

    const check = (cx: number, cy: number) => {
      doc.setDrawColor(0, 0, 0); doc.setLineWidth(0.5);
      doc.line(cx - 1.5, cy, cx - 0.3, cy + 1.4);
      doc.line(cx - 0.3, cy + 1.4, cx + 1.8, cy - 1.6);
      doc.setLineWidth(0.4);
    };

    doc.setDrawColor(0); doc.setLineWidth(0.4); doc.setTextColor(0, 0, 0);
    let x = M;

    // Orden, cliente y referencia
    doc.rect(x, yy, cw[0], rowH);
    doc.setFont('helvetica', 'bold'); doc.setFontSize(6.5);
    doc.text(String(r.ordenProduccion), x + cw[0] / 2, yy + 3, { align: 'center' });
    doc.setFont('helvetica', 'normal'); doc.setFontSize(4.5);
    doc.text(this.pdfCortar(r.cliente, 24), x + cw[0] / 2, yy + 5.6, { align: 'center' });
    doc.text(this.pdfCortar(r.referencia, 24), x + cw[0] / 2, yy + 8, { align: 'center' });
    doc.setFontSize(6);
    x += cw[0];

    // Fecha de entrada
    doc.rect(x, yy, cw[1], rowH);
    doc.text(this.pdfDia(r), x + cw[1] / 2, yy + 5, { align: 'center', maxWidth: cw[1] - 1 });
    x += cw[1];

    // Etiquetas, embalaje y defectos (SI / NO)
    doc.rect(x, yy, cw[2], rowH); if (r.etiquetasSI) check(x + cw[2] / 2, yy + 4); x += cw[2];
    doc.rect(x, yy, cw[3], rowH); if (!r.etiquetasSI) check(x + cw[3] / 2, yy + 4); x += cw[3];
    doc.rect(x, yy, cw[4], rowH); if (r.embalajeSI) check(x + cw[4] / 2, yy + 4); x += cw[4];
    doc.rect(x, yy, cw[5], rowH); if (!r.embalajeSI) check(x + cw[5] / 2, yy + 4); x += cw[5];
    doc.rect(x, yy, cw[6], rowH); if (r.defectosSI) check(x + cw[6] / 2, yy + 4); x += cw[6];
    doc.rect(x, yy, cw[7], rowH); if (!r.defectosSI) check(x + cw[7] / 2, yy + 4); x += cw[7];

    // Cantidad programada
    doc.setFillColor(225, 225, 225);
    doc.rect(x, yy, cw[8], rowH, 'FD');
    doc.setFont('helvetica', 'bold');
    doc.text(this.pdfNum(cantOP, Number.isInteger(cantOP) ? 0 : 2), x + cw[8] / 2, yy + 5, { align: 'center' });
    doc.setFont('helvetica', 'normal');
    x += cw[8];

    // Cantidad real de ese día, con su unidad
    doc.rect(x, yy, cw[9], rowH);
    doc.text(this.pdfNum(r.cantidadReal, dec), x + cw[9] / 2, yy + 4, { align: 'center' });
    doc.setFontSize(4.5);
    doc.text(unidad, x + cw[9] / 2, yy + 7.5, { align: 'center' });
    doc.setFontSize(6);
    x += cw[9];

    // Concuerda: el acumulado (Bodega) o lo real (manual) contra lo programado
    const comparado = r.origen === 'Bodega' ? Number(r.acumuladoOP ?? r.cantidadReal) : Number(r.cantidadReal);
    const ok = comparado >= cantOP;
    doc.rect(x, yy, cw[10], rowH);
    doc.setTextColor(ok ? 26 : 180, ok ? 127 : 28, ok ? 90 : 28);
    doc.text(ok ? 'SI' : 'NO', x + cw[10] / 2, yy + 5, { align: 'center' });
    doc.setTextColor(0, 0, 0);
    x += cw[10];

    // Listo para entrada a Bodega
    doc.rect(x, yy, cw[11], rowH); if (r.listoBodega) check(x + cw[11] / 2, yy + 4); x += cw[11];
    doc.rect(x, yy, cw[12], rowH); if (!r.listoBodega) check(x + cw[12] / 2, yy + 4); x += cw[12];

    // Despachado
    doc.rect(x, yy, cw[13], rowH);
    doc.text(this.pdfCortar(r.despachado, 40), x + 1, yy + 3.5, { maxWidth: cw[13] - 2 });
  }

  // Formato oficial: una fila por día, con total por OP, acciones tomadas, firma y cargo. Devuelve la posición vertical final
  private pdfFormatoOficial(
    doc: jsPDF,
    registros: any[],
    grupos: { [op: string]: any[] },
    logo: { data: string; w: number; h: number } | null
  ): number {
    const W = 216, M = 10, PIE = 262;
    const cw = this.pdfCw;
    const rowH = this.pdfRowH;

    const nuevaPagina = (): number => {
      doc.addPage();
      return this.pdfColumnas(doc, this.pdfEncabezado(doc, M, logo));
    };

    let y = this.pdfEncabezado(doc, M, logo);

    doc.setFillColor(240, 240, 240);
    doc.rect(M, y, 22, 7, 'FD');
    doc.setFont('helvetica', 'bold'); doc.setFontSize(8); doc.setTextColor(0, 0, 0);
    doc.text('FECHA', M + 11, y + 5, { align: 'center' });
    doc.rect(M + 22, y, W - M * 2 - 22, 7);
    doc.setFont('helvetica', 'normal');
    doc.text(new Date().toLocaleDateString('es-CO'), M + 25, y + 5);
    y += 7;

    doc.setFillColor(248, 248, 248);
    doc.rect(M, y, W - M * 2, 6, 'FD');
    doc.setFont('helvetica', 'bold'); doc.setFontSize(8);
    doc.text('REGISTRO VERIFICACION DE EMPAQUE', W / 2, y + 4, { align: 'center' });
    y += 6;

    doc.setFontSize(7); doc.setFont('helvetica', 'italic');
    doc.rect(M, y, W - M * 2, 8);
    doc.text('Nota: Las verificaciones consisten en comprobar la informacion del empaque del producto final con los criterios de calidad y la informacion especificada en la orden de produccion.', M + 2, y + 3.5, { maxWidth: W - M * 2 - 4 });
    y += 9;

    y = this.pdfColumnas(doc, y);

    for (const op of Object.keys(grupos)) {
      const filas = grupos[op];
      const bodega = filas.filter((r: any) => r.origen === 'Bodega');
      const cantOP = Number((bodega[0] ?? filas[0]).cantidadOP) || 0;
      const unidadOp = bodega.length > 0 ? this.pdfUnidad(bodega[0]) : '';
      const dec = unidadOp === 'kg' ? 2 : 0;
      const totalReal = bodega.length > 0
        ? Math.max(...bodega.map((r: any) => Number(r.acumuladoOP) || 0))
        : filas.reduce((s: number, r: any) => s + (Number(r.cantidadReal) || 0), 0);

      if (y + rowH + 6 > PIE) y = nuevaPagina();

      for (const r of filas) {
        if (y + rowH > PIE) y = nuevaPagina();
        this.pdfFila(doc, r, y, cantOP);
        y += rowH;
      }

      if (y + 6 > PIE) y = nuevaPagina();
      const dif = totalReal - cantOP;
      const pct = cantOP > 0 ? (dif / cantOP) * 100 : 0;
      const tw = cw[0] + cw[1] + cw[2] + cw[3] + cw[4] + cw[5] + cw[6] + cw[7] + cw[8];
      let x = M;
      doc.setDrawColor(0); doc.setLineWidth(0.4);
      doc.setFillColor(225, 225, 225); doc.setFont('helvetica', 'bold'); doc.setFontSize(6); doc.setTextColor(0, 0, 0);
      doc.rect(x, y, tw, 5, 'FD'); doc.text('TOTAL ' + op + ' — ' + filas.length + ' registros', x + 2, y + 3.5); x += tw;
      doc.rect(x, y, cw[9], 5, 'FD'); doc.text(this.pdfNum(totalReal, dec), x + cw[9] / 2, y + 3.5, { align: 'center' }); x += cw[9];
      doc.rect(x, y, cw[10] + cw[11] + cw[12] + cw[13], 5, 'FD');
      doc.setTextColor(dif < 0 ? 180 : 26, dif < 0 ? 28 : 127, dif < 0 ? 28 : 90);
      doc.text('Dif: ' + (dif >= 0 ? '+' : '') + this.pdfNum(dif, dec) + ' ' + unidadOp + ' (' + this.pdfNum(pct, 1) + '%)', x + 2, y + 3.5);
      doc.setTextColor(0, 0, 0); doc.setFont('helvetica', 'normal');
      y += 8;
    }

    // Acciones tomadas (con el día), firma y cargo
    const accionesTexto = registros
      .filter((r: any) => r.accionesTomadas)
      .map((r: any) => r.ordenProduccion + ' (' + this.pdfDia(r) + '): ' + r.accionesTomadas)
      .join(' | ');
    doc.setFont('helvetica', 'normal'); doc.setFontSize(6.5);
    const lineasAcciones: string[] = accionesTexto ? doc.splitTextToSize(accionesTexto, W - M * 2 - 4) : [];
    const footerH = Math.max(18, 10 + lineasAcciones.length * 3);
    const firmaH = 14;

    if (y + footerH + firmaH + 20 > PIE) { doc.addPage(); y = M; } else { y += 3; }

    doc.setDrawColor(0); doc.setLineWidth(0.4);
    doc.rect(M, y, W - M * 2, footerH);
    doc.setFont('helvetica', 'bold'); doc.setFontSize(6.5);
    doc.text('Mencione las acciones tomadas para cada OP que incumplio alguna verificacion:', M + 2, y + 4, { maxWidth: W - M * 2 - 4 });
    doc.setFont('helvetica', 'normal');
    lineasAcciones.forEach((l, i) => doc.text(l, M + 2, y + 8 + i * 3));
    y += footerH + 2;

    const fx = M + (W - M * 2) * 0.60;
    const fw = (W - M * 2) * 0.40;
    const ultimo = registros.find((r: any) => r.firmaDigital) ?? registros[0];

    doc.rect(M, y, (W - M * 2) * 0.58, firmaH);
    doc.setFont('helvetica', 'bold'); doc.setFontSize(6.5);
    doc.text('Firma de quien hace la entrada a Bodega', M + 2, y + 4);
    if (ultimo?.firmaDigital) { try { doc.addImage(ultimo.firmaDigital, 'PNG', M + 2, y + 5, 24, 7); } catch (e) { } }
    doc.setFont('helvetica', 'normal'); doc.setFontSize(6);
    const nombreRevisor = ultimo?.revisadoPorNombre ? ' — ' + ultimo.revisadoPorNombre : '';
    doc.text('Realizado por: ' + (ultimo?.revisadoPor || '-') + nombreRevisor, M + 2, y + firmaH - 2, { maxWidth: (W - M * 2) * 0.58 - 4 });

    doc.rect(fx, y, fw, firmaH);
    doc.setFont('helvetica', 'bold'); doc.setFontSize(6.5);
    doc.text('Cargo', fx + 2, y + 4);
    doc.setFont('helvetica', 'normal');
    doc.text(this.cargoFirma, fx + 2, y + 9);
    y += firmaH + 3;

    doc.setFontSize(7); doc.setFont('helvetica', 'italic');
    doc.text('Nota: Los metodos de verificacion de las caracteristicas y los criterios de cumplimiento a tener en cuenta, estan definidos en el Procedimiento de Pruebas y Ensayos.', M, y + 3, { maxWidth: W - M * 2 });
    y += 8;
    return y;
  }
  // Anexo: detalle por día de cada OP (con la firma de cada fila) y todas las fotos de evidencia
  private pdfAnexo(
    doc: jsPDF,
    grupos: { [op: string]: any[] },
    fotos: { [archivo: string]: { data: string; w: number; h: number } | null },
    logo: { data: string; w: number; h: number } | null
  ): number {
    const W = 216, M = 10, PIE = 262;
    const GRIS: [number, number, number] = [225, 225, 225];
    const NEGRO: [number, number, number] = [0, 0, 0];
    const estadoTexto: { [k: string]: string } = {
      proceso: 'En proceso', cambio: 'Cambió después de revisar', revisada: 'Revisada', sin: 'Sin revisar'
    };
    const nombreSeccion = (r: any): string =>
      r.origen === 'Bodega' ? (r.seccion || '-') : (r.origen === 'Anterior' ? 'Registro anterior' : 'Manual');

    const nuevaPagina = (): number => {
      doc.addPage();
      return this.pdfEncabezado(doc, M, logo) + 4;
    };

    let y = nuevaPagina();
    doc.setDrawColor(0); doc.setLineWidth(0.4);
    doc.setFillColor(240, 240, 240);
    doc.rect(M, y, W - M * 2, 7, 'FD');
    doc.setFont('helvetica', 'bold'); doc.setFontSize(8); doc.setTextColor(0, 0, 0);
    doc.text('ANEXO — DETALLE POR DÍA Y EVIDENCIA FOTOGRÁFICA', W / 2, y + 4.8, { align: 'center' });
    y += 11;

    for (const op of Object.keys(grupos)) {
      const filas = grupos[op];
      const bodega = filas.filter((r: any) => r.origen === 'Bodega');
      const base = bodega[0] ?? filas[0];
      const unidadOp = bodega.length > 0 ? this.pdfUnidad(bodega[0]) : '';

      if (y + 30 > PIE) y = nuevaPagina();

      // Barra de la OP
      doc.setDrawColor(0); doc.setLineWidth(0.4);
      doc.setFillColor(GRIS[0], GRIS[1], GRIS[2]);
      doc.rect(M, y, W - M * 2, 7, 'FD');
      doc.setFont('helvetica', 'bold'); doc.setFontSize(7); doc.setTextColor(0, 0, 0);
      doc.text(this.pdfCortar('OP ' + op + ' — ' + (base.cliente || '-') + ' — ' + (base.referencia || '-'), 80), M + 2, y + 4.7);
      const prog = Number(base.cantidadOP) || 0;
      doc.text('Programada: ' + this.pdfNum(prog, Number.isInteger(prog) ? 0 : 2) + ' ' + unidadOp, W - M - 2, y + 4.7, { align: 'right' });
      y += 9;

      // Tabla de detalle: una fila por día
      const cuerpo = filas.map((r: any) => {
        const esBodega = r.origen === 'Bodega';
        const u = this.pdfUnidad(r);
        const dec = u === 'kg' ? 2 : 0;
        const estado = this.estadoRevision(r);

        let revision = estadoTexto[estado];
        if (r.revisadoPor && estado !== 'sin') {
          revision += '\n' + r.revisadoPor + (r.revisadoPorNombre ? ' — ' + r.revisadoPorNombre : '');
          if (r.fechaRevision) revision += '\n' + new Date(r.fechaRevision).toLocaleString('es-CO');
        }

        let falta = '-';
        if (esBodega && r.diferenciaPct != null) {
          falta = r.porHacer > 0
            ? 'Faltan ' + this.pdfNum(r.porHacer, dec) + ' (' + this.pdfNum(r.diferenciaPct, 1) + '%)'
            : (r.diferenciaPct > 0 ? '+' : '') + this.pdfNum(r.diferenciaPct, 1) + '% sobre lo programado';
        } else if (!esBodega) {
          const pct = this.diferenciaPct(r);
          if (pct !== null) falta = (pct > 0 ? '+' : '') + this.pdfNum(pct, 1) + '%';
        }

        const acumulado = esBodega
          ? this.pdfNum(r.acumuladoOP, dec) + ' ' + u
          : this.pdfNum(r.cantidadReal, Number.isInteger(Number(r.cantidadReal)) ? 0 : 2);

        return [
          this.pdfDia(r),
          nombreSeccion(r),
          esBodega ? (r.operariosBodega || '-') : '-',
          esBodega ? String(r.bultos ?? 0) : '-',
          esBodega ? (r.troquel || '-') : '-',
          esBodega ? this.pdfNum(r.unidadesEntrada, 0) : '-',
          esBodega ? this.pdfNum(r.kilosEntrada, 2) : '-',
          acumulado,
          falta,
          revision,
          r.despachado || '-',
          ''
        ];
      });

      autoTable(doc, {
        startY: y,
        head: [['Día', 'Sección', 'Operario(s)', 'Bultos', 'Troq.', 'Und', 'Kg', 'Acumulado', 'Falta / dif.', 'Revisión', 'Despachado', 'Firma']],
        body: cuerpo,
        theme: 'grid',
        showHead: 'everyPage',
        margin: { left: M, right: M, top: M, bottom: 18 },
        headStyles: { fillColor: GRIS, textColor: NEGRO, fontSize: 6, lineColor: NEGRO, lineWidth: 0.3 },
        bodyStyles: { fontSize: 6, lineColor: NEGRO, lineWidth: 0.3, textColor: NEGRO, minCellHeight: 9, valign: 'middle' },
        columnStyles: {
          0: { cellWidth: 15 },
          1: { cellWidth: 15 },
          2: { cellWidth: 20 },
          3: { cellWidth: 9, halign: 'center' },
          4: { cellWidth: 10, halign: 'center' },
          5: { cellWidth: 14, halign: 'right' },
          6: { cellWidth: 14, halign: 'right' },
          7: { cellWidth: 15, halign: 'right' },
          8: { cellWidth: 18 },
          9: { cellWidth: 26 },
          10: { cellWidth: 16 },
          11: { cellWidth: 24 }
        },
        didDrawCell: (data: any) => {
          // Firma de cada fila, dibujada dentro de su celda
          if (data.section === 'body' && data.column.index === 11) {
            const r = filas[data.row.index];
            if (r?.firmaDigital) {
              const w = Math.min(data.cell.width - 2, 22);
              const h = w * (100 / 340);
              try {
                doc.addImage(r.firmaDigital, 'PNG',
                  data.cell.x + (data.cell.width - w) / 2,
                  data.cell.y + (data.cell.height - h) / 2, w, h);
              } catch (e) { }
            }
          }
        }
      });
      y = (doc as any).lastAutoTable.finalY + 3;

      // Acciones tomadas de cada día
      const conAcciones = filas.filter((r: any) => r.accionesTomadas);
      if (conAcciones.length > 0) {
        doc.setFont('helvetica', 'normal'); doc.setFontSize(6.5); doc.setTextColor(0, 0, 0);
        const texto = 'Acciones tomadas — ' + conAcciones.map((r: any) => this.pdfDia(r) + ': ' + r.accionesTomadas).join(' | ');
        const lineas: string[] = doc.splitTextToSize(texto, W - M * 2);
        if (y + lineas.length * 3 + 2 > PIE) y = nuevaPagina();
        lineas.forEach((l, i) => doc.text(l, M, y + 3 + i * 3));
        y += lineas.length * 3 + 3;
      }

      // Fotos de evidencia: la antigua y las nuevas de todos los días, de a 4 por fila
      const items: { archivo: string; dia: string; seccion: string }[] = [];
      filas.forEach((r: any) => this.fotosDe(r).forEach((a: string) =>
        items.push({ archivo: a, dia: this.pdfDia(r), seccion: nombreSeccion(r) })));

      if (items.length > 0) {
        if (y + 49 > PIE) y = nuevaPagina();
        doc.setFont('helvetica', 'bold'); doc.setFontSize(7); doc.setTextColor(0, 0, 0);
        doc.text('Fotos de evidencia (' + items.length + ')', M, y + 3);
        y += 6;

        let col = 0;
        for (const it of items) {
          if (col === 4) { col = 0; y += 41; }
          if (y + 41 > PIE) { y = nuevaPagina(); col = 0; }

          const x = M + col * 50;
          const f = fotos[it.archivo];
          doc.setDrawColor(200, 200, 200); doc.setLineWidth(0.2);
          doc.rect(x, y, 46, 34);

          if (f) {
            const esc = Math.min(46 / f.w, 34 / f.h);
            const w = f.w * esc, h = f.h * esc;
            try {
              doc.addImage(f.data, 'JPEG', x + (46 - w) / 2, y + (34 - h) / 2, w, h);
            } catch (e) {
              doc.setFont('helvetica', 'normal'); doc.setFontSize(6);
              doc.text('No disponible', x + 23, y + 18, { align: 'center' });
            }
          } else {
            doc.setFont('helvetica', 'normal'); doc.setFontSize(6); doc.setTextColor(0, 0, 0);
            doc.text('No disponible', x + 23, y + 18, { align: 'center' });
          }

          doc.setFont('helvetica', 'normal'); doc.setFontSize(5.5); doc.setTextColor(0, 0, 0);
          doc.text(this.pdfCortar(it.dia + ' · ' + it.seccion, 34), x, y + 37);
          col++;
        }
        y += 41;
      }

      y += 4;
    }

    doc.setDrawColor(0); doc.setLineWidth(0.4);
    return y;
  }
}