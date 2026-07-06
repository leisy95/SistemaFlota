import { Component, OnInit, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ConductoresService } from '../../services/conductores.service';
import { VehiculosService } from '../../services/vehiculos.service';
import { ChecklistService } from '../../services/checklist.service';
import { InspeccionesService } from '../../services/inspecciones.service';
import { TiposVehiculoService } from '../../services/tipos-vehiculo.service';
import { PermisosService } from '../../services/permisos.service';
import SignaturePad from 'signature_pad';

@Component({
  selector: 'app-inspecciones',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './inspecciones.html',
  styleUrls: ['./inspecciones.scss']
})
export class InspeccionesComponent implements OnInit, AfterViewInit {

  @ViewChild('firmaModalCanvas') firmaModalCanvas!: ElementRef<HTMLCanvasElement>;
  signaturePad!: SignaturePad;

  kilometraje = 0;
  conductores: any[] = [];
  vehiculos: any[] = [];
  tiposVehiculo: any[] = [];
  checklist: any[] = [];
  conductorId = 0;
  vehiculoId = 0;
  tipoVehiculoId = 0;
  fotoOdometro: File | null = null;
  fotoSeleccionada: File | null = null;
  guardando = false;
  guardadoExito = false;
  resultadoInspeccion: any = null;  // ← línea nueva

  modalFirmaAbierto = false;
  firmaCapturada: string | null = null;

  get puedeCrear(): boolean { return this.permisosService.puedeCrear('inspecciones'); }

  constructor(
    private conductoresService: ConductoresService,
    private vehiculosService: VehiculosService,
    private checklistService: ChecklistService,
    private inspeccionesService: InspeccionesService,
    private tiposVehiculoService: TiposVehiculoService,
    private permisosService: PermisosService
  ) { }

  ngOnInit(): void {
    this.obtenerConductores();
    this.obtenerVehiculos();
    this.obtenerTipos();
  }

  ngAfterViewInit(): void { }

  abrirModalFirma() {
    this.modalFirmaAbierto = true;
    document.body.style.overflow = 'hidden';
    setTimeout(() => { this.inicializarSignaturePad(); }, 100);
  }

  private inicializarSignaturePad() {
    const canvas = this.firmaModalCanvas?.nativeElement;
    if (!canvas) return;
    const rect = canvas.getBoundingClientRect();
    canvas.width = rect.width * window.devicePixelRatio;
    canvas.height = rect.height * window.devicePixelRatio;
    const ctx = canvas.getContext('2d')!;
    ctx.scale(window.devicePixelRatio, window.devicePixelRatio);
    this.signaturePad = new SignaturePad(canvas, {
      backgroundColor: '#475569',
      penColor: '#FFFFFF',
      minWidth: 1.5,
      maxWidth: 3.5,
    });
    if (this.firmaCapturada) {
      this.signaturePad.fromDataURL(this.firmaCapturada);
    }
  }

  limpiarFirmaModal() { this.signaturePad?.clear(); }

  confirmarFirma() {
    if (!this.signaturePad || this.signaturePad.isEmpty()) {
      alert('Por favor firme antes de confirmar');
      return;
    }
    this.firmaCapturada = this.signaturePad.toDataURL('image/png');
    this.cerrarModalFirma();
  }

  cerrarModalFirma() {
    this.modalFirmaAbierto = false;
    document.body.style.overflow = '';
  }

  limpiarFirma() {
    this.firmaCapturada = null;
    this.signaturePad?.clear();
  }

  obtenerFirmaBase64(): string | null { return this.firmaCapturada; }

  obtenerConductores() {
    this.conductoresService.obtenerConductores().subscribe({
      next: (data) => this.conductores = data,
      error: (err) => console.error(err)
    });
  }

  obtenerVehiculos() {
    this.vehiculosService.obtenerVehiculos().subscribe({
      next: (data) => this.vehiculos = data,
      error: (err) => console.error(err)
    });
  }

  obtenerTipos() {
    this.tiposVehiculoService.obtenerTipos().subscribe({
      next: (data) => this.tiposVehiculo = data,
      error: (err) => console.error(err)
    });
  }

