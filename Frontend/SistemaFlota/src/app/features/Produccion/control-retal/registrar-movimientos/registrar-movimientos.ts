
import { CommonModule } from '@angular/common';
import {
  Component,
  ElementRef,
  OnDestroy,
  OnInit,
  ViewChild,
  ChangeDetectorRef
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { LectorReferenciaService } from '../../../../core/services/produccion/lector-referencia.service';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ControlRetalService } from '../../../../core/services/produccion/control-retal/control-retal.service';
import { InformacionOrdenProd } from '../informacion-orden-prod/informacion-orden-prod';

@Component({
  selector: 'app-registrar-movimientos',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule
  ],
  templateUrl: './registrar-movimientos.html',
  styleUrl: './registrar-movimientos.scss',
})
export class RegistrarMovimientos implements OnInit, OnDestroy {

  @ViewChild('videoCamara') videoCamara!: ElementRef<HTMLVideoElement>;
  @ViewChild('canvasCaptura') canvasCaptura!: ElementRef<HTMLCanvasElement>;

  proceso = 'Extrusión';

  camaraActiva = false;
  imagenCapturada: string | null = null;

  textoReconocido = '';

  cargandoOCR = false;
  mensajeError = '';
  mensajeExito = '';

  // Datos reconocidos por OCR
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
  cargandoOrden = false;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly lectorReferenciaService: LectorReferenciaService,
    private readonly cdr: ChangeDetectorRef,
    private readonly controlRetalService: ControlRetalService,
    private readonly dialog: MatDialog,
  ) { }

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(params => {
      const proceso = params.get('proceso');

      if (proceso) {
        this.proceso = proceso;
      }

      this.cdr.markForCheck();
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
      this.cdr.detectChanges();

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

      this.cdr.detectChanges();
    }
  }

  capturarImagen(): void {
    const video = this.videoCamara?.nativeElement;
    const canvas = this.canvasCaptura?.nativeElement;

    if (!video || !canvas || !video.videoWidth) {
      this.mensajeError = 'La cámara todavía no está lista.';
      this.cdr.detectChanges();
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

    this.cdr.detectChanges();
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

  reconocerTexto(): void {

    if (!this.imagenCapturada || this.cargandoOCR) {
      if (!this.imagenCapturada) {
        this.mensajeError = 'Primero debes capturar una imagen.';
      }
      return;
    }

    this.cargandoOCR = true;
    this.mensajeError = '';
    this.mensajeExito = '';
    this.datosEncontrados = false;

    this.cdr.detectChanges();

    this.dataUrlAArchivo(this.imagenCapturada)
      .then(archivo => {

        this.lectorReferenciaService.leerReferencia(archivo).subscribe({

          next: (respuesta) => {

            console.log('Respuesta OCR:', respuesta);

            if (!respuesta?.leidaCorrectamente) {
              this.mensajeError =
                respuesta?.mensaje || 'No fue posible reconocer los datos.';

              this.datosEncontrados = false;
              this.cargandoOCR = false;
              this.cdr.detectChanges();
              return;
            }

            this.textoReconocido = respuesta.texto ?? '';
            this.numeroOrden = respuesta.numeroOrden ?? '';
            this.codigo = respuesta.codigo ?? '';
            this.nombreOperario = respuesta.nombreOperario ?? '';
            this.turno = respuesta.turno ?? '';

            this.datosEncontrados = true;

            this.mensajeExito =
              'Datos reconocidos correctamente. Verifica la información.';

            console.log('Datos asignados:', {
              numeroOrden: this.numeroOrden,
              codigo: this.codigo,
              nombreOperario: this.nombreOperario,
              turno: this.turno
            });

            this.cargandoOCR = false;
            this.cdr.detectChanges();
          },

          error: (error) => {

            console.error('Error al leer referencia:', error);

            this.datosEncontrados = false;
            this.mensajeError =
              'No fue posible leer la imagen. Puedes completar los campos manualmente.';

            this.cargandoOCR = false;
            this.cdr.detectChanges();
          }
        });

      })
      .catch(error => {

        console.error('Error al convertir imagen:', error);

        this.mensajeError = 'No fue posible procesar la imagen.';
        this.cargandoOCR = false;
        this.cdr.detectChanges();

      });
  }

  buscarDatos(): void {

    this.mensajeError = '';
    this.mensajeExito = '';

    if (!this.numeroOrden.trim()) {
      this.mensajeError = 'Debes ingresar el número de orden.';
      return;
    }

    if (!this.codigo.trim()) {
      this.mensajeError = 'Debes ingresar el peso.';
      return;
    }

    if (!this.nombreOperario.trim()) {
      this.mensajeError = 'Debes ingresar el nombre del operario.';
      return;
    }

    if (!this.turno.trim()) {
      this.mensajeError = 'Debes ingresar el turno.';
      return;
    }

    this.datosEncontrados = true;

    this.mensajeExito = 'Información validada correctamente.';

    this.cdr.detectChanges();
  }

  consultarOrden(): void {

    if (
      !this.numeroOrden.trim() ||
      !this.codigo.trim() ||
      !this.nombreOperario.trim() ||
      !this.turno.trim() ||
      !this.datosEncontrados
    ) {

      this.mensajeError =
        'Primero debes completar y procesar la información.';

      this.cdr.detectChanges();
      return;
    }

    const numop = Number(this.numeroOrden.trim());

    if (!Number.isInteger(numop) || numop <= 0) {
      this.mensajeError = 'El número de orden no es válido.';
      return;
    }

    this.cargandoOrden = true;
    this.mensajeError = '';
    this.mensajeExito = '';

    this.controlRetalService.consultarOrden(numop).subscribe({

      next: (respuesta) => {

        this.dialog.open(InformacionOrdenProd, {
          width: '600px',
          maxWidth: '95vw',
          data: {
            referencia: respuesta.refer
          }
        });

        this.cargandoOrden = false;
        this.cdr.detectChanges();
      },

      error: (error) => {

        console.error('Error al consultar la orden:', error);

        this.mensajeError =
          error.status === 404
            ? 'No se encontró la orden de producción.'
            : 'Error al consultar la orden. Intenta nuevamente.';

        this.cargandoOrden = false;
        this.cdr.detectChanges();
      }

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

    this.cdr.detectChanges();

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
