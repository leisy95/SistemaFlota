import { Injectable } from '@angular/core';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import QRCode from 'qrcode';
import { ConfiguracionService } from './configuracion.service';

@Injectable({ providedIn: 'root' })
export class PdfService {

  private config: any = null;

  constructor(private configuracionService: ConfiguracionService) {
    this.configuracionService.obtenerConfiguracion().subscribe({
      next: (data) => this.config = data,
      error: () => this.config = null
    });
  }

  // =========================
  // CARGAR IMAGEN
  // =========================

  cargarImagen(src: string): Promise<HTMLImageElement | null> {
    return new Promise(resolve => {
      const img = new Image();
      img.crossOrigin = 'anonymous';
      img.src = src;
      img.onload  = () => resolve(img);
      img.onerror = () => resolve(null);
    });
  }

  // =========================
  // COLOR CORPORATIVO
  // =========================

  getColor(): [number, number, number] {
    const hex = this.config?.colorCorporativo ?? '#15803d';
    const r = parseInt(hex.slice(1, 3), 16);
    const g = parseInt(hex.slice(3, 5), 16);
    const b = parseInt(hex.slice(5, 7), 16);
    return [r, g, b];
  }

  // =========================
  // ENCABEZADO EMPRESARIAL
  // =========================

  async agregarEncabezado(
    doc: jsPDF,
    titulo: string,
    subtitulo: string,
    qrData: string
  ): Promise<void> {

    const COLOR = this.getColor();
    const BLANCO: [number, number, number] = [255, 255, 255];

    // FONDO ENCABEZADO
    doc.setFillColor(...COLOR);
    doc.rect(0, 0, 210, 42, 'F');

    // LOGO EMPRESA
    const logoUrl = this.config?.logo
      ? `https://api.gecobagsci.com/config/${this.config.logo}`
      : 'https://flota.gecobagsci.com/assets/logo.png';

    const logo = await this.cargarImagen(logoUrl);
    if (logo) doc.addImage(logo, 'PNG', 5, 4, 32, 32);

    // NOMBRE EMPRESA
    const nombreEmpresa = this.config?.nombreEmpresa ?? 'Sistema de GestiÃ³n de Flota';
    const nit           = this.config?.nit           ?? '';

    doc.setFontSize(18);
    doc.setTextColor(...BLANCO);
    doc.setFont('helvetica', 'bold');
    doc.text(titulo, 105, 15, { align: 'center' });

    doc.setFontSize(10);
    doc.setFont('helvetica', 'normal');
    doc.text(nombreEmpresa, 105, 24, { align: 'center' });

    if (nit) {
      doc.setFontSize(8);
      doc.text(`NIT: ${nit}`, 105, 31, { align: 'center' });
    }

    doc.setFontSize(8);
    doc.text(subtitulo, 105, 38, { align: 'center' });

    // QR
    const qrImage = await QRCode.toDataURL(qrData, { width: 120, margin: 1 });
    doc.addImage(qrImage, 'PNG', 175, 3, 32, 32);
    doc.setFontSize(6);
    doc.setTextColor(200, 200, 200);
    doc.text('Escanear QR', 191, 37, { align: 'center' });
  }

  // =========================
  // FOOTER
  // =========================

  agregarFooter(doc: jsPDF, margenDerecho = 196): void {
    const COLOR         = this.getColor();
    const nombreEmpresa = this.config?.nombreEmpresa ?? 'Sistema de GestiÃ³n de Flota';
    const numPaginas    = (doc as any).internal.getNumberOfPages();

    for (let i = 1; i <= numPaginas; i++) {
      doc.setPage(i);
      doc.setDrawColor(...COLOR);
      doc.setLineWidth(0.3);
      doc.line(14, 285, margenDerecho, 285);
      doc.setFontSize(7);
      doc.setTextColor(120, 120, 120);
      doc.setFont('helvetica', 'normal');
      doc.text(`${nombreEmpresa} â€” Documento generado automÃ¡ticamente`,
        105, 289, { align: 'center' });
      doc.text(`PÃ¡gina ${i} de ${numPaginas}`, margenDerecho, 289, { align: 'right' });
      doc.text(new Date().toLocaleString(), 14, 289);
    }
  }

  // =========================
  // DATOS EN COLUMNAS
  // =========================