  cargarChecklist() {
    if (this.tipoVehiculoId === 0) { this.checklist = []; return; }
    this.checklistService.obtenerChecklist(this.tipoVehiculoId).subscribe({
      next: (data) => {
        this.checklist = data.map((item: any) => ({
          ...item,
          estado: '',
          observacion: '',
          foto: null,
          evidencia: null
        }));
      },
      error: (err) => console.error(err)
    });
  }

  seleccionarFoto(event: any) {
    if (event.target.files.length > 0) {
      this.fotoOdometro = event.target.files[0];
      this.fotoSeleccionada = event.target.files[0];
    }
  }

  seleccionarEvidencia(event: any, item: any) {
    if (event.target.files.length > 0) {
      item.foto = event.target.files[0];
      item.evidencia = event.target.files[0];
    }
  }

  iniciarInspeccion() {
    if (this.conductorId === 0) { alert('Seleccione conductor'); return; }
    if (this.vehiculoId === 0) { alert('Seleccione vehículo'); return; }
    if (!this.firmaCapturada) { alert('El conductor debe firmar antes de guardar'); return; }

    const sinResponder = this.checklist.filter(item => !item.estado);
    if (sinResponder.length > 0) {
      alert(`Faltan ${sinResponder.length} pregunta(s) por responder en el checklist`);
      return;
    }

    this.guardando = true;
    const formData = new FormData();
    formData.append('VehiculoId', this.vehiculoId.toString());
    formData.append('ConductorId', this.conductorId.toString());
    // ── Kilometraje siempre entero ─────────────────────────────────────────
    formData.append('Kilometraje', Math.round(this.kilometraje).toString());

    if (this.fotoOdometro)
      formData.append('FotoOdometro', this.fotoOdometro);

    const firmaBase64 = this.obtenerFirmaBase64();
    if (firmaBase64) {
      const blob = this.base64ToBlob(firmaBase64, 'image/png');
      formData.append('FirmaCondutor', blob, 'firma.png');
    }

    const checklistEnviar = this.checklist.map((item: any) => ({
      id: item.id ?? null,
      descripcion: item.descripcion ?? '',
      estado: item.estado || 'No aplica',
      observacion: item.observacion || ''
    }));

    this.checklist.forEach((item: any, index: number) => {
      if (item.foto) {
        formData.append('Evidencias', item.foto);
        formData.append('EvidenciaIndices', index.toString());
      }
    });

    formData.append('Checklist', JSON.stringify(checklistEnviar));

    this.inspeccionesService.guardarInspeccion(formData).subscribe({
      next: (res: any) => {
        this.guardando = false;
        this.guardadoExito = true;
        this.resultadoInspeccion = {
          estado: res.estadoGeneral,
          emoji: res.emojiEstado,
          porcentaje: res.porcentajeNoConf,
          noConformes: res.totalNoConformes,
          total: res.totalItems
        };
        this.firmaCapturada = null;
        this.fotoOdometro = null;
        this.fotoSeleccionada = null;
        this.conductorId = 0;
        this.vehiculoId = 0;
        this.tipoVehiculoId = 0;
        this.kilometraje = 0;
        this.checklist = [];
        setTimeout(() => {
          this.guardadoExito = false;
          this.resultadoInspeccion = null;
        }, 6000);
      },
      error: (err) => {
        console.error(err);
        this.guardando = false;
        const mensaje = typeof err.error === 'string'
          ? err.error
          : err.error?.message
          || err.error?.title
          || err.error?.errors
          || JSON.stringify(err.error)
          || 'Error guardando inspección';
        alert(mensaje);
      }
    });
  }

  private base64ToBlob(base64: string, tipo: string): Blob {
    const partes = base64.split(',');
    const byteStr = atob(partes[1]);
    const buffer = new ArrayBuffer(byteStr.length);
    const vista = new Uint8Array(buffer);
    for (let i = 0; i < byteStr.length; i++) vista[i] = byteStr.charCodeAt(i);
    return new Blob([buffer], { type: tipo });
  }
}