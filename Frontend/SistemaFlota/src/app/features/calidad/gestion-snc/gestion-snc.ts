import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SalidaNoConformeService, VerificacionSncDto } from '../../../core/services/salida-no-conforme.service';
import { OrdenesProduccionService } from '../../../core/services/ordenes-produccion.service';
import { OpcionesFormularioService } from '../../../core/services/opciones-formulario.service';
import { PermisosService } from '../../../core/services/permisos.service';
import { MatDialog } from '@angular/material/dialog';
import { DialogConfirmacion } from '../../../shared/dialog-confirmacion/dialog-confirmacion';
import { environment } from '../../../../environments/environment';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

@Component({
    selector: 'app-gestion-snc',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './gestion-snc.html',
    styleUrls: ['./gestion-snc.scss']
})
export class GestionSncComponent implements OnInit {
    vista: 'lista' | 'nuevo' | 'ver' | 'editar' = 'lista';
    registros: any[] = [];
    registroSeleccionado: any = null;
    cargando = false;
    editandoId: number | null = null;

    // Filtros
    filtroDesde = '';
    filtroHasta = '';
    filtroReferencia = '';
    filtroMaterial = '';
    filtroOrdenProduccion = '';
    filtroTipoDefecto = '';
    filtroProceso = '';
    kgDelMes: number | null = null;
    sumaFiltrada: { total: number; totalRegistros: number } | null = null;

    // Formulario Paso 1
    form = {
        ordenProduccion: '',
        referencia: '',
        cantidadKg: null as number | null,
        cliente: '',
        linea: '',
        material: '',
        proceso: '',
        descripcionSalida: '',
        tipoDefecto: '',
        impacto: '',
        causaRaiz: '',
        nombreReporta: '',
        unidadCantidadReportada: 'Unidades',
        cantidadReportadaKg: null as number | null
    };
    evidenciaImagen: File | null = null;
    evidenciaPdf: File | null = null;
    evidenciaPdfVerificacion: File | null = null;
    firmaReportaUrl: string | null = null;

    // Formulario Paso 2
    formTratamiento = {
        tratamientoAdoptado: '',
        descripcionTratamiento: '',
        fechaTratamiento: ''
    };
    firmaTratamientoUrl: string | null = null;

    // Formulario Paso 3
    formVerificacion = {
        verificacionCumplimiento: '',
        requiereInformacionCliente: null as boolean | null,
        motivoInformacionCliente: '',
        aceptacionBajoConcesion: null as boolean | null,
        detalleAceptacionConcesion: '',
        revisadoPor: ''
    };
    firmaVerificacionUrl: string | null = null;

    modoFirmaReporta: 'dibujar' | 'escribir' = 'dibujar';
    modoFirmaTratamiento: 'dibujar' | 'escribir' = 'dibujar';
    modoFirmaVerificacion: 'dibujar' | 'escribir' = 'dibujar';

    // Opciones desplegables
    opcionesLinea: any[] = [];
    opcionesMaterial: any[] = [];
    opcionesProceso: any[] = [];
    opcionesTipoDefecto: any[] = [];
    opcionesImpacto: any[] = [];
    opcionesTratamiento: any[] = [];

    opcionesTipoDefectoFijas = [
        'Calibre fuera de tolerancia',
        'Dimensiones fuera de tolerancia',
        'Defecto de resistencia en la película plástica',
        'Defecto en la apariencia en la película plástica',
        'Defectos en la impresión',
        'Defecto en la línea de sellado',
        'Defecto en la línea de precorte',
        'Defecto de perforación o troquelado',
        'Falta de registro o trazabilidad',
        'Materia prima o mezcla defectuosa',
        'Otro'
    ];
    tiposDefectoSeleccionados: string[] = [];
    otroDefectoDescripcion = '';

    fotosSeleccionadas: { [paso: string]: File[] } = { Reportar: [], Tratamiento: [], Verificacion: [] };
    evidenciasCargadas: any[] = [];

    get usuario(): string {
        const u = JSON.parse(sessionStorage.getItem('user') || '{}');
        return u.username ?? '';
    }
    get esAdmin(): boolean {
        const u = JSON.parse(sessionStorage.getItem('user') || '{}');
        return u.rol === 'Admin';
    }
    get puedeCrear(): boolean { return this.permisosService.puedeCrear('gestion-snc'); }
    get puedeEliminar(): boolean { return this.permisosService.puedeEliminar('gestion-snc'); }