  agregarDatos(
    doc: jsPDF,
    datos: [string, string][],
    startY: number,
    startX = 14,
    labelX = 50
  ): number {
    const COLOR = this.getColor();
    const GRIS: [number, number, number] = [60, 60, 60];
    let y = startY;

    datos.forEach(([label, valor]) => {
      doc.setFont('helvetica', 'bold');
      doc.setTextColor(...COLOR);
      doc.setFontSize(10);
      doc.text(`${label}:`, startX, y);
      doc.setFont('helvetica', 'normal');
      doc.setTextColor(...GRIS);
      doc.text(valor, labelX, y);
      y += 9;
    });

    return y;
  }

  // =========================
  // PDF AUTORIZACIÃ“N
  // =========================

  async generarPDFAutorizacion(autorizacion: any): Promise<void> {
    const doc   = new jsPDF();
    const COLOR = this.getColor();
    const GRIS: [number, number, number]  = [60, 60, 60];
    const CLARO: [number, number, number] = [245, 245, 245];
    const VERDE: [number, number, number] = [21, 128, 61];
    const ROJO: [number, number, number]  = [185, 28, 28];

    const qrData = [
      `AutorizaciÃ³n: ${autorizacion.id}`,
      `Conductor: ${autorizacion.conductor?.nombre ?? '-'}`,
      `VehÃ­culo: ${autorizacion.vehiculo?.placa ?? '-'}`,
      `Estado: ${autorizacion.estado}`,
      `Fecha: ${new Date(autorizacion.fechaCreacion).toLocaleString()}`
    ].join('\n');

    await this.agregarEncabezado(
      doc,
      'AUTORIZACIÃ“N DE SALIDA',
      `ID: ${autorizacion.id} | Estado: ${autorizacion.estado}`,
      qrData
    );

    // BADGE ESTADO
    const esAutorizado = autorizacion.estado === 'Autorizado';
    doc.setFillColor(...(esAutorizado ? VERDE : ROJO));
    doc.roundedRect(14, 47, 90, 10, 2, 2, 'F');
    doc.setFontSize(9);
    doc.setTextColor(255, 255, 255);
    doc.setFont('helvetica', 'bold');
    doc.text(
      esAutorizado ? 'âœ” AUTORIZADO' : `â— ${autorizacion.estado.toUpperCase()}`,
      59, 54, { align: 'center' }
    );

    // DATOS PRINCIPALES
    const datos: [string, string][] = [
      ['Conductor',  autorizacion.conductor?.nombre    ?? '-'],
      ['VehÃ­culo',   autorizacion.vehiculo?.placa      ?? '-'],
      ['Tipo',       autorizacion.tipoVuelta            ?? '-'],
      ['Destino',    autorizacion.destinoCompleto       || 'MensajerÃ­a'],
      ['Clientes',   `${autorizacion.cantidadClientes  ?? '-'}`],
      ['Peso',       autorizacion.pesoKilos ? `${autorizacion.pesoKilos} kg` : '-'],
      ['GuÃ­a',       autorizacion.numeroGuia            || '-'],
      ['Fecha',      new Date(autorizacion.fechaCreacion).toLocaleString()],
    ];

    let y = this.agregarDatos(doc, datos, 63);

    if (autorizacion.descripcionCarga) {
      doc.setFont('helvetica', 'bold');
      doc.setTextColor(...COLOR);
      doc.text('Carga:', 14, y);
      doc.setFont('helvetica', 'normal');
      doc.setTextColor(...GRIS);
      const lineas = doc.splitTextToSize(autorizacion.descripcionCarga, 130);
      doc.text(lineas, 50, y);
      y += lineas.length * 6 + 4;
    }

    // LÃNEA DIVISORA
    doc.setDrawColor(...COLOR);
    doc.setLineWidth(0.5);
    doc.line(14, y + 4, 196, y + 4);
    y += 12;

    // TABLA FLUJO DE APROBACIÃ“N
    autoTable(doc, {
      startY: y,
      head: [['Etapa', 'Responsable', 'ObservaciÃ³n', 'Estado']],
      body: [
        [
          'FacturaciÃ³n',
          autorizacion.usuarioFacturacion   || '-',
          autorizacion.observacionFacturacion || '-',
          autorizacion.firmaFacturacion ? 'âœ” Firmado' : 'â³ Pendiente'
        ],
        [
          'Bodega',
          autorizacion.usuarioBodega        || '-',
          autorizacion.observacionBodega    || '-',
          autorizacion.firmaBodega ? 'âœ” Firmado' : 'â³ Pendiente'
        ],
        [
          'PorterÃ­a',
          autorizacion.usuarioPorteria      || '-',
          autorizacion.observacionPorteria  || '-',
          autorizacion.firmaPorteria ? 'âœ” Firmado' : 'â³ Pendiente'
        ],
      ],
      headStyles: { fillColor: COLOR, textColor: [255,255,255], fontStyle: 'bold', fontSize: 9 },
      bodyStyles: { fontSize: 9, textColor: GRIS },
      alternateRowStyles: { fillColor: CLARO },
      columnStyles: {
        0: { cellWidth: 30 },
        1: { cellWidth: 45 },
        2: { cellWidth: 75 },
        3: { cellWidth: 36, halign: 'center' }
      }
    });

    this.agregarFooter(doc);
    doc.save(`autorizacion-${autorizacion.id}.pdf`);
  }

