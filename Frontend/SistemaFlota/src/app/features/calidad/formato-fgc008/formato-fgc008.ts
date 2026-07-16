import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import { environment } from '../../../../environments/environment';
import { FormatoFGC008Service } from '../../../core/services/formato-fgc008.service';
import { PermisosService } from '../../../core/services/permisos.service';

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
  registros: any[] = [];
  baseUrl = environment.fotosUrl;
  filtroDesde = '';
  filtroHasta = '';
  filtroOP = '';
  form = {
    ordenProduccion: '', etiquetasSI: false, embalajeSI: false,
    defectosSI: false, cantidadOP: 0, cantidadReal: 0,
    listoBodega: false, despachado: '', accionesTomadas: ''
  };
  fotoEvidencia: File | null = null;
  firmaDataUrl: string | null = null;
  cargoFirma: string = 'Bodega';
  logoUrl: string = '';
  opExistente: any = null;
  despachado: boolean = false;

  get puedeVer(): boolean { return this.permisosService.puedeVer('calidad-formatos'); }
  get puedeCrear(): boolean { return this.permisosService.puedeCrear('calidad-formatos'); }
  get puedeEliminar(): boolean { return this.permisosService.puedeEliminar('calidad-formatos'); }
  get usuario(): string {
    const u = JSON.parse(sessionStorage.getItem('user') || '{}');
    return u.username ?? '';
  }

  constructor(private service: FormatoFGC008Service, private permisosService: PermisosService) {}

  ngOnInit(): void { this.cargar(); this.cargarLogo(); }

  cargar() {
    this.service.getRegistros(this.filtroDesde, this.filtroHasta, this.filtroOP).subscribe({
      next: d => this.registros = d,
      error: e => console.error(e)
    });
  }

  cargarLogo() {
    const config = JSON.parse(sessionStorage.getItem('configuracion') || '{}');
    if (config.logo) this.logoUrl = this.baseUrl + '/config/' + config.logo;
  }

  iniciarCanvas() {
    setTimeout(() => {
      const canvas = document.getElementById('firmaCanvas') as HTMLCanvasElement;
      if (!canvas) return;
      const ctx = canvas.getContext('2d')!;
      let dibujando = false;
      canvas.addEventListener('mousedown', e => { dibujando = true; ctx.beginPath(); ctx.moveTo(e.offsetX, e.offsetY); });
      canvas.addEventListener('mousemove', e => { if (!dibujando) return; ctx.lineTo(e.offsetX, e.offsetY); ctx.stroke(); });
      canvas.addEventListener('mouseup', () => { dibujando = false; this.firmaDataUrl = canvas.toDataURL(); });
      canvas.addEventListener('touchstart', e => { e.preventDefault(); dibujando = true; const t = e.touches[0]; const r = canvas.getBoundingClientRect(); ctx.beginPath(); ctx.moveTo(t.clientX-r.left, t.clientY-r.top); });
      canvas.addEventListener('touchmove', e => { e.preventDefault(); if (!dibujando) return; const t = e.touches[0]; const r = canvas.getBoundingClientRect(); ctx.lineTo(t.clientX-r.left, t.clientY-r.top); ctx.stroke(); });
      canvas.addEventListener('touchend', () => { dibujando = false; this.firmaDataUrl = (document.getElementById('firmaCanvas') as HTMLCanvasElement).toDataURL(); });
    }, 100);
  }

  limpiarFirma() {
    const canvas = document.getElementById('firmaCanvas') as HTMLCanvasElement;
    if (canvas) canvas.getContext('2d')!.clearRect(0, 0, canvas.width, canvas.height);
    this.firmaDataUrl = null;
  }

  buscarOP() {
    if (!this.form.ordenProduccion) return;
    this.service.buscarOP(this.form.ordenProduccion).subscribe({
      next: (data) => { this.opExistente = data; this.form.cantidadOP = data.cantidadOP; },
      error: () => { this.opExistente = null; }
    });
  }

  seleccionarFoto(event: any) { this.fotoEvidencia = event.target.files[0]; }

  guardar() {
    if (!this.form.ordenProduccion) { alert('Ingrese la orden de produccion'); return; }
    const fd = new FormData();
    fd.append('OrdenProduccion', this.form.ordenProduccion);
    fd.append('EtiquetasSI', this.form.etiquetasSI.toString());
    fd.append('EmbalajeSI', this.form.embalajeSI.toString());
    fd.append('DefectosSI', this.form.defectosSI.toString());
    fd.append('CantidadOP', this.form.cantidadOP.toString());
    fd.append('CantidadReal', this.form.cantidadReal.toString());
    fd.append('ListoBodega', this.form.listoBodega.toString());
    if (this.form.despachado) fd.append('Despachado', this.form.despachado);
    if (this.form.accionesTomadas) fd.append('AccionesTomadas', this.form.accionesTomadas);
    if (this.fotoEvidencia) fd.append('foto', this.fotoEvidencia);
    this.service.crearRegistro(fd).subscribe({
      next: () => {
        this.vista = 'lista';
        this.form = { ordenProduccion:'', etiquetasSI:false, embalajeSI:false, defectosSI:false, cantidadOP:0, cantidadReal:0, listoBodega:false, despachado:'', accionesTomadas:'' };
        this.fotoEvidencia = null; this.opExistente = null; this.despachado = false;
        this.cargar();
      },
      error: e => { console.error(e); alert('Error guardando registro'); }
    });
  }

  ver(r: any) { this.registroSeleccionado = r; this.vista = 'ver'; }

  editar(r: any) {
    this.editandoId = r.id;
    this.form = {
      ordenProduccion: r.ordenProduccion, etiquetasSI: r.etiquetasSI,
      embalajeSI: r.embalajeSI, defectosSI: r.defectosSI,
      cantidadOP: r.cantidadOP, cantidadReal: r.cantidadReal,
      listoBodega: r.listoBodega, despachado: r.despachado ?? '',
      accionesTomadas: r.accionesTomadas ?? ''
    };
    this.vista = 'editar';
  }

  guardarEdicion() {
    if (!this.form.ordenProduccion) { alert('Ingrese la orden de produccion'); return; }
    const fd = new FormData();
    fd.append('OrdenProduccion', this.form.ordenProduccion);
    fd.append('EtiquetasSI', this.form.etiquetasSI.toString());
    fd.append('EmbalajeSI', this.form.embalajeSI.toString());
    fd.append('DefectosSI', this.form.defectosSI.toString());
    fd.append('CantidadOP', this.form.cantidadOP.toString());
    fd.append('CantidadReal', this.form.cantidadReal.toString());
    fd.append('ListoBodega', this.form.listoBodega.toString());
    if (this.form.despachado) fd.append('Despachado', this.form.despachado);
    if (this.form.accionesTomadas) fd.append('AccionesTomadas', this.form.accionesTomadas);
    if (this.fotoEvidencia) fd.append('foto', this.fotoEvidencia);
    this.service.editarRegistro(this.editandoId!, fd).subscribe({
      next: () => { this.vista = 'lista'; this.editandoId = null; this.cargar(); },
      error: e => { console.error(e); alert('Error editando registro'); }
    });
  }

  eliminar(id: number) {
    if (!confirm('Eliminar este registro?')) return;
    this.service.eliminarRegistro(id).subscribe({ next: () => this.cargar() });
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
      img.onerror = () => resolve('');
      img.src = url + '?t=' + Date.now();
    });
  }

  async exportarPDF() {
    const doc = new jsPDF('p', 'mm', 'letter');
    const VERDE: [number,number,number] = [26,127,90];
    const VERDECLARO: [number,number,number] = [45,158,107];
    const NEGRO: [number,number,number] = [0,0,0];
    const GRIS: [number,number,number] = [232,245,233];
    const W = 216; const M = 10;

    const fotosBase64: {[key: string]: string} = {};
    for (const r of this.registros) {
      if (r.fotoEvidencia) {
        fotosBase64[r.fotoEvidencia] = await this.urlToBase64(this.baseUrl+'/formatos/'+r.fotoEvidencia);
      }
    }

    const dibujarEncabezado = (yy: number): number => {
      doc.setDrawColor(0); doc.setLineWidth(0.3);
      doc.rect(M, yy, W-M*2, 24);
      doc.rect(M, yy, 24, 24);
      if (this.logoUrl) { try { doc.addImage(this.logoUrl, 'PNG', M+1, yy+1, 22, 22); } catch(e) {} }
      doc.setFontSize(10); doc.setTextColor(...NEGRO); doc.setFont('helvetica','bold');
      doc.text('EMPAQUES PLASTICOS S.A.S', W/2+5, yy+8, {align:'center'});
      doc.setFontSize(7); doc.setFont('helvetica','normal');
      doc.text('NIT:816000992-1', W/2+5, yy+13, {align:'center'});
      doc.setFontSize(9); doc.setFont('helvetica','bold');
      doc.text('FORMATO: CONTROL BODEGA PRODUCTO TERMINADO', W/2+5, yy+19, {align:'center'});
      doc.rect(W-M-42, yy, 42, 8); doc.rect(W-M-42, yy+8, 42, 8); doc.rect(W-M-42, yy+16, 42, 8);
      doc.setFontSize(7); doc.setFont('helvetica','normal');
      doc.text('Codigo: F-GC-008', W-M-40, yy+5);
      doc.text('Version: 001', W-M-40, yy+13);
      doc.text('Fecha: 15/09/2024', W-M-40, yy+21);
      return yy+24;
    };

    let y = M;
    y = dibujarEncabezado(y);

    doc.setFillColor(240,240,240);
    doc.rect(M, y, 22, 7, 'FD');
    doc.setFont('helvetica','bold'); doc.setFontSize(8); doc.setTextColor(...NEGRO);
    doc.text('FECHA', M+11, y+5, {align:'center'});
    doc.rect(M+22, y, W-M*2-22, 7);
    doc.setFont('helvetica','normal');
    doc.text(new Date().toLocaleDateString('es-CO'), M+25, y+5);
    y += 7;

    doc.setFillColor(248,248,248);
    doc.rect(M, y, W-M*2, 6, 'FD');
    doc.setFont('helvetica','bold'); doc.setFontSize(8);
    doc.text('REGISTRO VERIFICACION DE EMPAQUE', W/2, y+4, {align:'center'});
    y += 6;

    doc.setFontSize(7); doc.setFont('helvetica','italic');
    doc.rect(M, y, W-M*2, 8);
    doc.text('Nota: Las verificaciones consisten en comprobar la informacion del empaque del producto final con los criterios de calidad y la informacion especificada en la orden de produccion.', M+2, y+3.5, {maxWidth: W-M*2-4});
    y += 9;

    const grupos: any = {};
    this.registros.forEach((r:any) => {
      if (!grupos[r.ordenProduccion]) grupos[r.ordenProduccion] = [];
      grupos[r.ordenProduccion].push(r);
    });

    const cw = [24, 18, 8, 8, 8, 8, 8, 8, 16, 14, 12, 8, 8, 24];
    const rowH = 7;

    doc.setFillColor(...VERDE); doc.setTextColor(255,255,255); doc.setFont('helvetica','bold'); doc.setFontSize(5.5);
    let x = M;
    doc.rect(x, y, cw[0], 12, 'FD'); doc.text('ORDEN DE', x+cw[0]/2, y+5, {align:'center'}); doc.text('PRODUCCION', x+cw[0]/2, y+9, {align:'center'}); x+=cw[0];
    doc.rect(x, y, cw[1], 12, 'FD'); doc.text('Fecha', x+cw[1]/2, y+5, {align:'center'}); doc.text('entrada', x+cw[1]/2, y+9, {align:'center'}); x+=cw[1];
    doc.setFillColor(...VERDECLARO);
    doc.rect(x, y, cw[2]+cw[3], 6, 'FD'); doc.text('Etiquetas completas,', x+(cw[2]+cw[3])/2, y+2.5, {align:'center', maxWidth:cw[2]+cw[3]-1}); doc.text('visibles y legibles', x+(cw[2]+cw[3])/2, y+5, {align:'center', maxWidth:cw[2]+cw[3]-1}); x+=cw[2]+cw[3];
    doc.rect(x, y, cw[4]+cw[5], 6, 'FD'); doc.text('Embalaje uniforme,', x+(cw[4]+cw[5])/2, y+2.5, {align:'center', maxWidth:cw[4]+cw[5]-1}); doc.text('sellado y adecuado', x+(cw[4]+cw[5])/2, y+5, {align:'center', maxWidth:cw[4]+cw[5]-1}); x+=cw[4]+cw[5];
    doc.rect(x, y, cw[6]+cw[7], 6, 'FD'); doc.text('Defectos visibles en el', x+(cw[6]+cw[7])/2, y+2.5, {align:'center', maxWidth:cw[6]+cw[7]-1}); doc.text('empaque o producto', x+(cw[6]+cw[7])/2, y+5, {align:'center', maxWidth:cw[6]+cw[7]-1}); x+=cw[6]+cw[7];
    doc.setFillColor(...VERDE);
    doc.rect(x, y, cw[8]+cw[9]+cw[10], 6, 'FD'); doc.text('Concuerda cantidad OP', x+(cw[8]+cw[9]+cw[10])/2, y+2.5, {align:'center', maxWidth:cw[8]+cw[9]+cw[10]-1}); doc.text('con cantidad empacada', x+(cw[8]+cw[9]+cw[10])/2, y+5, {align:'center', maxWidth:cw[8]+cw[9]+cw[10]-1}); x+=cw[8]+cw[9]+cw[10];
    doc.setFillColor(...VERDECLARO);
    doc.rect(x, y, cw[11]+cw[12], 6, 'FD'); doc.text('Listo para', x+(cw[11]+cw[12])/2, y+2.5, {align:'center'}); doc.text('entrada Bodega', x+(cw[11]+cw[12])/2, y+5, {align:'center', maxWidth:cw[11]+cw[12]-1}); x+=cw[11]+cw[12];
    doc.setFillColor(...VERDE);
    doc.rect(x, y, cw[13], 12, 'FD'); doc.text('Despachado', x+cw[13]/2, y+7, {align:'center'});

    x = M+cw[0]+cw[1];
    doc.setFillColor(...VERDECLARO);
    ['SI','NO','SI','NO','SI','NO'].forEach((t,i) => {
      doc.rect(x, y+6, cw[2+i], 6, 'FD'); doc.text(t, x+cw[2+i]/2, y+10, {align:'center'}); x+=cw[2+i];
    });
    doc.setFillColor(...VERDE);
    doc.rect(x, y+6, cw[8], 6, 'FD'); doc.text('Cant. OP', x+cw[8]/2, y+10, {align:'center', maxWidth:cw[8]-1}); x+=cw[8];
    doc.rect(x, y+6, cw[9], 6, 'FD'); doc.text('Cant.', x+cw[9]/2, y+10, {align:'center'}); x+=cw[9];
    doc.rect(x, y+6, cw[10], 6, 'FD'); doc.text('SI/NO', x+cw[10]/2, y+10, {align:'center'}); x+=cw[10];
    doc.setFillColor(...VERDECLARO);
    ['SI','NO'].forEach((t,i) => {
      doc.rect(x, y+6, cw[11+i], 6, 'FD'); doc.text(t, x+cw[11+i]/2, y+10, {align:'center'}); x+=cw[11+i];
    });
    y += 12;

    doc.setTextColor(...NEGRO); doc.setFont('helvetica','normal'); doc.setFontSize(6);

    for (const op of Object.keys(grupos)) {
      const filas = grupos[op];
      let totalReal = 0;
      const cantOP = filas[0].cantidadOP;
      const startY = y;
      if (y > 220) { doc.addPage(); y = M; y = dibujarEncabezado(y); }
      for (let idx = 0; idx < filas.length; idx++) {
        const r = filas[idx];
        if (y > 240) { doc.addPage(); y = M; y = dibujarEncabezado(y); }
        x = M;
        totalReal += r.cantidadReal;
        if (idx === 0) {
          doc.rect(x, startY, cw[0], rowH*filas.length);
          doc.setFont('helvetica','bold');
          doc.text(op, x+cw[0]/2, startY+(rowH*filas.length)/2+1, {align:'center', maxWidth:cw[0]-1});
          doc.setFont('helvetica','normal');
        }
        x += cw[0];
        doc.rect(x, y, cw[1], rowH); doc.text(new Date(r.fecha).toLocaleDateString('es-CO'), x+cw[1]/2, y+4, {align:'center', maxWidth:cw[1]-1}); x+=cw[1];
        doc.rect(x, y, cw[2], rowH); if(r.etiquetasSI) doc.text('X', x+cw[2]/2, y+4, {align:'center'}); x+=cw[2];
        doc.rect(x, y, cw[3], rowH); if(!r.etiquetasSI) doc.text('X', x+cw[3]/2, y+4, {align:'center'}); x+=cw[3];
        doc.rect(x, y, cw[4], rowH); if(r.embalajeSI) doc.text('X', x+cw[4]/2, y+4, {align:'center'}); x+=cw[4];
        doc.rect(x, y, cw[5], rowH); if(!r.embalajeSI) doc.text('X', x+cw[5]/2, y+4, {align:'center'}); x+=cw[5];
        doc.rect(x, y, cw[6], rowH); if(r.defectosSI) doc.text('X', x+cw[6]/2, y+4, {align:'center'}); x+=cw[6];
        doc.rect(x, y, cw[7], rowH); if(!r.defectosSI) doc.text('X', x+cw[7]/2, y+4, {align:'center'}); x+=cw[7];
        if (idx === 0) {
          doc.setFillColor(...GRIS);
          doc.rect(x, startY, cw[8], rowH*filas.length, 'FD');
          doc.setFont('helvetica','bold');
          doc.text(String(cantOP), x+cw[8]/2, startY+(rowH*filas.length)/2+1, {align:'center'});
          doc.setFont('helvetica','normal'); doc.setFillColor(255,255,255);
        }
        x+=cw[8];
        doc.rect(x, y, cw[9], rowH); doc.text(String(r.cantidadReal), x+cw[9]/2, y+4, {align:'center'}); x+=cw[9];
        const sino = r.cantidadReal >= cantOP ? 'SI' : 'NO';
        doc.rect(x, y, cw[10], rowH);
        doc.setTextColor(sino==='SI' ? 26 : 180, sino==='SI' ? 127 : 28, sino==='SI' ? 90 : 28);
        doc.text(sino, x+cw[10]/2, y+4, {align:'center'}); doc.setTextColor(...NEGRO); x+=cw[10];
        doc.rect(x, y, cw[11], rowH); if(r.listoBodega) doc.text('X', x+cw[11]/2, y+4, {align:'center'}); x+=cw[11];
        doc.rect(x, y, cw[12], rowH); if(!r.listoBodega) doc.text('X', x+cw[12]/2, y+4, {align:'center'}); x+=cw[12];
        doc.rect(x, y, cw[13], rowH); doc.text(r.despachado ?? '', x+1, y+4, {maxWidth:cw[13]-2}); x+=cw[13];
        y += rowH;
      }
      const dif = totalReal - cantOP;
      const pct = cantOP > 0 ? ((dif/cantOP)*100).toFixed(1) : '0';
      doc.setFillColor(...GRIS); doc.setFont('helvetica','bold'); doc.setTextColor(...NEGRO);
      x = M;
      const tw = cw[0]+cw[1]+cw[2]+cw[3]+cw[4]+cw[5]+cw[6]+cw[7]+cw[8];
      doc.rect(x, y, tw, 5, 'D'); doc.text('TOTAL '+op+' — '+filas.length+' entradas', x+2, y+3.5); x+=tw;
      doc.rect(x, y, cw[9], 5, 'D'); doc.text(String(totalReal), x+cw[9]/2, y+3.5, {align:'center'}); x+=cw[9];
      doc.rect(x, y, cw[10]+cw[11]+cw[12]+cw[13], 5, 'D');
      doc.setTextColor(dif<0 ? 180 : 26, dif<0 ? 28 : 127, dif<0 ? 28 : 90);
      doc.text('Dif: '+(dif>=0?'+':'')+dif+' uds ('+pct+'%)', x+2, y+3.5);
      doc.setTextColor(...NEGRO); doc.setFont('helvetica','normal');
      y += 8;
    }

    const yFooterStart = y + 3;
    const footerH = 32;
    const firmaH = 22;
    const totalFooter = footerH + firmaH + 20;

    if (yFooterStart + totalFooter > 260) { doc.addPage(); y = M; }
    else { y = yFooterStart; }

    const accionesTexto = this.registros.filter((r:any) => r.accionesTomadas).map((r:any) => r.ordenProduccion+': '+r.accionesTomadas).join(' | ');
    doc.rect(M, y, (W-M*2)*0.58, footerH);
    doc.setFont('helvetica','bold'); doc.setFontSize(7);
    doc.text('Mencione las acciones tomadas para cada OP que incumplio alguna verificacion:', M+2, y+5, {maxWidth:(W-M*2)*0.58-4});
    doc.setFont('helvetica','normal');
    if (accionesTexto) doc.text(accionesTexto, M+2, y+10, {maxWidth:(W-M*2)*0.58-4});
    const fx = M+(W-M*2)*0.60;
    const fw = (W-M*2)*0.40;
    doc.rect(fx, y, fw, footerH);
    doc.setFont('helvetica','bold'); doc.setFontSize(7);
    doc.text('Revisado por:', fx+2, y+5);
    doc.setFont('helvetica','normal');
    doc.text(this.usuario, fx+2, y+10);
    y += footerH+2;

    doc.rect(M, y, (W-M*2)*0.58, firmaH);
    doc.setFont('helvetica','bold'); doc.setFontSize(7);
    doc.text('Firma de quien hace la entrada a Bodega', M+2, y+5);
    if (this.firmaDataUrl) { try { doc.addImage(this.firmaDataUrl, 'PNG', M+2, y+6, 50, 10); } catch(e) {} }
    doc.line(M+2, y+18, M+(W-M*2)*0.58-2, y+18);
    doc.setFont('helvetica','normal'); doc.setFontSize(7);
    doc.text(this.usuario, M+2, y+21);
    doc.rect(fx, y, fw, firmaH);
    doc.setFont('helvetica','bold'); doc.setFontSize(7);
    doc.text('Cargo', fx+2, y+5);
    doc.setFont('helvetica','normal');
    doc.text(this.cargoFirma, fx+2, y+12);
    y += firmaH+3;

    doc.setFontSize(7); doc.setFont('helvetica','italic');
    doc.text('Nota: Los metodos de verificacion de las caracteristicas y los criterios de cumplimiento a tener en cuenta, estan definidos en el Procedimiento de Pruebas y Ensayos.', M, y+3, {maxWidth:W-M*2});
    y += 8;

    const tieneFooter = Object.keys(grupos).some(op => grupos[op].some((r:any) => r.fotoEvidencia));

    if (tieneFooter) {
      const fotosHeight = Object.keys(grupos).reduce((total, op) => {
        const conFoto = grupos[op].filter((r:any) => r.fotoEvidencia);
        if (!conFoto.length) return total;
        const filas = Math.ceil(conFoto.length / 5);
        return total + 7 + filas * 38 + 10;
      }, 0);

      if (y + fotosHeight > 260) {
        doc.setFont('helvetica','bold'); doc.setFontSize(8);
        doc.text('Continua en pagina siguiente — Fotos de evidencia', W/2, y+3, {align:'center'});
        doc.addPage(); y = M; y = dibujarEncabezado(y);
        doc.setFont('helvetica','bold'); doc.setFontSize(9);
        doc.text('FOTOS DE EVIDENCIA — F-GC-008', W/2, y+6, {align:'center'});
        y += 12;
      } else {
        doc.setFont('helvetica','bold'); doc.setFontSize(9);
        doc.text('FOTOS DE EVIDENCIA', W/2, y+6, {align:'center'});
        y += 12;
      }

      for (const op of Object.keys(grupos)) {
        const filas = grupos[op];
        const conFoto = filas.filter((r:any) => r.fotoEvidencia);
        if (!conFoto.length) continue;
        if (y > 240) { doc.addPage(); y = M; }
        doc.setFillColor(...GRIS); doc.setFont('helvetica','bold'); doc.setFontSize(8);
        doc.rect(M, y, W-M*2, 7, 'FD'); doc.setTextColor(...NEGRO);
        doc.text(op+' — '+filas.length+' entradas', M+2, y+5);
        y += 9;
        let fx2 = M;
        for (const r of conFoto) {
          if (fx2+36 > W-M) { fx2 = M; y += 36; }
          if (y > 240) { doc.addPage(); y = M; fx2 = M; }
          const b64 = fotosBase64[r.fotoEvidencia] || '';
          if (b64) { try { doc.addImage(b64, 'JPEG', fx2, y, 32, 26); } catch(e) { doc.rect(fx2, y, 32, 26); } }
          else { doc.rect(fx2, y, 32, 26); doc.setFontSize(6); doc.text('Sin foto', fx2+10, y+14); }
          doc.setFont('helvetica','normal'); doc.setFontSize(6); doc.setTextColor(...NEGRO);
          doc.text(new Date(r.fecha).toLocaleDateString('es-CO'), fx2, y+28);
          fx2 += 36;
        }
        y += 36;
      }
    }

    if (y > 240) { doc.addPage(); y = M; }
    doc.setFontSize(7); doc.setFont('helvetica','italic');
    doc.text('Nota: Los metodos de verificacion de las caracteristicas y los criterios de cumplimiento a tener en cuenta, estan definidos en el Procedimiento de Pruebas y Ensayos.', M, y+3, {maxWidth:W-M*2});
    y += 8;
    doc.setFont('helvetica','bold'); doc.setFontSize(10);
    doc.text('FIN DEL DOCUMENTO', W/2, y+4, {align:'center'});
    doc.save('F-GC-008_'+new Date().toISOString().slice(0,10)+'.pdf');
  }
}