    constructor(
        private service: SalidaNoConformeService,
        private ordenesService: OrdenesProduccionService,
        private opcionesService: OpcionesFormularioService,
        private permisosService: PermisosService,
        private dialog: MatDialog
    ) { }

    ngOnInit(): void {
        this.cargar();
        this.cargarOpciones();
    }

    cargarOpciones() {
        this.opcionesService.getOpciones('Linea').subscribe({ next: d => this.opcionesLinea = d });
        this.opcionesService.getOpciones('Material').subscribe({ next: d => this.opcionesMaterial = d });
        this.opcionesService.getOpciones('ProcesoSNC').subscribe({ next: d => this.opcionesProceso = d });
        this.opcionesService.getOpciones('TipoDefecto').subscribe({ next: d => this.opcionesTipoDefecto = d });
        this.opcionesService.getOpciones('Impacto').subscribe({ next: d => this.opcionesImpacto = d });
        this.opcionesService.getOpciones('TratamientoAdoptado').subscribe({ next: d => this.opcionesTratamiento = d });
    }

    cargar() {
        this.cargando = true;
        this.service.getRegistros(
            this.filtroDesde, this.filtroHasta, this.filtroReferencia,
            this.filtroMaterial, this.filtroOrdenProduccion, this.filtroTipoDefecto, this.filtroProceso
        ).subscribe({
            next: (d) => { this.registros = d; this.cargando = false; },
            error: (e) => { console.error(e); this.cargando = false; }
        });
        this.consultarSuma();
    }

    etiquetaEstado(estado: string): string {
        const mapa: { [key: string]: string } = {
            Reportado: 'Abierta', Tratamiento: 'En tratamiento', Cerrado: 'Cerrada'
        };
        return mapa[estado] || estado;
    }

    consultarSuma() {
        this.service.sumaFiltrada(this.filtroDesde, this.filtroHasta, this.filtroReferencia, this.filtroMaterial, this.filtroOrdenProduccion, this.filtroTipoDefecto, this.filtroProceso)
            .subscribe({ next: (d) => this.sumaFiltrada = d });
    }

    consultarKgDelMes() {
        const ahora = new Date();
        this.service.kgDelMes(ahora.getMonth() + 1, ahora.getFullYear()).subscribe({
            next: (data) => { this.kgDelMes = data.totalKg; }
        });
    }

    buscarOP() {
        if (!this.form.ordenProduccion) return;
        this.ordenesService.buscar(this.form.ordenProduccion).subscribe({
            next: (data: any) => {
                this.form.referencia = data.referencia;
                this.form.cantidadKg = data.cantidadOP;
                this.form.cliente = data.cliente;
            },
            error: () => { /* no encontrada, se llena manual */ }
        });
    }

    seleccionarImagen(event: any) { this.evidenciaImagen = event.target.files[0]; }
    seleccionarPdf(event: any) { this.evidenciaPdf = event.target.files[0]; }
    seleccionarPdfVerificacion(event: any) { this.evidenciaPdfVerificacion = event.target.files[0]; }
    urlEvidencia(nombreArchivo: string): string {
        return `${environment.apiUrl.replace('/api', '')}/snc/${nombreArchivo}`;
    }
    toggleTipoDefecto(opcion: string) {
        const idx = this.tiposDefectoSeleccionados.indexOf(opcion);
        if (idx >= 0) this.tiposDefectoSeleccionados.splice(idx, 1);
        else this.tiposDefectoSeleccionados.push(opcion);
    }
    seleccionarFotos(event: any, paso: string) {
        const archivos = Array.from(event.target.files) as File[];
        this.fotosSeleccionadas[paso] = archivos.slice(0, 5);
    }

    cargarEvidencias(id: number) {
        this.service.getEvidencias(id).subscribe({
            next: (d) => { this.evidenciasCargadas = d; }
        });
    }

    evidenciasDePaso(paso: string): any[] {
        return this.evidenciasCargadas.filter(e => e.paso === paso);
    }

