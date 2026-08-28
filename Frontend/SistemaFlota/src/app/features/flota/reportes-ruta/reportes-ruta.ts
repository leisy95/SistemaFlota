import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ConductoresService } from '../../../core/services/conductores.service';
import { VehiculosService } from '../../../core/services/vehiculos.service';
import { IncidentesService } from '../../../core/services/incidentes.service';
import { PermisosService } from '../../../core/services/permisos.service';

@Component({
  selector: 'app-reportes-ruta',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reportes-ruta.html',
  styleUrls: ['./reportes-ruta.scss']
})
export class ReporteRutaComponent implements OnInit {

  conductores: any[] = [];
  vehiculos: any[] = [];

  enviando = false;
  enviado = false;
  errorMsg = '';

  fotosSeleccionadas: File[] = [];
  fotosPreview: string[] = [];
  ubicacionObtenida = false;
  obteniendo = false;

  form = {
    conductorId: 0, vehiculoId: 0,
    tipoIncidente: '', descripcionDetallada: '',
    ubicacionGPS: '', latitud: 0, longitud: 0
  };

  readonly tiposIncidente = [
    { value: 'DañoMecanico', label: 'Daño mecánico' },
    { value: 'Averia', label: 'Avería del vehículo' },
    { value: 'Trancon', label: 'Trancón' },
    { value: 'CierreVia', label: 'Cierre de vía' },
    { value: 'Accidente', label: 'Accidente de tránsito' },
    { value: 'Otro', label: 'Otro' },
  ];

  get puedeCrear(): boolean {
    return this.permisosService.puedeCrear('reporte-ruta');
  }

  constructor(
    private conductoresService: ConductoresService,
    private vehiculosService: VehiculosService,
    private incidentesService: IncidentesService,
    private permisosService: PermisosService
  ) { }

  ngOnInit(): void {
    this.obtenerConductores();
    this.obtenerVehiculos();
  }

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

  capturarUbicacion() {
    if (!navigator.geolocation) { alert('Tu dispositivo no soporta geolocalización'); return; }
    this.obteniendo = true;
    navigator.geolocation.getCurrentPosition(
      (pos) => {
        this.form.latitud = pos.coords.latitude;
        this.form.longitud = pos.coords.longitude;
        this.form.ubicacionGPS = `${pos.coords.latitude.toFixed(6)}, ${pos.coords.longitude.toFixed(6)}`;
        this.ubicacionObtenida = true;
        this.obteniendo = false;
      },
      (err) => { console.error(err); alert('No se pudo obtener la ubicación.'); this.obteniendo = false; },
      { enableHighAccuracy: true, timeout: 10000 }
    );
  }

  seleccionarFotos(event: any) {
    const archivos = Array.from(event.target.files) as File[];
    if (archivos.length + this.fotosSeleccionadas.length > 5) { alert('Máximo 5 fotos'); return; }
    archivos.forEach(archivo => {
      this.fotosSeleccionadas.push(archivo);
      const reader = new FileReader();
      reader.onload = (e: any) => this.fotosPreview.push(e.target.result);
      reader.readAsDataURL(archivo);
    });
  }

  eliminarFoto(index: number) {
    this.fotosSeleccionadas.splice(index, 1);
    this.fotosPreview.splice(index, 1);
  }

  enviarReporte() {
    if (!this.form.conductorId) { alert('Seleccione el conductor'); return; }
    if (!this.form.vehiculoId) { alert('Seleccione el vehículo'); return; }
    if (!this.form.tipoIncidente) { alert('Seleccione el tipo de incidente'); return; }
    if (!this.form.descripcionDetallada) { alert('Ingrese la descripción detallada'); return; }

    this.enviando = true; this.errorMsg = '';
    const fd = new FormData();
    fd.append('ConductorId', this.form.conductorId.toString());
    fd.append('VehiculoId', this.form.vehiculoId.toString());
    fd.append('TipoIncidente', this.form.tipoIncidente);
    fd.append('DescripcionDetallada', this.form.descripcionDetallada);
    fd.append('UbicacionGPS', this.form.ubicacionGPS);
    fd.append('Latitud', this.form.latitud.toString());
    fd.append('Longitud', this.form.longitud.toString());
    this.fotosSeleccionadas.forEach(foto => fd.append('Fotos', foto));

    this.incidentesService.crearIncidente(fd).subscribe({
      // Twilio envía WhatsApp automáticamente desde el backend
      next: () => { this.enviando = false; this.enviado = true; },
      error: (err) => { console.error(err); this.enviando = false; this.errorMsg = 'Error enviando el reporte. Intente de nuevo.'; }
    });
  }

  nuevoReporte() {
    this.enviado = false; this.enviando = false; this.errorMsg = '';
    this.fotosSeleccionadas = []; this.fotosPreview = [];
    this.ubicacionObtenida = false;
    this.form = { conductorId: 0, vehiculoId: 0, tipoIncidente: '', descripcionDetallada: '', ubicacionGPS: '', latitud: 0, longitud: 0 };
  }
}