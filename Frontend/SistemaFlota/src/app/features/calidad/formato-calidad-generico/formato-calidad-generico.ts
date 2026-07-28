import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { FormatosCalidadService } from '../../../core/services/formatos-calidad.service';
import { OrdenesProduccionService } from '../../../core/services/ordenes-produccion.service';
import { PermisosService } from '../../../core/services/permisos.service';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

@Component({
    selector: 'app-formato-calidad-generico',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './formato-calidad-generico.html',
    styleUrls: ['./formato-calidad-generico.scss']
})
export class FormatoCalidadGenericoComponent implements OnInit {
    // ── Configuración del tipo de formato (viene de la ruta) ──
    codigoFormato: string = '';
    tipoFormato: any = null;
    caracteristicas: any[] = [];

    // ── Estado de la vista ──
    vista: 'lista' | 'nuevo' | 'ver' | 'editar' | 'liberar' = 'lista';
    cargando = false;
    registros: any[] = [];
    registroSeleccionado: any = null;
    editandoId: number | null = null;

    // ── Filtros ──
    filtroDesde = '';
    filtroHasta = '';
    filtroOP = '';

    // ── Formulario ──
    form = {
        ordenProduccion: '',
        cliente: '',
        referencia: '',
        operarios: '',
        hora: '',
        maquina: '',
        puedeLiberarse: null as boolean | null,
        explicacionNoLiberado: '',
        cargoFirma: ''
    };

    // Resultados por característica: { [caracteristicaId]: { cumple: number|null, noCumple: number|null, na: boolean, observacion: string } }
    resultados: { [key: number]: { cumple: number | null, noCumple: number | null, na: boolean, observacion: string } } = {};

    // Variables críticas (solo si tipoFormato.tieneVariablesCriticas)
    variablesCriticas: any = {
        corona: '', molde: '',
        temperaturas: { zona1: '', zona2: '', zona3: '', zona4: '', zona5: '', zona6: '' },
        velocidades: { maquina: '', halador: '', bobinador: '' },
        aire: '', amperaje: '', alturaBurbuja: '', produccionKgHora: ''
    };

    firmaDataUrl: string | null = null;
    opExistente: any = null;
    clienteDetectado: string | null = null;
    referenciaDetectada: string | null = null;

    get usuario(): string {
        const u = JSON.parse(sessionStorage.getItem('user') || '{}');
        return u.username ?? '';
    }

    get puedeCrear(): boolean { return this.permisosService.puedeCrear('calidad-formatos'); }
    get puedeEliminar(): boolean { return this.permisosService.puedeEliminar('calidad-formatos'); }

    constructor(
        private service: FormatosCalidadService,
        private ordenesService: OrdenesProduccionService,
        private permisosService: PermisosService,
        private route: ActivatedRoute
    ) { }

    ngOnInit(): void {
        this.codigoFormato = this.route.snapshot.data['codigo'];
        this.cargarConfiguracion();
    }

    cargarConfiguracion() {
        this.service.getCaracteristicas(this.codigoFormato).subscribe({
            next: (data) => {
                this.tipoFormato = data;
                this.caracteristicas = data.caracteristicas;
                this.inicializarResultados();
                this.cargar();
            },
            error: (e) => console.error('Error cargando configuracion', e)
        });
    }

    inicializarResultados() {
        this.resultados = {};
        for (const c of this.caracteristicas) {
            this.resultados[c.id] = { cumple: null, noCumple: null, na: false, observacion: '' };
        }
    }