    iniciarCanvasReporta() {
        setTimeout(() => {
            const canvas = document.getElementById('firmaCanvasSnc') as HTMLCanvasElement;
            if (!canvas) return;
            const ctx = canvas.getContext('2d')!;
            let dibujando = false;
            canvas.addEventListener('mousedown', e => { dibujando = true; ctx.beginPath(); ctx.moveTo(e.offsetX, e.offsetY); });
            canvas.addEventListener('mousemove', e => { if (!dibujando) return; ctx.lineTo(e.offsetX, e.offsetY); ctx.stroke(); });
            canvas.addEventListener('mouseup', () => { dibujando = false; this.firmaReportaUrl = canvas.toDataURL(); });
            canvas.addEventListener('touchstart', e => { e.preventDefault(); dibujando = true; const t = e.touches[0]; const r = canvas.getBoundingClientRect(); ctx.beginPath(); ctx.moveTo(t.clientX - r.left, t.clientY - r.top); });
            canvas.addEventListener('touchmove', e => { e.preventDefault(); if (!dibujando) return; const t = e.touches[0]; const r = canvas.getBoundingClientRect(); ctx.lineTo(t.clientX - r.left, t.clientY - r.top); ctx.stroke(); });
            canvas.addEventListener('touchend', () => { dibujando = false; this.firmaReportaUrl = canvas.toDataURL(); });
        }, 300);
    }

    limpiarFirmaReporta() {
        const canvas = document.getElementById('firmaCanvasSnc') as HTMLCanvasElement;
        if (canvas) canvas.getContext('2d')!.clearRect(0, 0, canvas.width, canvas.height);
        this.firmaReportaUrl = null;
    }

    iniciarCanvasTratamiento() {
        setTimeout(() => {
            const canvas = document.getElementById('firmaCanvasTratamiento') as HTMLCanvasElement;
            if (!canvas) return;
            const ctx = canvas.getContext('2d')!;
            let dibujando = false;
            canvas.addEventListener('mousedown', e => { dibujando = true; ctx.beginPath(); ctx.moveTo(e.offsetX, e.offsetY); });
            canvas.addEventListener('mousemove', e => { if (!dibujando) return; ctx.lineTo(e.offsetX, e.offsetY); ctx.stroke(); });
            canvas.addEventListener('mouseup', () => { dibujando = false; this.firmaTratamientoUrl = canvas.toDataURL(); });
            canvas.addEventListener('touchstart', e => { e.preventDefault(); dibujando = true; const t = e.touches[0]; const r = canvas.getBoundingClientRect(); ctx.beginPath(); ctx.moveTo(t.clientX - r.left, t.clientY - r.top); });
            canvas.addEventListener('touchmove', e => { e.preventDefault(); if (!dibujando) return; const t = e.touches[0]; const r = canvas.getBoundingClientRect(); ctx.lineTo(t.clientX - r.left, t.clientY - r.top); ctx.stroke(); });
            canvas.addEventListener('touchend', () => { dibujando = false; this.firmaTratamientoUrl = canvas.toDataURL(); });
        }, 300);
    }

    limpiarFirmaTratamiento() {
        const canvas = document.getElementById('firmaCanvasTratamiento') as HTMLCanvasElement;
        if (canvas) canvas.getContext('2d')!.clearRect(0, 0, canvas.width, canvas.height);
        this.firmaTratamientoUrl = null;
    }

    iniciarCanvasVerificacion() {
        setTimeout(() => {
            const canvas = document.getElementById('firmaCanvasVerificacion') as HTMLCanvasElement;
            if (!canvas) return;
            const ctx = canvas.getContext('2d')!;
            let dibujando = false;
            canvas.addEventListener('mousedown', e => { dibujando = true; ctx.beginPath(); ctx.moveTo(e.offsetX, e.offsetY); });
            canvas.addEventListener('mousemove', e => { if (!dibujando) return; ctx.lineTo(e.offsetX, e.offsetY); ctx.stroke(); });
            canvas.addEventListener('mouseup', () => { dibujando = false; this.firmaVerificacionUrl = canvas.toDataURL(); });
            canvas.addEventListener('touchstart', e => { e.preventDefault(); dibujando = true; const t = e.touches[0]; const r = canvas.getBoundingClientRect(); ctx.beginPath(); ctx.moveTo(t.clientX - r.left, t.clientY - r.top); });
            canvas.addEventListener('touchmove', e => { e.preventDefault(); if (!dibujando) return; const t = e.touches[0]; const r = canvas.getBoundingClientRect(); ctx.lineTo(t.clientX - r.left, t.clientY - r.top); ctx.stroke(); });
            canvas.addEventListener('touchend', () => { dibujando = false; this.firmaVerificacionUrl = canvas.toDataURL(); });
        }, 300);
    }

