import { Component, OnInit, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule }  from '@angular/forms';
import { ConductoresService }    from '../../services/conductores.service';
import { VehiculosService }      from '../../services/vehiculos.service';
import { ChecklistService }      from '../../services/checklist.service';
import { InspeccionesService }   from '../../services/inspecciones.service';
import { TiposVehiculoService }  from '../../services/tipos-vehiculo.service';
import { PermisosService }       from '../../services/permisos.service';
import SignaturePad from 'signature_pad';

@Component({
  selector: 'app-inspecciones',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './inspecciones.html',
  styleUrls: ['./inspecciones.scss']
})
export class InspeccionesComponent implements OnInit, AfterViewInit {

  @ViewChild('firmaCanvas') firmaCanvas!: ElementRef<HTMLCanvasElement>;
  signaturePad!: SignaturePad;

  kilometraje       = 0;
  conductores:      any[] = [];
  vehiculos:        any[] = [];
  tiposVehiculo:    any[] = [];
  checklist:        any[] = [];
  conductorId       = 0;
  vehiculoId        = 0;
  tipoVehiculoId    = 0;
  fotoOdometro:     File | null = null;
  fotoSeleccionada: File | null = null;
  guardando         = false;
  guardadoExito     = false;

  get puedeCrear(): boolean { return this.permisosService.puedeCrear('inspecciones'); }

  constructor(
    private conductoresService:   ConductoresService,
    private vehiculosService:     VehiculosService,
    private checklistService:     ChecklistService,
    private inspeccionesService:  InspeccionesService,
    private tiposVehiculoService: TiposVehiculoService,
    private permisosService:      PermisosService
  ) {}

  ngOnInit(): void {
    this.obtenerConductores();
    this.obtenerVehiculos();
    this.obtenerTipos();
  }

  ngAfterViewInit(): void {
    this.signaturePad = new SignaturePad(
      this.firmaCanvas.nativeElement,
      { backgroundColor: 'rgb(255,255,255)', penColor: 'rgb(0,0,0)' }
    );
  }

  limpiarFirma() { this.signaturePad.clear(); }

  obtenerFirmaBase64(): string | null {
    if (this.signaturePad.isEmpty()) return null;
    return this.signaturePad.toDataURL('image/png');
  }

  obtenerConductores() {
    this.conductoresService.obtenerConductores().subscribe({
      next: (data) => this.conductores = data,
      error: (err)  => console.error(err)
    });
  }

  obtenerVehiculos() {
    this.vehiculosService.obtenerVehiculos().subscribe({
      next: (data) => this.vehiculos = data,
      error: (err)  => console.error(err)
    });
  }

  obtenerTipos() {
    this.tiposVehiculoService.obtenerTipos().subscribe({
      next: (data) => this.tiposVehiculo = data,
      error: (err)  => console.error(err)
    });
  }

  cargarChecklist() {
    if (this.tipoVehiculoId === 0) { this.checklist = []; return; }
    this.checklistService.obtenerChecklist(this.tipoVehiculoId).subscribe({
      next: (data) => {
        this.checklist = data.map((item: any) => ({
          ...item, estado: '', observacion: '', foto: null, evidencia: null
        }));
      },
      error: (err) => console.error(err)
    });
  }

  seleccionarFoto(event: any) {
    if (event.target.files.length > 0) {
      this.fotoOdometro     = event.target.files[0];
      this.fotoSeleccionada = event.target.files[0];
    }
  }

  seleccionarEvidencia(event: any, item: any) {
    if (event.target.files.length > 0) {
      item.foto      = event.target.files[0];
      item.evidencia = event.target.files[0];
    }
  }

  iniciarInspeccion() {
    if (this.conductorId === 0)      { alert('Seleccione conductor'); return; }
    if (this.vehiculoId === 0)       { alert('Seleccione vehículo'); return; }
    if (this.signaturePad.isEmpty()) { alert('El conductor debe firmar antes de guardar'); return; }

    this.guardando = true;
    const formData = new FormData();
    formData.append('VehiculoId',  this.vehiculoId.toString());
    formData.append('ConductorId', this.conductorId.toString());
    formData.append('Kilometraje', this.kilometraje.toString());

    if (this.fotoOdometro)
      formData.append('FotoOdometro', this.fotoOdometro);

    const firmaBase64 = this.obtenerFirmaBase64();
    if (firmaBase64) {
      const blob = this.base64ToBlob(firmaBase64, 'image/png');
      formData.append('FirmaCondutor', blob, 'firma.png');
    }

    const checklistEnviar = this.checklist.map((item: any) => ({
      id: item.id, estado: item.estado, observacion: item.observacion
    }));

    this.checklist.forEach((item: any, index: number) => {
      if (item.foto) {
        formData.append('Evidencias',       item.foto);
        formData.append('EvidenciaIndices', index.toString());
      }
    });

    formData.append('Checklist', JSON.stringify(checklistEnviar));

    this.inspeccionesService.guardarInspeccion(formData).subscribe({
      next: () => {
        this.guardando = false; this.guardadoExito = true;
        this.limpiarFirma();
        this.fotoOdometro = null; this.fotoSeleccionada = null;
        this.conductorId = 0; this.vehiculoId = 0;
        this.tipoVehiculoId = 0; this.kilometraje = 0;
        this.checklist = [];
        setTimeout(() => this.guardadoExito = false, 4000);
      },
      error: (err) => {
        console.error(err);
        this.guardando = false;
        alert(err.error || 'Error guardando inspección');
      }
    });
  }

  private base64ToBlob(base64: string, tipo: string): Blob {
    const partes  = base64.split(',');
    const byteStr = atob(partes[1]);
    const buffer  = new ArrayBuffer(byteStr.length);
    const vista   = new Uint8Array(buffer);
    for (let i = 0; i < byteStr.length; i++) vista[i] = byteStr.charCodeAt(i);
    return new Blob([buffer], { type: tipo });
  }
}