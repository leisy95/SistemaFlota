import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FormatoFGC008Service } from '../../services/formato-fgc008.service';
import { PermisosService } from '../../services/permisos.service';
import { environment } from '../../../environments/environment';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

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
        this.fotoEvidencia = null; this.opExistente = null;
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
    const doc = new jsPDF('p', 'mm', 'a4');
    const VERDE: [number,number,number] = [26,127,90];
    const VERDECLARO: [number,number,number] = [45,158,107];
    const NEGRO: [number,number,number] = [0,0,0];
    const GRIS: [number,number,number] = [232,245,233];
    const W = 210; const M = 8;

    const fotosBase64: {[key: string]: string} = {};
    for (const r of this.registros) {
      if (r.fotoEvidencia) {
        fotosBase64[r.fotoEvidencia] = await this.urlToBase64(this.baseUrl+'/formatos/'+r.fotoEvidencia);
      }
    }

    const dibujarEncabezado = (yy: number): number => {
      doc.setDrawColor(0); doc.setLineWidth(0.3);
      doc.rect(M, yy, W-M*2, 22);
      doc.rect(M, yy, 22, 22);
      if (this.logoUrl) { try { doc.addImage(this.logoUrl, 'PNG', M+1, yy+1, 20, 20); } catch(e) {} }
      doc.setFontSize(9); doc.setTextColor(...NEGRO); doc.setFont('helvetica','bold');
      doc.text('EMPAQUES PLASTICOS S.A.S', W/2+5, yy+7, {align:'center'});
      doc.setFontSize(7); doc.setFont('helvetica','normal');
      doc.text('NIT:816000992-1', W/2+5, yy+11, {align:'center'});
      doc.setFontSize(8); doc.setFont('helvetica','bold');
      doc.text('FORMATO: CONTROL BODEGA PRODUCTO TERMINADO', W/2+5, yy+16, {align:'center'});
      doc.rect(W-M-38, yy, 38, 8); doc.rect(W-M-38, yy+8, 38, 7); doc.rect(W-M-38, yy+15, 38, 7);
      doc.setFontSize(6); doc.setFont('helvetica','normal');
      doc.text('Codigo: F-GC-008', W-M-36, yy+5);
      doc.text('Version: 001', W-M-36, yy+12);
      doc.text('Fecha: 15/09/2024', W-M-36, yy+19);
      return yy+22;
    };

    let y = M;
    y = dibujarEncabezado(y);
    doc.setFillColor(240,240,240);
    doc.rect(M, y, 20, 6, 'FD');
    doc.setFont('helvetica','bold'); doc.setFontSize(7); doc.setTextColor(...NEGRO);
    doc.text('FECHA', M+10, y+4, {align:'center'});
    doc.rect(M+20, y, W-M*2-20, 6);
    doc.setFont('helvetica','normal');
    doc.text(new Date().toLocaleDateString('es-CO'), M+22, y+4);
    y += 6;
    doc.setFillColor(250,250,250);
    doc.rect(M, y, W-M*2, 5, 'FD');
    doc.setFont('helvetica','bold'); doc.setFontSize(7);
    doc.text('REGISTRO VERIFICACION DE EMPAQUE', W/2, y+3.5, {align:'center'});
    y += 5;
    doc.setFontSize(6); doc.setFont('helvetica','italic');
    doc.rect(M, y, W-M*2, 7);
    doc.text('Nota: Las verificaciones consisten en comprobar la informacion del empaque del producto final con los criterios de calidad y la informacion especificada en la orden de produccion.', M+1, y+3, {maxWidth: W-M*2-2});
    y += 8;

    const grupos: any = {};
    this.registros.forEach((r:any) => {
      if (!grupos[r.ordenProduccion]) grupos[r.ordenProduccion] = [];
      grupos[r.ordenProduccion].push(r);
    });

    const cw = [22, 16, 7, 7, 7, 7, 7, 7, 16, 14, 12, 7, 7, 22];
    const rowH = 6;

    doc.setFillColor(...VERDE); doc.setTextColor(255,255,255); doc.setFont('helvetica','bold'); doc.setFontSize(5.5);
    let x = M;
    doc.rect(x, y, cw[0], 10, 'FD'); doc.text('ORDEN PROD.', x+cw[0]/2, y+5, {align:'center', maxWidth:cw[0]-1}); x+=cw[0];
    doc.rect(x, y, cw[1], 10, 'FD'); doc.text('Fecha entrada', x+cw[1]/2, y+5, {align:'center', maxWidth:cw[1]-1}); x+=cw[1];
    doc.setFillColor(...VERDECLARO);
    doc.rect(x, y, cw[2]+cw[3], 5, 'FD'); doc.text('Etiquetas completas', x+(cw[2]+cw[3])/2, y+3.5, {align:'center', maxWidth:cw[2]+cw[3]-1}); x+=cw[2]+cw[3];
    doc.rect(x, y, cw[4]+cw[5], 5, 'FD'); doc.text('Embalaje uniforme', x+(cw[4]+cw[5])/2, y+3.5, {align:'center', maxWidth:cw[4]+cw[5]-1}); x+=cw[4]+cw[5];
    doc.rect(x, y, cw[6]+cw[7], 5, 'FD'); doc.text('Defectos visibles', x+(cw[6]+cw[7])/2, y+3.5, {align:'center', maxWidth:cw[6]+cw[7]-1}); x+=cw[6]+cw[7];
    doc.setFillColor(...VERDE);
    doc.rect(x, y, cw[8], 10, 'FD'); doc.text('Cant. OP', x+cw[8]/2, y+5, {align:'center'}); x+=cw[8];
    doc.rect(x, y, cw[9], 10, 'FD'); doc.text('Cant. Real', x+cw[9]/2, y+5, {align:'center', maxWidth:cw[9]-1}); x+=cw[9];
    doc.rect(x, y, cw[10], 10, 'FD'); doc.text('SI/NO', x+cw[10]/2, y+5, {align:'center'}); x+=cw[10];
    doc.setFillColor(...VERDECLARO);
    doc.rect(x, y, cw[11]+cw[12], 5, 'FD'); doc.text('Listo Bodega', x+(cw[11]+cw[12])/2, y+3.5, {align:'center', maxWidth:cw[11]+cw[12]-1}); x+=cw[11]+cw[12];
    doc.setFillColor(...VERDE);
    doc.rect(x, y, cw[13], 10, 'FD'); doc.text('Despachado', x+cw[13]/2, y+6, {align:'center', maxWidth:cw[13]-1});
    x = M+cw[0]+cw[1];
    doc.setFillColor(...VERDECLARO);
    ['SI','NO','SI','NO','SI','NO'].forEach((t,i) => {
      doc.rect(x, y+5, cw[2+i], 5, 'FD'); doc.text(t, x+cw[2+i]/2, y+8.5, {align:'center'}); x+=cw[2+i];
    });
    x += cw[8]+cw[9]+cw[10];
    ['SI','NO'].forEach((t,i) => {
      doc.rect(x, y+5, cw[11+i], 5, 'FD'); doc.text(t, x+cw[11+i]/2, y+8.5, {align:'center'}); x+=cw[11+i];
    });
    y += 10;

    doc.setTextColor(...NEGRO); doc.setFont('helvetica','normal'); doc.setFontSize(5.5);

    for (const op of Object.keys(grupos)) {
      const filas = grupos[op];
      let totalReal = 0;
      const cantOP = filas[0].cantidadOP;
      const startY = y;
      if (y > 230) { doc.addPage(); y = M; y = dibujarEncabezado(y); }
      for (let idx = 0; idx < filas.length; idx++) {
        const r = filas[idx];
        if (y > 250) { doc.addPage(); y = M; y = dibujarEncabezado(y); }
        x = M;
        totalReal += r.cantidadReal;
        if (idx === 0) {
          doc.rect(x, startY, cw[0], rowH*filas.length);
          doc.setFont('helvetica','bold');
          doc.text(op, x+cw[0]/2, startY+(rowH*filas.length)/2+1, {align:'center', maxWidth:cw[0]-1});
          doc.setFont('helvetica','normal');
        }
        x += cw[0];
        doc.rect(x, y, cw[1], rowH); doc.text(new Date(r.fecha).toLocaleDateString('es-CO'), x+cw[1]/2, y+3.5, {align:'center', maxWidth:cw[1]-1}); x+=cw[1];
        doc.rect(x, y, cw[2], rowH); if(r.etiquetasSI) doc.text('X', x+cw[2]/2, y+3.5, {align:'center'}); x+=cw[2];
        doc.rect(x, y, cw[3], rowH); if(!r.etiquetasSI) doc.text('X', x+cw[3]/2, y+3.5, {align:'center'}); x+=cw[3];
        doc.rect(x, y, cw[4], rowH); if(r.embalajeSI) doc.text('X', x+cw[4]/2, y+3.5, {align:'center'}); x+=cw[4];
        doc.rect(x, y, cw[5], rowH); if(!r.embalajeSI) doc.text('X', x+cw[5]/2, y+3.5, {align:'center'}); x+=cw[5];
        doc.rect(x, y, cw[6], rowH); if(r.defectosSI) doc.text('X', x+cw[6]/2, y+3.5, {align:'center'}); x+=cw[6];
        doc.rect(x, y, cw[7], rowH); if(!r.defectosSI) doc.text('X', x+cw[7]/2, y+3.5, {align:'center'}); x+=cw[7];
        if (idx === 0) {
          doc.setFillColor(...GRIS);
          doc.rect(x, startY, cw[8], rowH*filas.length, 'FD');
          doc.setFont('helvetica','bold');
          doc.text(String(cantOP), x+cw[8]/2, startY+(rowH*filas.length)/2+1, {align:'center'});
          doc.setFont('helvetica','normal'); doc.setFillColor(255,255,255);
        }
        x+=cw[8];
        doc.rect(x, y, cw[9], rowH); doc.text(String(r.cantidadReal), x+cw[9]/2, y+3.5, {align:'center'}); x+=cw[9];
        const sino = r.cantidadReal >= cantOP ? 'SI' : 'NO';
        doc.rect(x, y, cw[10], rowH);
        doc.setTextColor(sino==='SI' ? 26 : 180, sino==='SI' ? 127 : 28, sino==='SI' ? 90 : 28);
        doc.text(sino, x+cw[10]/2, y+3.5, {align:'center'}); doc.setTextColor(...NEGRO); x+=cw[10];
        doc.rect(x, y, cw[11], rowH); if(r.listoBodega) doc.text('X', x+cw[11]/2, y+3.5, {align:'center'}); x+=cw[11];
        doc.rect(x, y, cw[12], rowH); if(!r.listoBodega) doc.text('X', x+cw[12]/2, y+3.5, {align:'center'}); x+=cw[12];
        doc.rect(x, y, cw[13], rowH); doc.text(r.despachado ?? '', x+1, y+3.5, {maxWidth:cw[13]-2}); x+=cw[13];
        y += rowH;
      }
      const dif = totalReal - cantOP;
      const pct = cantOP > 0 ? ((dif/cantOP)*100).toFixed(1) : '0';
      doc.setFillColor(...GRIS); doc.setFont('helvetica','bold'); doc.setTextColor(...NEGRO);
      x = M;
      const tw = cw[0]+cw[1]+cw[2]+cw[3]+cw[4]+cw[5]+cw[6]+cw[7]+cw[8];
      doc.rect(x, y, tw, 5, 'FD'); doc.text('TOTAL '+op+' — '+filas.length+' entradas', x+2, y+3.5); x+=tw;
      doc.rect(x, y, cw[9], 5, 'FD'); doc.text(String(totalReal), x+cw[9]/2, y+3.5, {align:'center'}); x+=cw[9];
      doc.rect(x, y, cw[10]+cw[11]+cw[12]+cw[13], 5, 'FD');
      doc.setTextColor(dif<0 ? 180 : 26, dif<0 ? 28 : 127, dif<0 ? 28 : 90);
      doc.text('Dif: '+(dif>=0?'+':'')+dif+' uds ('+pct+'%)', x+2, y+3.5);
      doc.setTextColor(...NEGRO); doc.setFont('helvetica','normal');
      y += 7;
    }

    if (y > 220) { doc.addPage(); y = M; }
    y += 3;
    const accionesTexto = this.registros.filter((r:any) => r.accionesTomadas).map((r:any) => r.ordenProduccion+': '+r.accionesTomadas).join(' | ');
    const footerH = 30;
    doc.rect(M, y, (W-M*2)*0.6, footerH);
    doc.setFont('helvetica','bold'); doc.setFontSize(6);
    doc.text('Mencione las acciones tomadas para cada OP que incumplio alguna verificacion:', M+1, y+4, {maxWidth:(W-M*2)*0.6-2});
    doc.setFont('helvetica','normal');
    if (accionesTexto) doc.text(accionesTexto, M+1, y+9, {maxWidth:(W-M*2)*0.6-2});
    const fx = M+(W-M*2)*0.62;
    const fw = (W-M*2)*0.38;
    doc.rect(fx, y, fw, footerH);
    doc.setFont('helvetica','bold'); doc.setFontSize(6);
    doc.text('Firma de quien hace la entrada a Bodega', fx+1, y+4, {maxWidth:fw-2});
    if (this.firmaDataUrl) { try { doc.addImage(this.firmaDataUrl, 'PNG', fx+2, y+6, 40, 10); } catch(e) {} }
    doc.line(fx+2, y+22, fx+fw-2, y+22);
    doc.setFont('helvetica','normal'); doc.setFontSize(6);
    doc.text(this.usuario, fx+2, y+25);
    doc.rect(fx, y+footerH-8, fw, 8);
    doc.setFont('helvetica','bold');
    doc.text('Cargo:', fx+1, y+footerH-4);
    doc.setFont('helvetica','normal');
    doc.text(this.cargoFirma, fx+12, y+footerH-4);
    y += footerH+3;
    doc.setFontSize(5.5); doc.setFont('helvetica','italic');
    doc.text('Nota: Los metodos de verificacion de las caracteristicas y los criterios de cumplimiento a tener en cuenta, estan definidos en el Procedimiento de Pruebas y Ensayos.', M, y+3, {maxWidth:W-M*2});
    y += 7;
    doc.setFont('helvetica','bold'); doc.setFontSize(7);
    doc.text('Continua en pagina siguiente — Fotos de evidencia', W/2, y+3, {align:'center'});

    doc.addPage(); y = M; y = dibujarEncabezado(y);
    doc.setFont('helvetica','bold'); doc.setFontSize(8); doc.setTextColor(...NEGRO);
    doc.text('FOTOS DE EVIDENCIA — F-GC-008', W/2, y+5, {align:'center'});
    y += 10;

    for (const op of Object.keys(grupos)) {
      const filas = grupos[op];
      const conFoto = filas.filter((r:any) => r.fotoEvidencia);
      if (!conFoto.length) continue;
      if (y > 260) { doc.addPage(); y = M; }
      doc.setFillColor(...GRIS); doc.setFont('helvetica','bold'); doc.setFontSize(7);
      doc.rect(M, y, W-M*2, 6, 'FD'); doc.setTextColor(...NEGRO);
      doc.text(op+' — '+filas.length+' entradas', M+2, y+4);
      y += 7;
      let fx2 = M;
      for (const r of conFoto) {
        if (fx2+30 > W-M) { fx2 = M; y += 32; }
        if (y > 260) { doc.addPage(); y = M; fx2 = M; }
        const b64 = fotosBase64[r.fotoEvidencia] || '';
        if (b64) { try { doc.addImage(b64, 'JPEG', fx2, y, 28, 22); } catch(e) { doc.rect(fx2, y, 28, 22); } }
        else { doc.rect(fx2, y, 28, 22); doc.setFontSize(5); doc.text('Sin foto', fx2+8, y+12); }
        doc.setFont('helvetica','normal'); doc.setFontSize(5.5); doc.setTextColor(...NEGRO);
        doc.text(new Date(r.fecha).toLocaleDateString('es-CO'), fx2, y+24);
        fx2 += 32;
      }
      y += 34;
    }

    if (y > 260) { doc.addPage(); y = M; }
    doc.setFontSize(5.5); doc.setFont('helvetica','italic');
    doc.text('Nota: Los metodos de verificacion de las caracteristicas y los criterios de cumplimiento a tener en cuenta, estan definidos en el Procedimiento de Pruebas y Ensayos.', M, y+3, {maxWidth:W-M*2});
    y += 8;
    doc.setFont('helvetica','bold'); doc.setFontSize(9);
    doc.text('FIN DEL DOCUMENTO', W/2, y+3, {align:'center'});
    doc.save('F-GC-008_'+new Date().toISOString().slice(0,10)+'.pdf');
  }
}