    limpiarFirmaVerificacion() {
        const canvas = document.getElementById('firmaCanvasVerificacion') as HTMLCanvasElement;
        if (canvas) canvas.getContext('2d')!.clearRect(0, 0, canvas.width, canvas.height);
        this.firmaVerificacionUrl = null;
    }

    firmarConTexto(paso: 'reporta' | 'tratamiento' | 'verificacion', nombre: string) {
        const texto = nombre.trim();
        if (paso === 'reporta') this.firmaReportaUrl = texto || null;
        if (paso === 'tratamiento') this.firmaTratamientoUrl = texto || null;
        if (paso === 'verificacion') this.firmaVerificacionUrl = texto || null;
    }

    nuevo() {
        this.form = {
            ordenProduccion: '', referencia: '', cantidadKg: null, cliente: '',
            linea: '', material: '', proceso: '', descripcionSalida: '',
            tipoDefecto: '', impacto: '', causaRaiz: '', nombreReporta: '',
            cantidadReportadaKg: null, unidadCantidadReportada: 'Unidades'
        };
        this.evidenciaPdf = null;
        this.firmaReportaUrl = null;
        this.editandoId = null;
        this.vista = 'nuevo';
        this.tiposDefectoSeleccionados = [];
        this.otroDefectoDescripcion = '';
        this.iniciarCanvasReporta();

    }

    guardarPaso1() {
        if (!this.form.ordenProduccion.trim()) { alert('Ingrese la orden de producción'); return; }
        if (!this.firmaReportaUrl) { alert('Debe firmar el reporte'); return; }

        const fd = new FormData();
        fd.append('OrdenProduccion', this.form.ordenProduccion);
        fd.append('Referencia', this.form.referencia || '');
        fd.append('CantidadKg', String(this.form.cantidadKg ?? ''));
        fd.append('Cliente', this.form.cliente || '');
        fd.append('Linea', this.form.linea || '');
        fd.append('Material', this.form.material || '');
        fd.append('Proceso', this.form.proceso || '');
        fd.append('DescripcionSalida', this.form.descripcionSalida || '');
        let tipoDefectoTexto = this.tiposDefectoSeleccionados.filter(t => t !== 'Otro').join(', ');
        if (this.tiposDefectoSeleccionados.includes('Otro') && this.otroDefectoDescripcion.trim()) {
            tipoDefectoTexto += (tipoDefectoTexto ? ', ' : '') + 'Otro: ' + this.otroDefectoDescripcion.trim();
        }
        fd.append('TipoDefecto', tipoDefectoTexto);
        fd.append('Impacto', this.form.impacto || '');
        fd.append('CausaRaiz', this.form.causaRaiz || '');
        fd.append('CantidadReportadaKg', String(this.form.cantidadReportadaKg ?? ''));
        fd.append('UnidadCantidadReportada', this.form.unidadCantidadReportada || '');
        fd.append('FirmaReporta', this.firmaReportaUrl || '');
        fd.append('NombreReporta', this.form.nombreReporta || '');
        if (this.evidenciaPdf) fd.append('evidenciaPdf', this.evidenciaPdf);

        this.service.crear(fd).subscribe({
            next: (r) => {
                if (this.fotosSeleccionadas['Reportar'].length > 0) {
                    this.service.subirEvidencias(r.id, 'Reportar', this.fotosSeleccionadas['Reportar']).subscribe();
                }
                this.vista = 'lista'; this.cargar();
            },
            error: (e) => { console.error(e); alert('Error guardando el reporte'); }
        });
    }

    ver(r: any) {
        this.registroSeleccionado = r;
        this.editandoId = r.id;
        this.cargarEvidencias(r.id);
        this.formTratamiento = {
            tratamientoAdoptado: r.tratamientoAdoptado ?? '',
            descripcionTratamiento: r.descripcionTratamiento ?? '',
            fechaTratamiento: r.fechaTratamiento ?? ''
        };
        this.formVerificacion = {
            verificacionCumplimiento: r.verificacionCumplimiento ?? '',
            requiereInformacionCliente: r.requiereInformacionCliente,
            motivoInformacionCliente: r.motivoInformacionCliente ?? '',
            aceptacionBajoConcesion: r.aceptacionBajoConcesion,
            detalleAceptacionConcesion: r.detalleAceptacionConcesion ?? '',
            revisadoPor: r.revisadoPor ?? ''
        };
        this.vista = 'ver';

        if (r.estado === 'Reportado') this.iniciarCanvasTratamiento();
        if (r.estado === 'Tratamiento') this.iniciarCanvasVerificacion();
    }

