import {
  Component,
  OnInit
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ConductoresService } from '../../services/conductores.service';
import { VehiculosService } from '../../services/vehiculos.service';
import { IncidentesService } from '../../services/incidentes.service';
import { ContactosService } from '../../services/contactos.service';

@Component({
  selector: 'app-reportes-ruta',
  standalone: true,
  imports: [CommonModule, FormsModule],
 templateUrl: './reportes-ruta.html',
styleUrls: ['./reportes-ruta.scss']
})

export class ReporteRutaComponent implements OnInit {

  conductores: any[] = [];
  vehiculos:   any[] = [];
  contactos:   any[] = [];

  enviando  = false;
  enviado   = false;
  errorMsg  = '';

  fotosSeleccionadas: File[] = [];
  fotosPreview:       string[] = [];

  ubicacionObtenida = false;
  obteniendo        = false;

  form = {
    conductorId:          0,
    vehiculoId:           0,
    tipoIncidente:        '',
    descripcionDetallada: '',
    ubicacionGPS:         '',
    latitud:              0,
    longitud:             0
  };

  readonly tiposIncidente = [
    { value: 'DañoMecanico',  label: '🔧 Daño mecánico' },
    { value: 'Averia',        label: '⚠️ Avería del vehículo' },
    { value: 'Trancon',       label: '🚗 Trancón' },
    { value: 'CierreVia',     label: '🚧 Cierre de vía' },
    { value: 'Accidente',     label: '💥 Accidente de tránsito' },
    { value: 'Otro',          label: '📋 Otro' },
  ];

  constructor(
    private conductoresService: ConductoresService,
    private vehiculosService:   VehiculosService,
    private incidentesService:  IncidentesService,
    private contactosService:   ContactosService
  ) {}

  ngOnInit(): void {
    this.obtenerConductores();
    this.obtenerVehiculos();
    this.obtenerContactos();

    // SI ES CONDUCTOR CARGAR SU INFO
    const user = localStorage.getItem('user');
    if (user) {
      const userData = JSON.parse(user);
      if (userData.rol === 'Conductor') {
        // Pre-seleccionar conductor si aplica
      }
    }
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

  obtenerContactos() {
    this.incidentesService.obtenerContactosWhatsApp().subscribe({
      next: (data) => this.contactos = data,
      error: (err) => console.error(err)
    });
  }

  // =========================
  // CAPTURAR UBICACIÓN GPS
  // =========================

  capturarUbicacion() {
    if (!navigator.geolocation) {
      alert('Tu dispositivo no soporta geolocalización');
      return;
    }

    this.obteniendo = true;

    navigator.geolocation.getCurrentPosition(
      (pos) => {
        this.form.latitud   = pos.coords.latitude;
        this.form.longitud  = pos.coords.longitude;
        this.form.ubicacionGPS = `${pos.coords.latitude.toFixed(6)}, ${pos.coords.longitude.toFixed(6)}`;
        this.ubicacionObtenida = true;
        this.obteniendo        = false;
      },
      (err) => {
        console.error(err);
        alert('No se pudo obtener la ubicación. Verifica los permisos.');
        this.obteniendo = false;
      },
      { enableHighAccuracy: true, timeout: 10000 }
    );
  }

  // =========================
  // SELECCIONAR FOTOS
  // =========================

  seleccionarFotos(event: any) {
    const archivos = Array.from(event.target.files) as File[];

    if (archivos.length + this.fotosSeleccionadas.length > 5) {
      alert('Máximo 5 fotos permitidas');
      return;
    }

    archivos.forEach(archivo => {
      this.fotosSeleccionadas.push(archivo);
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.fotosPreview.push(e.target.result);
      };
      reader.readAsDataURL(archivo);
    });
  }

  eliminarFoto(index: number) {
    this.fotosSeleccionadas.splice(index, 1);
    this.fotosPreview.splice(index, 1);
  }

  // =========================
  // ENVIAR REPORTE
  // =========================

  enviarReporte() {
    if (!this.form.conductorId)          { alert('Seleccione el conductor'); return; }
    if (!this.form.vehiculoId)           { alert('Seleccione el vehículo'); return; }
    if (!this.form.tipoIncidente)        { alert('Seleccione el tipo de incidente'); return; }
    if (!this.form.descripcionDetallada) { alert('Ingrese la descripción detallada'); return; }

    this.enviando = true;
    this.errorMsg = '';

    const formData = new FormData();
    formData.append('ConductorId',          this.form.conductorId.toString());
    formData.append('VehiculoId',           this.form.vehiculoId.toString());
    formData.append('TipoIncidente',        this.form.tipoIncidente);
    formData.append('DescripcionDetallada', this.form.descripcionDetallada);
    formData.append('UbicacionGPS',         this.form.ubicacionGPS);
    formData.append('Latitud',              this.form.latitud.toString());
    formData.append('Longitud',             this.form.longitud.toString());

    this.fotosSeleccionadas.forEach(foto => {
      formData.append('Fotos', foto);
    });

    this.incidentesService.crearIncidente(formData).subscribe({
      next: (data) => {
        this.enviando = false;
        this.enviado  = true;
        this.enviarWhatsApp(data);
      },
      error: (err) => {
        console.error(err);
        this.enviando = false;
        this.errorMsg = 'Error enviando el reporte. Intente de nuevo.';
      }
    });
  }

  // =========================
  // ENVIAR WHATSAPP
  // =========================

  enviarWhatsApp(incidente: any) {
    const conductor = this.conductores.find(c => c.id === this.form.conductorId);
    const vehiculo  = this.vehiculos.find(v => v.id === this.form.vehiculoId);
    const tipo      = this.tiposIncidente.find(t => t.value === this.form.tipoIncidente);

    const mensaje = encodeURIComponent(
`🚨 *INCIDENTE EN RUTA* 🚨
━━━━━━━━━━━━━━━━━━
🆔 *ID Reporte:* ${incidente.id}
📅 *Fecha:* ${new Date().toLocaleString()}
👤 *Conductor:* ${conductor?.nombre ?? '-'}
🚚 *Vehículo:* ${vehiculo?.placa ?? '-'}
⚠️ *Tipo:* ${tipo?.label ?? this.form.tipoIncidente}
📍 *Ubicación:* ${this.form.ubicacionGPS || 'No capturada'}
━━━━━━━━━━━━━━━━━━
📋 *Descripción:*
${this.form.descripcionDetallada}
━━━━━━━━━━━━━━━━━━
_Sistema de Gestión de Flota_`
    );

    // ABRIR WHATSAPP PARA CADA CONTACTO
    this.contactos.forEach((contacto, index) => {
      setTimeout(() => {
        const url = `https://wa.me/${contacto.numeroWhatsApp}?text=${mensaje}`;
        window.open(url, '_blank');
      }, index * 1000);
    });
  }

  // =========================
  // NUEVO REPORTE
  // =========================

  nuevoReporte() {
    this.enviado  = false;
    this.enviando = false;
    this.errorMsg = '';
    this.fotosSeleccionadas = [];
    this.fotosPreview       = [];
    this.ubicacionObtenida  = false;
    this.form = {
      conductorId: 0, vehiculoId: 0,
      tipoIncidente: '', descripcionDetallada: '',
      ubicacionGPS: '', latitud: 0, longitud: 0
    };
  }

}