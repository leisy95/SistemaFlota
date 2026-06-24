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
        this.fotoEvidencia = null;
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

  exportarPDF() {
    const doc = new jsPDF('l', 'mm', 'a4');
    const VERDE: [number,number,number] = [26,127,90];
    const VERDECLARO: [number,number,number] = [45,158,107];
    const NEGRO: [number,number,number] = [0,0,0];
    const GRIS: [number,number,number] = [232,245,233];
    const W = 297; const M = 10;

    doc.setDrawColor(0,0,0); doc.setLineWidth(0.3);
    doc.rect(M, M, W-M*2, 26);
    doc.rect(M, M, 30, 26);
    if (this.logoUrl) {
      try { doc.addImage(this.logoUrl, 'PNG', M+1, M+1, 28, 24); } catch(e) {}
    }
    doc.setFontSize(11); doc.setTextColor(...NEGRO); doc.setFont('helvetica','bold');
    doc.text('EMPAQUES PLASTICOS S.A.S', (W/2)+15, 17, { align: 'center' });
    doc.setFontSize(8); doc.setFont('helvetica','normal');
    doc.text('NIT 816000992-1', (W/2)+15, 21, { align: 'center' });
    doc.setFontSize(9); doc.setFont('helvetica','bold');
    doc.text('FORMATO: CONTROL BODEGA PRODUCTO TERMINADO', (W/2)+15, 25, { align: 'center' });
    doc.setFontSize(7); doc.setFont('helvetica','normal');
    doc.text('REGISTRO VERIFICACION DE EMPAQUE', (W/2)+15, 29, { align: 'center' });
    doc.rect(W-M-45, M, 45, 9); doc.rect(W-M-45, M+9, 45, 9); doc.rect(W-M-45, M+18, 45, 8);
    doc.setFontSize(7);
    doc.text('Codigo: F-GC-008', W-M-43, M+6);
    doc.text('Version: 001', W-M-43, M+15);
    doc.text('Fecha: 15/09/2024', W-M-43, M+23);

    let y = M+30;
    doc.setFontSize(7); doc.setFont('helvetica','italic');
    doc.text('Nota: Las verificaciones consisten en comprobar la informacion del empaque del producto final con los criterios de calidad.', M, y+4, { maxWidth: W-M*2 });
    y += 9;
    doc.setFont('helvetica','normal');
    doc.text('Fecha reporte: ' + new Date().toLocaleDateString('es-CO'), M, y+4);
    y += 6;

    const grupos: any = {};
    this.registros.forEach(r => {
      if (!grupos[r.ordenProduccion]) grupos[r.ordenProduccion] = [];
      grupos[r.ordenProduccion].push(r);
    });

    const colWidths = [28, 20, 8, 8, 8, 8, 8, 8, 20, 20, 15, 8, 8, 25, 18, 20];
    const totalW = colWidths.reduce((a,b) => a+b, 0);
    const scale = (W-M*2) / totalW;
    const cw = colWidths.map(w => w*scale);

    doc.setFillColor(...VERDE); doc.setTextColor(255,255,255); doc.setFont('helvetica','bold'); doc.setFontSize(6);
    let x = M;
    const drawCell = (text: string, w: number, h: number, fill?: [number,number,number]) => {
      if (fill) doc.setFillColor(...fill);
      doc.rect(x, y, w, h, fill ? 'FD' : 'D');
      doc.text(text, x+w/2, y+h/2+1, { align: 'center', maxWidth: w-1 });
      x += w;
    };

    drawCell('Orden\nProduccion', cw[0], 10, VERDE);
    drawCell('Fecha', cw[1], 10, VERDE);
    x -= cw[2]+cw[3]; drawCell('Etiquetas\ncompletas', cw[2]+cw[3], 5, VERDECLARO); x -= cw[2]+cw[3]; x += cw[2]+cw[3];
    x -= cw[4]+cw[5]; drawCell('Embalaje\nuniforme', cw[4]+cw[5], 5, VERDECLARO); x -= cw[4]+cw[5]; x += cw[4]+cw[5];
    x -= cw[6]+cw[7]; drawCell('Defectos\nvisibles', cw[6]+cw[7], 5, VERDECLARO); x -= cw[6]+cw[7]; x += cw[6]+cw[7];
    drawCell('Cant.\nOP', cw[8], 10, VERDE);
    drawCell('Cant.\nReal', cw[9], 10, VERDE);
    drawCell('SI/NO', cw[10], 10, VERDE);
    x -= cw[11]+cw[12]; drawCell('Listo\nBodega', cw[11]+cw[12], 5, VERDECLARO); x -= cw[11]+cw[12]; x += cw[11]+cw[12];
    drawCell('Despachado', cw[13], 10, VERDE);
    drawCell('Foto', cw[14], 10, VERDE);
    drawCell('Revisado', cw[15], 10, VERDE);
    y += 5;

    x = M+cw[0]+cw[1];
    doc.setFillColor(...VERDE);
    ['SI','NO','SI','NO','SI','NO'].forEach((t,i) => {
      doc.rect(x, y, cw[2+i], 5, 'FD');
      doc.text(t, x+cw[2+i]/2, y+3.5, {align:'center'});
      x += cw[2+i];
    });
    x += cw[8]+cw[9]+cw[10];
    ['SI','NO'].forEach((t,i) => {
      doc.rect(x, y, cw[11+i], 5, 'FD');
      doc.text(t, x+cw[11+i]/2, y+3.5, {align:'center'});
      x += cw[11+i];
    });
    y += 5;

    const rowH = 7;
    doc.setTextColor(...NEGRO); doc.setFont('helvetica','normal'); doc.setFontSize(6);

    Object.keys(grupos).forEach(op => {
      const filas = grupos[op];
      let totalOP = 0; let totalReal = 0;
      const startY = y;

      filas.forEach((r: any, idx: number) => {
        if (y > 170) { doc.addPage(); y = 15; }
        x = M;
        totalOP += r.cantidadOP; totalReal += r.cantidadReal;
        if (idx === 0) {
          doc.rect(x, startY, cw[0], rowH*filas.length);
          doc.text(op, x+cw[0]/2, startY+(rowH*filas.length)/2+1, {align:'center', maxWidth: cw[0]-1});
        }
        x += cw[0];
        doc.rect(x, y, cw[1], rowH); doc.text(new Date(r.fecha).toLocaleDateString('es-CO'), x+cw[1]/2, y+4, {align:'center'}); x+=cw[1];
        doc.rect(x, y, cw[2], rowH); if(r.etiquetasSI) doc.text('X', x+cw[2]/2, y+4, {align:'center'}); x+=cw[2];
        doc.rect(x, y, cw[3], rowH); if(!r.etiquetasSI) doc.text('X', x+cw[3]/2, y+4, {align:'center'}); x+=cw[3];
        doc.rect(x, y, cw[4], rowH); if(r.embalajeSI) doc.text('X', x+cw[4]/2, y+4, {align:'center'}); x+=cw[4];
        doc.rect(x, y, cw[5], rowH); if(!r.embalajeSI) doc.text('X', x+cw[5]/2, y+4, {align:'center'}); x+=cw[5];
        doc.rect(x, y, cw[6], rowH); if(r.defectosSI) doc.text('X', x+cw[6]/2, y+4, {align:'center'}); x+=cw[6];
        doc.rect(x, y, cw[7], rowH); if(!r.defectosSI) doc.text('X', x+cw[7]/2, y+4, {align:'center'}); x+=cw[7];
        doc.rect(x, y, cw[8], rowH); doc.text(String(r.cantidadOP), x+cw[8]/2, y+4, {align:'center'}); x+=cw[8];
        doc.rect(x, y, cw[9], rowH); doc.text(String(r.cantidadReal), x+cw[9]/2, y+4, {align:'center'}); x+=cw[9];
        const sino = r.cantidadReal >= r.cantidadOP ? 'SI' : 'NO';
        doc.rect(x, y, cw[10], rowH); doc.text(sino, x+cw[10]/2, y+4, {align:'center'}); x+=cw[10];
        doc.rect(x, y, cw[11], rowH); if(r.listoBodega) doc.text('X', x+cw[11]/2, y+4, {align:'center'}); x+=cw[11];
        doc.rect(x, y, cw[12], rowH); if(!r.listoBodega) doc.text('X', x+cw[12]/2, y+4, {align:'center'}); x+=cw[12];
        doc.rect(x, y, cw[13], rowH); doc.text(r.despachado ?? '', x+2, y+4, {maxWidth: cw[13]-2}); x+=cw[13];
        doc.rect(x, y, cw[14], rowH);
        if (r.fotoEvidencia) {
          try { doc.addImage(this.baseUrl+'/formatos/'+r.fotoEvidencia, 'JPEG', x+0.5, y+0.5, cw[14]-1, rowH-1); } catch(e) { doc.text('Foto', x+cw[14]/2, y+4, {align:'center'}); }
        }
        x+=cw[14];
        doc.rect(x, y, cw[15], rowH); doc.text(r.revisadoPor ?? '', x+2, y+4, {maxWidth: cw[15]-2}); x+=cw[15];
        y += rowH;
      });

      const dif = totalReal - totalOP;
      const pct = totalOP > 0 ? ((dif/totalOP)*100).toFixed(1) : '0';
      doc.setFillColor(...GRIS); doc.setFont('helvetica','bold');
      x = M;
      doc.rect(x, y, cw[0]+cw[1], 6, 'FD'); doc.text('TOTAL '+op, x+4, y+4); x+=cw[0]+cw[1];
      for(let i=2; i<=7; i++) { doc.rect(x, y, cw[i], 6, 'FD'); x+=cw[i]; }
      doc.rect(x, y, cw[8], 6, 'FD'); doc.text(String(totalOP), x+cw[8]/2, y+4, {align:'center'}); x+=cw[8];
      doc.rect(x, y, cw[9], 6, 'FD'); doc.text(String(totalReal), x+cw[9]/2, y+4, {align:'center'}); x+=cw[9];
      doc.rect(x, y, cw[10]+cw[11]+cw[12]+cw[13]+cw[14]+cw[15], 6, 'FD');
      doc.setTextColor(dif < 0 ? 180 : 26, dif < 0 ? 28 : 127, dif < 0 ? 28 : 90);
      doc.text('Dif: '+(dif >= 0 ? '+' : '')+dif+' uds ('+pct+'%)', x+4, y+4);
      doc.setTextColor(...NEGRO); doc.setFont('helvetica','normal');
      y += 8;
    });

    y += 4;
    if (y > 175) { doc.addPage(); y = 15; }
    const accionesTexto = this.registros.filter((r:any) => r.accionesTomadas).map((r:any) => r.ordenProduccion+': '+r.accionesTomadas).join(' | ');
    doc.setFontSize(7);
    doc.text('Mencione las acciones tomadas para cada OP que incumplio alguna verificacion:', M, y+4);
    y += 6;
    doc.rect(M, y, (W-M*2)*0.55, 22);
    if (accionesTexto) doc.text(accionesTexto, M+2, y+5, { maxWidth: (W-M*2)*0.55-4 });

    const fx = M+(W-M*2)*0.57;
    doc.rect(fx, y, (W-M*2)*0.43, 22);
    doc.text('Firma de quien hace la entrada a Bodega:', fx+2, y+5);
    if (this.firmaDataUrl) {
      try { doc.addImage(this.firmaDataUrl, 'PNG', fx+4, y+7, 60, 10); } catch(e) {}
    }
    doc.line(fx+4, y+17, W-M-4, y+17);
    doc.text('Nombre: '+this.usuario, fx+2, y+19);
    doc.text('Cargo: '+this.cargoFirma, fx+2, y+22);
    y += 26;

    doc.setFontSize(6); doc.setFont('helvetica','italic');
    doc.text('Nota: Los metodos de verificacion de las caracteristicas y los criterios de cumplimiento a tener en cuenta, estan definidos en el Procedimiento de Pruebas y Ensayos.', M, y+4, { maxWidth: W-M*2 });
    y += 8;
    doc.setFont('helvetica','bold'); doc.setFontSize(9);
    doc.text('FIN DEL DOCUMENTO', W/2, y+4, { align: 'center' });

    doc.save('F-GC-008_'+new Date().toISOString().slice(0,10)+'.pdf');
  }
}