    guardarTratamiento() {
        if (!this.editandoId) return;
        if (!this.formTratamiento.tratamientoAdoptado) { alert('Seleccione el tratamiento adoptado'); return; }
        if (!this.firmaTratamientoUrl) { alert('Debe firmar el tratamiento'); return; }

        const dto = { ...this.formTratamiento, firmaTratamiento: this.firmaTratamientoUrl };
        this.service.registrarTratamiento(this.editandoId, dto).subscribe({
            next: (r) => {
                this.registroSeleccionado = r;
                this.cargar();
                if (this.fotosSeleccionadas['Tratamiento'].length > 0) {
                    this.service.subirEvidencias(r.id, 'Tratamiento', this.fotosSeleccionadas['Tratamiento']).subscribe({
                        next: () => this.cargarEvidencias(r.id)
                    });
                }
                this.iniciarCanvasVerificacion();
            },
            error: (e) => { console.error(e); alert('Error guardando el tratamiento'); }
        });
    }

   guardarVerificacion() {
    if (!this.editandoId) return;
    if (!this.firmaVerificacionUrl) { alert('Debe firmar la verificación'); return; }

    const dialogRef = this.dialog.open(DialogConfirmacion, {
        data: {
            titulo: 'Cerrar salida no conforme',
            mensaje: '¿Confirma el cierre de este registro? No se podrá modificar después.',
            textoConfirmar: 'Cerrar',
            textoCancelar: 'Cancelar',
            tipo: 'warning'
        }
    });

    dialogRef.afterClosed().subscribe((confirmado: boolean) => {
        if (!confirmado) return;

        const fd = new FormData();
        fd.append('VerificacionCumplimiento', this.formVerificacion.verificacionCumplimiento || '');
        fd.append('RequiereInformacionCliente', String(this.formVerificacion.requiereInformacionCliente ?? ''));
        fd.append('MotivoInformacionCliente', this.formVerificacion.motivoInformacionCliente || '');
        fd.append('AceptacionBajoConcesion', String(this.formVerificacion.aceptacionBajoConcesion ?? ''));
        fd.append('DetalleAceptacionConcesion', this.formVerificacion.detalleAceptacionConcesion || '');
        fd.append('RevisadoPor', this.formVerificacion.revisadoPor || '');
        fd.append('FirmaVerificacion', this.firmaVerificacionUrl || '');
        if (this.evidenciaPdfVerificacion) fd.append('evidenciaPdf', this.evidenciaPdfVerificacion);

        this.service.cerrar(this.editandoId!, fd).subscribe({
            next: (r) => {
                if (this.fotosSeleccionadas['Verificacion'].length > 0) {
                    this.service.subirEvidencias(r.id, 'Verificacion', this.fotosSeleccionadas['Verificacion']).subscribe({
                        next: () => { this.vista = 'lista'; this.cargar(); }
                    });
                } else {
                    this.vista = 'lista'; this.cargar();
                }
            },
            error: (e) => { console.error(e); alert('Error cerrando el registro'); }
        });
    });
}

    eliminar(id: number) {
        const dialogRef = this.dialog.open(DialogConfirmacion, {
            data: {
                titulo: 'Eliminar registro',
                mensaje: '¿Está seguro de eliminar este registro? Esta acción no se puede deshacer.',
                textoConfirmar: 'Eliminar',
                textoCancelar: 'Cancelar',
                tipo: 'danger'
            }
        });

        dialogRef.afterClosed().subscribe((confirmado: boolean) => {
            if (!confirmado) return;
            this.service.eliminar(id).subscribe({
                next: () => this.cargar(),
                error: () => alert('No tiene permiso para eliminar este registro (ya no está en estado Reportado)')
            });
        });
    }