    cargar() {
        this.cargando = true;
        this.service.getRegistros(this.codigoFormato, this.filtroDesde, this.filtroHasta, this.filtroOP).subscribe({
            next: (d) => { this.registros = d; this.cargando = false; },
            error: (e) => { console.error(e); this.cargando = false; }
        });
    }
    buscarOP() {
        if (!this.form.ordenProduccion || !this.tipoFormato) return;
        this.service.buscarOP(this.form.ordenProduccion, this.tipoFormato.id).subscribe({
            next: () => { this.opExistente = true; },
            error: () => {
                this.opExistente = null;
                this.ordenesService.buscar(this.form.ordenProduccion).subscribe({
                    next: (data: any) => {
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

    nuevo() {
        this.form = {
            ordenProduccion: '', cliente: '', referencia: '', operarios: '',
            hora: '', maquina: '', puedeLiberarse: null, explicacionNoLiberado: '', cargoFirma: ''
        };
        this.inicializarResultados();
        this.variablesCriticas = {
            corona: '', molde: '',
            temperaturas: { zona1: '', zona2: '', zona3: '', zona4: '', zona5: '', zona6: '' },
            velocidades: { maquina: '', halador: '', bobinador: '' },
            aire: '', amperaje: '', alturaBurbuja: '', produccionKgHora: ''
        };
        this.firmaDataUrl = null;
        this.opExistente = null;
        this.clienteDetectado = null;
        this.editandoId = null;
        this.vista = 'nuevo';
        setTimeout(() => this.iniciarCanvas(), 300);
    }

    iniciarCanvas() {
        const canvas = document.getElementById('firmaCanvasGenerico') as HTMLCanvasElement;
        if (!canvas) return;
        const ctx = canvas.getContext('2d')!;
        let dibujando = false;
        canvas.addEventListener('mousedown', e => { dibujando = true; ctx.beginPath(); ctx.moveTo(e.offsetX, e.offsetY); });
        canvas.addEventListener('mousemove', e => { if (!dibujando) return; ctx.lineTo(e.offsetX, e.offsetY); ctx.stroke(); });
        canvas.addEventListener('mouseup', () => { dibujando = false; this.firmaDataUrl = canvas.toDataURL(); });
        canvas.addEventListener('touchstart', e => { e.preventDefault(); dibujando = true; const t = e.touches[0]; const r = canvas.getBoundingClientRect(); ctx.beginPath(); ctx.moveTo(t.clientX - r.left, t.clientY - r.top); });
        canvas.addEventListener('touchmove', e => { e.preventDefault(); if (!dibujando) return; const t = e.touches[0]; const r = canvas.getBoundingClientRect(); ctx.lineTo(t.clientX - r.left, t.clientY - r.top); ctx.stroke(); });
        canvas.addEventListener('touchend', () => { dibujando = false; this.firmaDataUrl = canvas.toDataURL(); });
    }

    limpiarFirma() {
        const canvas = document.getElementById('firmaCanvasGenerico') as HTMLCanvasElement;
        if (canvas) canvas.getContext('2d')!.clearRect(0, 0, canvas.width, canvas.height);
        this.firmaDataUrl = null;
    }

   guardar() {
    if (!this.form.ordenProduccion.trim()) { alert('Ingrese la orden de produccion'); return; }

    const resultadosArray = this.caracteristicas.map(c => ({
      caracteristicaId: c.id,
      descripcion: c.descripcion,
      cumple: this.resultados[c.id]?.cumple ?? null,
      noCumple: this.resultados[c.id]?.noCumple ?? null,
      na: this.resultados[c.id]?.na ?? false,
      observacion: this.resultados[c.id]?.observacion ?? ''
    }));

    const dto = {
      tipoFormatoId: this.tipoFormato.id,
      ordenProduccion: this.form.ordenProduccion,
      cliente: this.form.cliente,
      referencia: this.form.referencia,
      operarios: this.form.operarios,
      hora: this.form.hora,
      maquina: this.form.maquina,
      variablesCriticasJson: this.tipoFormato.tieneVariablesCriticas ? JSON.stringify(this.variablesCriticas) : null,
      resultadosJson: JSON.stringify(resultadosArray)
    };

    const peticion = this.editandoId
      ? this.service.editarRegistro(this.editandoId, dto)
      : this.service.crearRegistro(dto);

    peticion.subscribe({
      next: () => { this.vista = 'lista'; this.editandoId = null; this.cargar(); },
      error: (e) => { console.error(e); alert('Error guardando el registro'); }
    });
  }

  guardarLiberacion() {
    if (this.form.puedeLiberarse === null) { alert('Seleccione SI o NO'); return; }

    const dto = {
      puedeLiberarse: this.form.puedeLiberarse,
      explicacionNoLiberado: this.form.explicacionNoLiberado,
      firmaDigital: this.firmaDataUrl,
      cargoFirma: this.form.cargoFirma
    };

    this.service.liberarRegistro(this.editandoId!, dto).subscribe({
      next: () => { this.vista = 'lista'; this.editandoId = null; this.cargar(); },
      error: (e) => { console.error(e); alert('Error al liberar el registro'); }
    });
  }

    ver(r: any) {
        this.registroSeleccionado = r;
        this.registroSeleccionado.resultadosParsed = JSON.parse(r.resultadosJson || '[]');
        if (r.variablesCriticasJson) this.registroSeleccionado.variablesCriticasParsed = JSON.parse(r.variablesCriticasJson);
        this.vista = 'ver';
    }

    editar(r: any) {
        this.editandoId = r.id;
        this.form = {
            ordenProduccion: r.ordenProduccion, cliente: r.cliente ?? '', referencia: r.referencia ?? '',
            operarios: r.operarios ?? '', hora: r.hora ?? '', maquina: r.maquina ?? '',
            puedeLiberarse: r.puedeLiberarse, explicacionNoLiberado: r.explicacionNoLiberado ?? '',
            cargoFirma: r.cargoFirma ?? ''
        };

        this.inicializarResultados();
        const resultadosGuardados = JSON.parse(r.resultadosJson || '[]');
        for (const res of resultadosGuardados) {
            if (this.resultados[res.caracteristicaId]) {
                this.resultados[res.caracteristicaId] = {
                    cumple: res.cumple, noCumple: res.noCumple, na: res.na, observacion: res.observacion
                };
            }
        }

        if (r.variablesCriticasJson) this.variablesCriticas = JSON.parse(r.variablesCriticasJson);
        this.firmaDataUrl = r.firmaDigital ?? null;
        this.vista = 'editar';
        setTimeout(() => this.iniciarCanvas(), 300);
    }

    abrirLiberar(r: any) {
        this.editandoId = r.id;
        this.registroSeleccionado = r;
        this.form.puedeLiberarse = r.puedeLiberarse ?? null;
        this.form.explicacionNoLiberado = r.explicacionNoLiberado ?? '';
        this.form.cargoFirma = r.cargoFirma ?? '';
        this.firmaDataUrl = r.firmaDigital ?? null;
        this.vista = 'liberar';
        setTimeout(() => this.iniciarCanvas(), 300);
    }
    eliminar(id: number) {
        if (!confirm('¿Eliminar este registro?')) return;
        this.service.eliminarRegistro(id).subscribe({ next: () => this.cargar() });
    }
    exportarPDF() {
        if (this.registros.length === 0) { alert('No hay registros para exportar'); return; }

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
        doc.text(`FORMATO: CONTROL Y LIBERACION DE PRODUCTO - ${this.tipoFormato?.nombre?.toUpperCase()}`, W / 2, y + 17, { align: 'center' });
        doc.rect(W - M - 42, y, 42, 20);
        doc.setFontSize(7); doc.setFont('helvetica', 'normal');
        doc.text('Codigo: ' + this.codigoFormato, W - M - 40, y + 6);
        doc.text('Version: 001', W - M - 40, y + 13);
        doc.text('Fecha: 15/09/2024', W - M - 40, y + 18);
        y += 24;

        doc.setFontSize(7); doc.setFont('helvetica', 'italic');
        doc.rect(M, y, W - M * 2, 12);
        doc.text('Nota: Las verificaciones consisten en comparar los datos obtenidos con los instrumentos de medición e inspecciones con los criterios de calidad y las especificaciones dadas en la Orden de producción adjunta a este formato, con el fin de comprobar que se cumplan.', M + 2, y + 3.5, { maxWidth: W - M * 2 - 4 });
        doc.text('Nota: Las verificaciones se registran cada 10% del avance en la ejecución de la orden de Producción y una al final.', M + 2, y + 9.5, { maxWidth: W - M * 2 - 4 });
        y += 15;

        for (const r of this.registros) {
            if (y > 240) { doc.addPage(); y = M; }

            doc.setFontSize(9); doc.setFont('helvetica', 'bold');
            doc.text(`Orden de Producción: ${r.ordenProduccion}`, M, y + 5);
            doc.setFont('helvetica', 'normal'); doc.setFontSize(8);
            doc.text(`Fecha: ${new Date(r.fecha).toLocaleDateString('es-CO')}   Cliente: ${r.cliente ?? '-'}   Operarios: ${r.operarios ?? '-'}   Máquina: ${r.maquina ?? '-'}   Hora: ${r.hora ?? '-'}`, M, y + 10);
            y += 14;

            if (r.variablesCriticasJson) {
                const vc = JSON.parse(r.variablesCriticasJson);
                doc.setFontSize(8); doc.setFont('helvetica', 'bold');
                doc.text('REGISTRO VARIABLES CRITICAS', M, y);
                y += 5;

                autoTable(doc, {
                    startY: y,
                    body: [
                        ['Corona', vc.corona || '-', 'Molde', vc.molde || '-'],
                    ],
                    theme: 'grid',
                    bodyStyles: { fontSize: 7, lineColor: NEGRO, lineWidth: 0.3 },
                    margin: { left: M, right: M }
                });
                y = (doc as any).lastAutoTable.finalY + 3;

                autoTable(doc, {
                    startY: y,
                    head: [['Temperaturas Máquina (°C)', '', 'Velocidades Máquina (Hz)', '']],
                    body: [
                        ['Zona 1', vc.temperaturas?.zona1 || '-', 'Máquina', vc.velocidades?.maquina || '-'],
                        ['Zona 2', vc.temperaturas?.zona2 || '-', 'Halador', vc.velocidades?.halador || '-'],
                        ['Zona 3', vc.temperaturas?.zona3 || '-', 'Bobinador', vc.velocidades?.bobinador || '-'],
                        ['Zona 4', vc.temperaturas?.zona4 || '-', '', ''],
                        ['Zona 5', vc.temperaturas?.zona5 || '-', '', ''],
                        ['Zona 6', vc.temperaturas?.zona6 || '-', '', ''],
                    ],
                    headStyles: { fillColor: GRIS, textColor: NEGRO, fontSize: 7, lineColor: NEGRO, lineWidth: 0.3 },
                    bodyStyles: { fontSize: 7, lineColor: NEGRO, lineWidth: 0.3 },
                    margin: { left: M, right: M }
                });
                y = (doc as any).lastAutoTable.finalY + 3;

                autoTable(doc, {
                    startY: y,
                    body: [
                        ['Aire (H)', vc.aire || '-', 'Amperaje (A)', vc.amperaje || '-', 'Altura Burbuja (Cm)', vc.alturaBurbuja || '-', 'Producción (Kg/h)', vc.produccionKgHora || '-'],
                    ],
                    theme: 'grid',
                    bodyStyles: { fontSize: 6.5, lineColor: NEGRO, lineWidth: 0.3 },
                    margin: { left: M, right: M }
                });
                y = (doc as any).lastAutoTable.finalY + 5;

                if (y > 240) { doc.addPage(); y = M; }
            }

            const resultados = JSON.parse(r.resultadosJson || '[]');
            const filas = resultados.map((res: any) => [
                res.descripcion,
                res.cumple ? res.cumple + '%' : '-',
                res.noCumple ? res.noCumple + '%' : '-',
                res.na ? 'X' : '-',
                res.observacion || '-'
            ]);

            autoTable(doc, {
                startY: y,
                head: [['Característica', 'Cumple', 'No Cumple', 'N/A', 'Observaciones']],
                body: filas,
                headStyles: { fillColor: GRIS, textColor: NEGRO, fontSize: 7, lineColor: NEGRO, lineWidth: 0.3 },
                bodyStyles: { fontSize: 7, lineColor: NEGRO, lineWidth: 0.3 },
                margin: { left: M, right: M }
            });
            y = (doc as any).lastAutoTable.finalY + 4;

            if (y > 250) { doc.addPage(); y = M; }
            doc.setFontSize(8); doc.setFont('helvetica', 'bold');
            const liberado = r.puedeLiberarse === true ? 'SI' : (r.puedeLiberarse === false ? 'NO' : '-');
            doc.text(`¿Puede ser liberado? ${liberado}`, M, y + 4);
            if (r.explicacionNoLiberado) {
                doc.setFont('helvetica', 'normal');
                doc.text(`Motivo: ${r.explicacionNoLiberado}`, M, y + 9, { maxWidth: W - M * 2 });
                y += 5;
            }
            doc.text(`Revisado por: ${r.revisadoPor || '-'}   Cargo: ${r.cargoFirma || '-'}`, M, y + 9);
            y += 16;

            doc.setDrawColor(200); doc.setLineWidth(0.2);
            doc.line(M, y, W - M, y);
            y += 6;
        }

        if (y > 250) { doc.addPage(); y = M; }
        doc.setFontSize(7); doc.setFont('helvetica', 'italic'); doc.setTextColor(...NEGRO);
        doc.text(`Nota: Los métodos de verificación de las características y los criterios de cumplimiento a tener en cuenta, están definidos en el "Procedimiento de Pruebas y Ensayos: ${this.tipoFormato?.nombre}" e "Instructivos para Pruebas y Ensayos"`, M, y + 3, { maxWidth: W - M * 2 });
        y += 10;

        if (y > 260) { doc.addPage(); y = M; }
        doc.setFont('helvetica', 'bold'); doc.setFontSize(10);
        doc.text('FIN DEL DOCUMENTO', W / 2, y + 4, { align: 'center' });
        doc.save(`${this.codigoFormato}_${new Date().toISOString().slice(0, 10)}.pdf`);
    }
}

