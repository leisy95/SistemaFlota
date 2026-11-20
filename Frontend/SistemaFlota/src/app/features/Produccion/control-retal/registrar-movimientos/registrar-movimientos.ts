
import { CommonModule } from '@angular/common';
import {
  Component,
  ElementRef,
  OnDestroy,
  OnInit,
  ViewChild
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { LectorReferenciaService } from '../../../../core/services/produccion/lector-referencia.service';

@Component({
  selector: 'app-registrar-movimientos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './registrar-movimientos.html',
  styleUrl: './registrar-movimientos.scss',
})
export class RegistrarMovimientos implements OnInit, OnDestroy {

  @ViewChild('videoCamara') videoCamara!: ElementRef<HTMLVideoElement>;
  @ViewChild('canvasCaptura') canvasCaptura!: ElementRef<HTMLCanvasElement>;

  proceso = 'Extrusión';

  camaraActiva = false;
  imagenCapturada: string | null = null;

  // Texto completo devuelto por el OCR
  textoReconocido = '';

  cargandoOCR = false;
  mensajeError = '';
  mensajeExito = '';

  // Campos editables identificados en la fotografía
  numeroOrden = '';
  codigo = '';
  nombreOperario = '';
  turno = '';

  private stream: MediaStream | null = null;

  datosReferencia = {
    tipo: 'Tubular',
    material: 'Original',
    densidad: 'Baja',
    color: 'Transparente',
    impresion: 'Sin tinta',
    kilos: 125,
    estado: 'Óptimo'
  };

  datosEncontrados = false;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly lectorReferenciaService: LectorReferenciaService
  ) { }

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(params => {
      const proceso = params.get('proceso');

      if (proceso) {
        this.proceso = proceso;
      }
    });
  }

  async abrirCamara(): Promise<void> {
    this.mensajeError = '';
    this.mensajeExito = '';

    try {
      this.stream = await navigator.mediaDevices.getUserMedia({
        video: {
          facingMode: { ideal: 'environment' },
          width: { ideal: 1280 },
          height: { ideal: 720 }
        },
        audio: false
      });

      this.camaraActiva = true;

      setTimeout(() => {
        if (this.videoCamara?.nativeElement && this.stream) {
          this.videoCamara.nativeElement.srcObject = this.stream;
          this.videoCamara.nativeElement.play();
        }
      }, 100);

    } catch (error) {
      console.error('Error al abrir la cámara:', error);

      this.mensajeError =
        'No fue posible acceder a la cámara. Verifica los permisos del navegador.';
    }
  }

  capturarImagen(): void {
    const video = this.videoCamara?.nativeElement;
    const canvas = this.canvasCaptura?.nativeElement;

    if (!video || !canvas || !video.videoWidth) {
      this.mensajeError = 'La cámara todavía no está lista.';
      return;
    }

    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;

    const contexto = canvas.getContext('2d');

    if (!contexto) return;

    contexto.drawImage(video, 0, 0, canvas.width, canvas.height);

    this.imagenCapturada = canvas.toDataURL('image/jpeg', 0.9);

    this.detenerCamara();

    this.limpiarDatos();

    this.mensajeExito = 'Imagen capturada correctamente.';
  }

  private async dataUrlAArchivo(
    dataUrl: string,
    nombreArchivo = 'referencia.jpg'
  ): Promise<File> {

    const respuesta = await fetch(dataUrl);
    const blob = await respuesta.blob();

    return new File(
      [blob],
      nombreArchivo,
      { type: blob.type || 'image/jpeg' }
    );
  }

  async reconocerTexto(): Promise<void> {

    if (!this.imagenCapturada) {
      this.mensajeError = 'Primero debes capturar una imagen.';
      return;
    }

    this.cargandoOCR = true;
    this.mensajeError = '';
    this.mensajeExito = '';

    try {

      const archivo = await this.dataUrlAArchivo(
        this.imagenCapturada
      );

      const respuesta = await firstValueFrom(
        this.lectorReferenciaService.leerReferencia(archivo)
      );

      this.textoReconocido = (respuesta?.texto ?? '').trim();

      if (!this.textoReconocido) {
        this.mensajeError =
          'No se pudo reconocer información. Puedes completar los campos manualmente.';
        return;
      }

      // Por ahora conservamos el texto OCR completo.
      // Los campos se pueden completar o corregir manualmente.
      this.mensajeExito =
        'Lectura finalizada. Verifica y completa los cuatro campos de información.';

    } catch (error) {

      console.error(
        'Error al leer referencia en el backend:',
        error
      );

      this.mensajeError =
        'No fue posible leer la imagen. Puedes completar los campos manualmente.';

    } finally {
      this.cargandoOCR = false;
    }
  }

  buscarDatos(): void {

    this.mensajeError = '';
    this.mensajeExito = '';

    if (!this.numeroOrden.trim()) {
      this.mensajeError = 'Debes ingresar el número de orden.';
      return;
    }

    if (!this.codigo.trim()) {
      this.mensajeError = 'Debes ingresar el código.';
      return;
    }

    if (!this.nombreOperario.trim()) {
      this.mensajeError = 'Debes ingresar el nombre del operario.';
      return;
    }

    if (!this.turno) {
      this.mensajeError = 'Debes seleccionar el turno.';
      return;
    }

    this.datosEncontrados = true;

    this.mensajeExito =
      'Información validada correctamente.';
  }

  registrarMovimiento(): void {

    if (
      !this.numeroOrden.trim() ||
      !this.codigo.trim() ||
      !this.nombreOperario.trim() ||
      !this.turno ||
      !this.datosEncontrados
    ) {
      this.mensajeError =
        'Primero debes completar y procesar la información.';
      return;
    }

    // Pendiente: conectar con el servicio de registro de movimientos.
    this.mensajeExito =
      'Demostración: la información está lista para registrarse.';

    console.log('Información del movimiento:', {
      proceso: this.proceso,
      numeroOrden: this.numeroOrden,
      codigo: this.codigo,
      nombreOperario: this.nombreOperario,
      turno: this.turno
    });
  }

  private limpiarDatos(): void {
    this.textoReconocido = '';

    this.numeroOrden = '';
    this.codigo = '';
    this.nombreOperario = '';
    this.turno = '';

    this.datosEncontrados = false;
  }

  repetirCaptura(): void {

    this.imagenCapturada = null;

    this.limpiarDatos();

    this.mensajeError = '';
    this.mensajeExito = '';

    this.abrirCamara();
  }

  detenerCamara(): void {

    if (this.stream) {
      this.stream.getTracks().forEach(track => track.stop());
      this.stream = null;
    }

    this.camaraActiva = false;
  }

  volver(): void {
    this.detenerCamara();

    this.router.navigate([
      '/produccion/control-retal'
    ]);
  }

  ngOnDestroy(): void {
    this.detenerCamara();
  }
}