    urlToBase64(url: string): Promise<string> {
        return new Promise((resolve) => {
            const img = new Image();
            img.crossOrigin = 'anonymous';
            img.onload = () => {
                const canvas = document.createElement('canvas');
                canvas.width = img.width; canvas.height = img.height;
                canvas.getContext('2d')!.drawImage(img, 0, 0);
                resolve(canvas.toDataURL('image/jpeg'));
            };
            img.onerror = (e) => { console.error('Error cargando imagen para PDF:', url, e); resolve(''); };
            img.src = url + '?t=' + Date.now();
        });
    }

    async exportarPDF() {
        const r = this.registroSeleccionado;
        if (!r) return;

        const doc = new jsPDF('p', 'mm', 'letter');
        const NEGRO: [number, number, number] = [0, 0, 0];
        const GRIS: [number, number, number] = [225, 225, 225];
        const W = 216; const M = 10;
        let y = M;

        doc.setDrawColor(0); doc.setLineWidth(0.4);
        doc.rect(M, y, W - M * 2, 20);
        doc.setFontSize(11); doc.setTextColor(...NEGRO); doc.setFont('helvetica', 'bold');
        doc.text('EMPAQUES PLASTICOS S.A.S', W / 2, y + 7, { align: 'center' });
        doc.setFontSize(7); doc.setFont('helvetica', 'normal');
        doc.text('NIT:816000992-1', W / 2, y + 12, { align: 'center' });
        doc.setFontSize(9); doc.setFont('helvetica', 'bold');
        doc.text('FORMATO: GESTIÓN S.N.C. - SALIDAS NO CONFORMES', W / 2, y + 17, { align: 'center' });
        doc.rect(W - M - 42, y, 42, 20);
        doc.setFontSize(7); doc.setFont('helvetica', 'normal');
        doc.text('Codigo: F-GC-001', W - M - 40, y + 6);
        doc.text('Version: 002', W - M - 40, y + 13);
        doc.text('Fecha: 10/09/2024', W - M - 40, y + 18);
        y += 26;

        doc.setFontSize(10); doc.setFont('helvetica', 'bold');
        doc.text(`Orden de Producción: ${r.ordenProduccion}`, M, y);
        y += 6;

        const fotosBase64: { [key: number]: string } = {};
        for (const ev of this.evidenciasCargadas) {
            fotosBase64[ev.id] = await this.urlToBase64(this.urlEvidencia(ev.nombreArchivo));
        }

        const agregarFotosDelPaso = (paso: string) => {
            const fotos = this.evidenciasDePaso(paso);
            if (fotos.length === 0) return;
            if (y > 230) { doc.addPage(); y = M; }
            doc.setFontSize(8); doc.setFont('helvetica', 'bold');
            doc.text(`Evidencia — ${paso}`, M, y);
            y += 4;
            let x = M;
            for (const ev of fotos) {
                const b64 = fotosBase64[ev.id];
                if (b64) { try { doc.addImage(b64, 'JPEG', x, y, 35, 28); } catch (e) { } }
                x += 38;
                if (x + 35 > W - M) { x = M; y += 32; }
            }
            y += 34;
        };

        autoTable(doc, {
            startY: y,
            head: [['Paso 1 — Reportar', '']],
            body: [
                ['Fecha', new Date(r.fechaReporte).toLocaleString('es-CO')],
                ['Cliente', r.cliente || '-'],
                ['Referencia', r.referencia || '-'],
                ['Línea', r.linea || '-'],
                ['Material', r.material || '-'],
                ['Proceso', r.proceso || '-'],
                ['Tipo de defecto', r.tipoDefecto || '-'],
                ['Impacto', r.impacto || '-'],
                ['Descripción', r.descripcionSalida || '-'],
                ['Causa raíz', r.causaRaiz || '-'],
                ['Cantidad reportada', `${r.cantidadReportadaKg ?? '-'} ${r.unidadCantidadReportada || ''}`],
                ['Reportado por', r.nombreReporta || r.usuarioReporta || '-'],
            ],
            headStyles: { fillColor: GRIS, textColor: NEGRO, fontSize: 8, lineColor: NEGRO, lineWidth: 0.3 },
            bodyStyles: { fontSize: 8, textColor: NEGRO, lineColor: NEGRO, lineWidth: 0.3 },
            columnStyles: { 0: { fontStyle: 'bold', cellWidth: 55 } },
            margin: { left: M, right: M }
        });
        y = (doc as any).lastAutoTable.finalY + 3;
        if (r.firmaReporta) {
            if (r.firmaReporta.startsWith('data:image')) {
                try { doc.addImage(r.firmaReporta, 'PNG', M, y, 45, 15); } catch (e) { }
            } else {
                doc.setFont('helvetica', 'italic'); doc.setFontSize(14);
                doc.text(r.firmaReporta, M, y + 10);
                doc.setFont('helvetica', 'normal'); doc.setFontSize(8);
            }
        }
        y += 18;
        agregarFotosDelPaso('Reportar');

        if (r.tratamientoAdoptado || r.descripcionTratamiento) {
            if (y > 200) { doc.addPage(); y = M; }
            autoTable(doc, {
                startY: y,
                head: [['Paso 2 — Tratamiento', '']],
                body: [
                    ['Tratamiento adoptado', r.tratamientoAdoptado || '-'],
                    ['Descripción', r.descripcionTratamiento || '-'],
                    ['Fecha', r.fechaTratamiento ? new Date(r.fechaTratamiento).toLocaleDateString('es-CO') : '-'],
                    ['Registrado por', r.usuarioTratamiento || '-'],
                ],
                headStyles: { fillColor: GRIS, textColor: NEGRO, fontSize: 8, lineColor: NEGRO, lineWidth: 0.3 },
                bodyStyles: { fontSize: 8, textColor: NEGRO, lineColor: NEGRO, lineWidth: 0.3 },
                columnStyles: { 0: { fontStyle: 'bold', cellWidth: 55 } },
                margin: { left: M, right: M }
            });
            y = (doc as any).lastAutoTable.finalY + 3;
            if (r.firmaTratamiento) {
                if (r.firmaTratamiento.startsWith('data:image')) {
                    try { doc.addImage(r.firmaTratamiento, 'PNG', M, y, 45, 15); } catch (e) { }
                } else {
                    doc.setFont('helvetica', 'italic'); doc.setFontSize(14);
                    doc.text(r.firmaTratamiento, M, y + 10);
                    doc.setFont('helvetica', 'normal'); doc.setFontSize(8);
                }
            }
            y += 18;
            agregarFotosDelPaso('Tratamiento');
        }

        if (r.estado === 'Cerrado') {
            if (y > 200) { doc.addPage(); y = M; }
            autoTable(doc, {
                startY: y,
                head: [['Paso 3 — Verificación / Cierre', '']],
                body: [
                    ['Verificación de cumplimiento', r.verificacionCumplimiento || '-'],
                    ['¿Requirió información al cliente?', r.requiereInformacionCliente ? 'SI' : 'NO'],
                    ['Motivo', r.motivoInformacionCliente || '-'],
                    ['Aceptación bajo concesión', r.aceptacionBajoConcesion ? 'SI' : 'NO'],
                    ['Detalle', r.detalleAceptacionConcesion || '-'],
                    ['Revisado por', r.revisadoPor || '-'],
                    ['Fecha de verificación', r.fechaVerificacion ? new Date(r.fechaVerificacion).toLocaleDateString('es-CO') : '-'],
                ],
                headStyles: { fillColor: GRIS, textColor: NEGRO, fontSize: 8, lineColor: NEGRO, lineWidth: 0.3 },
                bodyStyles: { fontSize: 8, textColor: NEGRO, lineColor: NEGRO, lineWidth: 0.3 },
                columnStyles: { 0: { fontStyle: 'bold', cellWidth: 55 } },
                margin: { left: M, right: M }
            });
            y = (doc as any).lastAutoTable.finalY + 3;
            if (r.firmaVerificacion) {
                if (r.firmaVerificacion.startsWith('data:image')) {
                    try { doc.addImage(r.firmaVerificacion, 'PNG', M, y, 45, 15); } catch (e) { }
                } else {
                    doc.setFont('helvetica', 'italic'); doc.setFontSize(14);
                    doc.text(r.firmaVerificacion, M, y + 10);
                    doc.setFont('helvetica', 'normal'); doc.setFontSize(8);
                }
            }
            y += 18;
            agregarFotosDelPaso('Verificacion');
        }

        if (r.estado === 'Cerrado') {
            doc.setFontSize(9); doc.setFont('helvetica', 'italic'); doc.setTextColor(...NEGRO);
            doc.text('— FIN DE DOCUMENTO —', W / 2, y, { align: 'center' });
        }

        doc.save(`SNC_${r.ordenProduccion}_${new Date().toISOString().slice(0, 10)}.pdf`);
    }
}