  // =========================
  // PDF INCIDENTE
  // =========================

  async generarPDFIncidente(incidente: any): Promise<void> {
    const doc   = new jsPDF();
    const COLOR = this.getColor();
    const GRIS: [number, number, number]  = [60, 60, 60];
    const ROJO: [number, number, number]  = [185, 28, 28];
    const AMARILLO: [number, number, number] = [245, 158, 11];

    const qrData = [
      `Incidente: ${incidente.id}`,
      `Conductor: ${incidente.conductor?.nombre ?? '-'}`,
      `VehÃ­culo: ${incidente.vehiculo?.placa ?? '-'}`,
      `Tipo: ${incidente.tipoIncidente}`,
      `Estado: ${incidente.estado}`,
      `Fecha: ${new Date(incidente.fechaReporte).toLocaleString()}`
    ].join('\n');

    await this.agregarEncabezado(
      doc,
      'REPORTE DE INCIDENTE EN RUTA',
      `ID: ${incidente.id} | ${incidente.tipoIncidente}`,
      qrData
    );

    // BADGE ESTADO
    const esRevisado = incidente.estado === 'Revisado';
    doc.setFillColor(...(esRevisado ? COLOR : AMARILLO));
    doc.roundedRect(14, 47, 80, 10, 2, 2, 'F');
    doc.setFontSize(9);
    doc.setTextColor(255, 255, 255);
    doc.setFont('helvetica', 'bold');
    doc.text(esRevisado ? 'âœ” REVISADO' : 'â³ PENDIENTE', 54, 54, { align: 'center' });

    // DATOS
    const tiposLabel: any = {
      DañoMecanico: 'Daño mecánico', Averia: 'Avería',
      Trancon: 'Trancón', CierreVia: 'Cierre de vía',
      Accidente: 'Accidente de tránsito', Otro: 'Otro'
    };

    const datos: [string, string][] = [
      ['Conductor',  incidente.conductor?.nombre ?? '-'],
      ['VehÃ­culo',   incidente.vehiculo?.placa   ?? '-'],
      ['Tipo',       tiposLabel[incidente.tipoIncidente] ?? incidente.tipoIncidente],
      ['Fecha',      new Date(incidente.fechaReporte).toLocaleString()],
      ['UbicaciÃ³n',  incidente.ubicacionGPS || 'No capturada'],
    ];

    let y = this.agregarDatos(doc, datos, 63);

    // DESCRIPCIÃ“N
    doc.setFont('helvetica', 'bold');
    doc.setTextColor(...COLOR);
    doc.setFontSize(10);
    doc.text('DescripciÃ³n:', 14, y + 4);
    y += 10;
    doc.setFont('helvetica', 'normal');
    doc.setTextColor(...GRIS);
    const lineas = doc.splitTextToSize(incidente.descripcionDetallada ?? '-', 180);
    doc.text(lineas, 14, y);
    y += lineas.length * 6 + 8;

    // REVISIÃ“N
    if (incidente.estado === 'Revisado') {
      doc.setDrawColor(...COLOR);
      doc.setLineWidth(0.5);
      doc.line(14, y, 196, y);
      y += 8;

      doc.setFont('helvetica', 'bold');
      doc.setTextColor(...COLOR);
      doc.text('InformaciÃ³n de revisiÃ³n:', 14, y);
      y += 8;

      const datosRevision: [string, string][] = [
        ['Revisado por',  incidente.revisadoPor ?? '-'],
        ['Fecha revisiÃ³n', incidente.fechaRevision
          ? new Date(incidente.fechaRevision).toLocaleString() : '-'],
        ['ObservaciÃ³n',   incidente.observacionRevision || '-'],
      ];

      this.agregarDatos(doc, datosRevision, y);
    }

    this.agregarFooter(doc);
    doc.save(`incidente-${incidente.id}.pdf`);
  }

  // =========================
  // PDF MANTENIMIENTO
  // =========================

  async generarPDFMantenimiento(m: any): Promise<void> {
    const doc   = new jsPDF();
    const COLOR = this.getColor();
    const GRIS: [number, number, number]  = [60, 60, 60];
    const CLARO: [number, number, number] = [245, 245, 245];
    const VERDE: [number, number, number] = [21, 128, 61];

    const qrData = [
      `Mantenimiento: ${m.id}`,
      `VehÃ­culo: ${m.vehiculo?.placa ?? '-'}`,
      `Tipo: ${m.tipoMantenimiento}`,
      `Taller: ${m.nombreTaller}`,
      `Estado: ${m.estado}`,
      `Fecha: ${new Date(m.fechaEntrada).toLocaleString()}`
    ].join('\n');

    await this.agregarEncabezado(
      doc,
      'REPORTE DE MANTENIMIENTO',
      `ID: ${m.id} | ${m.tipoMantenimiento}`,
      qrData
    );

    // BADGE ESTADO
    const colores: any = {
      EnTaller:   [245, 158, 11],
      Finalizado: VERDE,
      Cancelado:  [185, 28, 28]
    };
    const labels: any = {
      EnTaller: 'ðŸ”§ EN TALLER', Finalizado: 'âœ… FINALIZADO', Cancelado: 'âŒ CANCELADO'
    };

    const colorEstado = colores[m.estado] ?? COLOR;
doc.setFillColor(colorEstado[0], colorEstado[1], colorEstado[2]);
    doc.roundedRect(14, 47, 80, 10, 2, 2, 'F');
    doc.setFontSize(9);
    doc.setTextColor(255, 255, 255);
    doc.setFont('helvetica', 'bold');
    doc.text(labels[m.estado] ?? m.estado, 54, 54, { align: 'center' });

    // DATOS
    const datos: [string, string][] = [
      ['VehÃ­culo',       `${m.vehiculo?.placa ?? '-'} â€” ${m.vehiculo?.marca ?? ''} ${m.vehiculo?.modelo ?? ''}`],
      ['Tipo',           m.tipoMantenimiento],
      ['Taller',         m.nombreTaller],
      ['TÃ©cnico',        m.tecnicoResponsable || '-'],
      ['TelÃ©fono',       m.telefonoTaller     || '-'],
      ['Fecha entrada',  new Date(m.fechaEntrada).toLocaleString()],
      ['Fecha salida',   m.fechaSalida ? new Date(m.fechaSalida).toLocaleString() : 'En taller'],
      ['Kilometraje',    `${m.kilometrajeEntrada} km`],
    ];

    let y = this.agregarDatos(doc, datos, 63);

    // LÃNEA
    doc.setDrawColor(...COLOR);
    doc.setLineWidth(0.5);
    doc.line(14, y + 2, 196, y + 2);
    y += 10;

    // TRABAJOS Y COSTOS
    autoTable(doc, {
      startY: y,
      head: [['Detalle', 'InformaciÃ³n']],
      body: [
        ['Trabajos realizados',  m.trabajosRealizados  || '-'],
        ['Repuestos utilizados', m.repuestosUtilizados || '-'],
        ['Observaciones',        m.observaciones       || '-'],
        ['Costo mano de obra',   `$${(m.costoManoObra  || 0).toLocaleString()}`],
        ['Costo repuestos',      `$${(m.costoRepuestos || 0).toLocaleString()}`],
        ['COSTO TOTAL',          `$${(m.costoTotal     || 0).toLocaleString()}`],
        ['PrÃ³ximo mantenimiento km',   m.kilometrajeSiguiente ? `${m.kilometrajeSiguiente} km` : '-'],
        ['PrÃ³xima fecha mantenimiento', m.fechaSiguiente
          ? new Date(m.fechaSiguiente).toLocaleDateString() : '-'],
      ],
      headStyles: { fillColor: COLOR, textColor: [255,255,255], fontStyle: 'bold', fontSize: 9 },
      bodyStyles: { fontSize: 9, textColor: GRIS },
      alternateRowStyles: { fillColor: CLARO },
      columnStyles: {
        0: { cellWidth: 70, fontStyle: 'bold' },
        1: { cellWidth: 116 }
      },
      didDrawCell: (data: any) => {
        if (data.section === 'body' && data.row.index === 5) {
          doc.setFillColor(...COLOR);
        }
      }
    });

    this.agregarFooter(doc);
    doc.save(`mantenimiento-${m.id}.pdf`);
  }

}

