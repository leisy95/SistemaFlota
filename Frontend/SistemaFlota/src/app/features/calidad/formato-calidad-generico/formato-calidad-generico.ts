import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { FormatosCalidadService } from '../../../core/services/formatos-calidad.service';
import { OrdenesProduccionService } from '../../../core/services/ordenes-produccion.service';
import { PermisosService } from '../../../core/services/permisos.service';
import jsPDF from 'jspdf';
import { firstValueFrom } from 'rxjs';
import autoTable from 'jspdf-autotable';
import { OpcionesFormularioService } from '../../../core/services/opciones-formulario.service';
import { MatDialog } from '@angular/material/dialog';
import { DialogConfirmacion } from '../../../shared/dialog-confirmacion/dialog-confirmacion';
import { DialogInput } from '../../../shared/dialog-input/dialog-input';
import { environment } from '../../../../environments/environment';


@Component({
    selector: 'app-formato-calidad-generico',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './formato-calidad-generico.html',
    styleUrls: ['./formato-calidad-generico.scss']
})
export class FormatoCalidadGenericoComponent implements OnInit, OnDestroy {
    // ── Configuración del tipo de formato (viene de la ruta) ──
    codigoFormato: string = '';
    tipoFormato: any = null;
    caracteristicas: any[] = [];

    // ── Estado de la vista ──
    vista: 'lista' | 'nuevo' | 'ver' | 'editar' | 'liberar' = 'lista';
    cargando = false;
    registros: any[] = [];
    registroSeleccionado: any = null;
    editandoId: number | null = null;
    menuAbiertoId: number | null = null;
    mostrarModalNoCumple = false;
    rondaModalPendiente: number | null = null;
    caracteristicaModalPendiente: number | null = null;
    pestanasAbiertas: { titulo: string; estado: any }[] = [];
    pestanaActivaIndice = -1;

    toggleMenu(id: number, event: Event) {
        event.stopPropagation();
        this.menuAbiertoId = this.menuAbiertoId === id ? null : id;
    }

    cerrarMenus() {
        this.menuAbiertoId = null;
    }

    // ── Filtros ──
    filtroDesde = '';
    filtroHasta = '';
    filtroOP = '';
    filtroOPDesperdicio = '';
    resultadoDesperdicio: { totalDesperdicio: number; totalRegistros: number } | null = null;
    desperdicioAbierto = false;

    // ── Formulario ──
    form = {
        ordenProduccion: '',
        cliente: '',
        referencia: '',
        operarios: '',
        hora: '',
        maquina: '',
        puedeLiberarse: null as boolean | null,
        explicacionNoLiberado: '',
        cargoFirma: '',
        produccionKgHora: '',
        desperdicioTotalKg: null as number | null,
        fechaRealProduccion: ''
    };

    // Rondas de verificación por hora: cada ronda tiene su hora, operario, y los valores de cada característica
    rondas: {
        id?: string;
        hora: string;
        final: boolean;
        operario: string;
        cierreDeOperario: boolean;
        kilosDesperdicio: string;
        valores: { [caracteristicaId: number]: 'cumple' | 'noCumple' | 'na' | null };
        valoresNumericos: { [caracteristicaId: number]: string };
    }[] = [];
    observaciones: { [caracteristicaId: number]: string } = {};
    tieneRondaFinal = false;

    // Variables críticas (solo si tipoFormato.tieneVariablesCriticas)
    variablesCriticas: any = {
        // Extrusión (F-GC-004)
        corona: '', molde: '',
        temperaturas: { zona1: '', zona2: '', zona3: '', zona4: '', zona5: '', zona6: '' },
        velocidades: { maquina: '', maquina2: '', halador: '', bobinador: '' },
        aire: '', amperaje: '', alturaBurbuja: '', vela: '',
        // Impresión (F-GC-005)
        maquinaImpresion: '', metrosPorMinuto: '', velocidadMaquinaHz: '',
        // Sellado (F-GC-006)
        maquinaSellado: '', golpesPorMinuto: '', bolsasPorMinuto: '', temperaturaSuperior: '', temperaturaInferior: '',
        // Precorte (F-GC-007)
        maquinaPrecorte: '', temperaturaPrecorte: ''
    };

    firmaDataUrl: string | null = null;
    opExistente: any = null;
    clienteDetectado: string | null = null;
    mezclaOrden: any[] = [];
    referenciaDetectada: string | null = null;
    descripcionDetectada: string | null = null;
    cantidadDetectada: number | null = null;
    unidadDetectada: string | null = null;
    opcionesMaquina: any[] = [];
    opcionesCorona: any[] = [];
    opcionesMolde: any[] = [];
    opcionesOperario: any[] = [];
    opcionesSupervisor: any[] = [];
    operarioSesion: string | null = null;
    mostrarModalLoginOperario = false;
    operarioSeleccionadoLogin = '';

    get usuario(): string {
        const u = JSON.parse(sessionStorage.getItem('user') || '{}');
        return u.username ?? '';
    }

    get rolUsuario(): string {
        const u = JSON.parse(sessionStorage.getItem('user') || '{}');
        return u.rol ?? '';
    }
    get esCuentaCompartidaMaquina(): boolean {
        const u = JSON.parse(sessionStorage.getItem('user') || '{}');
        const username = u.username ?? '';
        return ['Extrusion', 'Precorte', 'Impresion', 'Sellado'].includes(username);
    }
    get puedeFijarFechaReal(): boolean {
        return this.permisos.puedeEditar('calidad-formatos') && !this.esCuentaCompartidaMaquina;
    }

    // Fecha de hoy en hora local (toISOString usaría UTC y de noche daría el día siguiente)
    get hoyISO(): string {
        const d = new Date();
        return d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0');
    }
    get esCoextrusora(): boolean {
        return (this.form.maquina || '').toLowerCase().includes('coextrusora');
    }

    constructor(
        private service: FormatosCalidadService,
        private ordenesService: OrdenesProduccionService,
        private opcionesService: OpcionesFormularioService,
        public permisos: PermisosService,
        private route: ActivatedRoute,
        private dialog: MatDialog
    ) { }

    ngOnInit(): void {
        this.codigoFormato = this.route.snapshot.data['codigo'];
        this.esSupervisor = this.permisos.puedeEditar('calidad-formatos');

        const sesionGuardada = sessionStorage.getItem('operarioSesionFormatos');
        if (this.esCuentaCompartidaMaquina && sesionGuardada) {
            this.operarioSesion = sesionGuardada;
            this.mostrarModalLoginOperario = false;
        } else {
            this.mostrarModalLoginOperario = this.esCuentaCompartidaMaquina;
        }

        this.cargarConfiguracion();
    }
    confirmarLoginOperario() {

        if (!this.operarioSeleccionadoLogin) { alert('Seleccione su nombre'); return; }

        const operarioInfo = this.opcionesOperario.find(o => o.valor === this.operarioSeleccionadoLogin);

        if (operarioInfo?.codigo) {
            const dialogRef = this.dialog.open(DialogInput, {
                data: {
                    titulo: 'Ingrese su código',
                    mensaje: `Ingrese el código de ${this.operarioSeleccionadoLogin} para continuar`,
                    label: 'Código',
                    placeholder: 'Ej: 1234',
                    textoConfirmar: 'Ingresar'
                }
            });


            dialogRef.afterClosed().subscribe((codigo: string | null) => {
                if (codigo === null) return;
                if (codigo.trim() !== operarioInfo.codigo) { alert('Código incorrecto'); return; }
                this.operarioSesion = this.operarioSeleccionadoLogin;
                sessionStorage.setItem('operarioSesionFormatos', this.operarioSeleccionadoLogin);
                this.mostrarModalLoginOperario = false;
                this.cargar();
            });
        } else {
            this.operarioSesion = this.operarioSeleccionadoLogin;
            sessionStorage.setItem('operarioSesionFormatos', this.operarioSeleccionadoLogin);
            this.mostrarModalLoginOperario = false;
            this.cargar();
        }
    }
    cambiarOperario() {
        this.operarioSesion = null;
        this.operarioSeleccionadoLogin = '';
        sessionStorage.removeItem('operarioSesionFormatos');
        this.mostrarModalLoginOperario = true;
    }
    ngOnDestroy(): void {
        this.detenerAutoguardado();
    }

    cargarConfiguracion() {
        this.service.getCaracteristicas(this.codigoFormato).subscribe({
            next: (data) => {
                this.tipoFormato = data;
                this.caracteristicas = data.caracteristicas;
                this.inicializarResultados();
                this.cargarOpciones();
                this.cargar();
            },
            error: (e) => console.error('Error cargando configuracion', e)
        });
    }

    cargarOpciones() {
        const tipoId = this.tipoFormato?.id;
        this.opcionesService.getOpciones('Maquina', tipoId).subscribe({ next: (d) => this.opcionesMaquina = d, error: (e) => console.error(e) });
        this.opcionesService.getOpciones('Corona', tipoId).subscribe({ next: (d) => this.opcionesCorona = d, error: (e) => console.error(e) });
        this.opcionesService.getOpciones('Molde', tipoId).subscribe({ next: (d) => this.opcionesMolde = d, error: (e) => console.error(e) });
        this.opcionesService.getOpciones('Operario', tipoId).subscribe({ next: (d) => this.opcionesOperario = d, error: (e) => console.error(e) });
        this.opcionesService.getOpciones('Supervisor', tipoId).subscribe({ next: (d) => this.opcionesSupervisor = d, error: (e) => console.error(e) });
    }

    inicializarResultados() {
        this.rondas = [];
        this.observaciones = {};
        this.tieneRondaFinal = false;
        for (const c of this.caracteristicas) {
            this.observaciones[c.id] = '';
        }
    }

    operarioRondaNueva = '';
    operarioActualNombre = '';
    intervaloAutoguardado: any = null;
    registrosOperarioDisponibles: any[] = [];
    esSupervisor = false;
    fotosSeleccionadasRonda: { [rondaIndex: number]: File[] } = {};
    previewsFotosRonda: { [rondaIndex: number]: string[] } = {};
    subiendoFotosRonda: number | null = null;
    generandoPdf = false;
    parametrosVer: { etiqueta: string; valor: string }[] = [];
    motivoVer: string | null = null;
    operariosVer: { nombre: string; fecha: string; motivo: string | null; parametros: { etiqueta: string; valor: string }[] }[] = [];
    mezclaVer: any[] = [];
    fotosRondas: { id: number; rondaId: string; archivo: string }[] = [];
    readonly maxFotosRonda = 5;

    agregarRondaHora() {
        if (this.tieneRondaFinal) { alert('Ya se marcó la verificación final, no se pueden agregar más horas'); return; }
        if (!this.operarioRondaNueva) { alert('Seleccione el operario que realiza esta verificación'); return; }

        const operarioSeleccionado = this.opcionesSupervisor.find(o => o.valor === this.operarioRondaNueva);

        if (operarioSeleccionado?.codigo) {
            const dialogRef = this.dialog.open(DialogInput, {
                data: {
                    titulo: `Verificación de identidad`,
                    mensaje: `Ingrese el código de ${this.operarioRondaNueva} para continuar`,
                    label: 'Código',
                    placeholder: 'Ej: 1234',
                    textoConfirmar: 'Confirmar'
                }
            });

            dialogRef.afterClosed().subscribe((codigoIngresado: string | null) => {
                if (codigoIngresado === null) return;
                if (codigoIngresado.trim() !== operarioSeleccionado.codigo) {
                    alert('Código incorrecto. No se agregó la verificación.');
                    return;
                }
                this.crearRondaHora();
            });
        } else {
            this.crearRondaHora();
        }
    }

    seleccionarRegistroOperario(rondaIndex: number, registro: any) {
        if (!registro || !registro.variablesCriticasJson) return;
        const ronda: any = this.rondas[rondaIndex];
        ronda.operarioRegistroId = registro.id;
        ronda.operario = registro.operarioNombre;
        ronda.parametrosOperario = JSON.parse(registro.variablesCriticasJson);
    }
    // Reduce la foto a máximo 1600 px y calidad 80 %: pesa unos 200–400 KB en vez de varios MB
    private comprimirImagen(file: File, maxLado = 1600, calidad = 0.8): Promise<File> {
        return new Promise((resolve) => {
            const reader = new FileReader();
            reader.onerror = () => resolve(file);
            reader.onload = () => {
                const img = new Image();
                img.onerror = () => resolve(file);
                img.onload = () => {
                    const escala = Math.min(1, maxLado / Math.max(img.width, img.height));
                    const w = Math.round(img.width * escala);
                    const h = Math.round(img.height * escala);
                    const canvas = document.createElement('canvas');
                    canvas.width = w;
                    canvas.height = h;
                    const ctx = canvas.getContext('2d');
                    if (!ctx) { resolve(file); return; }
                    ctx.drawImage(img, 0, 0, w, h);
                    canvas.toBlob(blob => {
                        if (!blob) { resolve(file); return; }
                        const nombre = file.name.replace(/\.[^.]+$/, '') + '.jpg';
                        resolve(new File([blob], nombre, { type: 'image/jpeg' }));
                    }, 'image/jpeg', calidad);
                };
                img.src = reader.result as string;
            };
            reader.readAsDataURL(file);
        });
    }

    // Fotos ya guardadas de esa hora
    fotosGuardadasRonda(i: number): { id?: number; archivo: string }[] {
        const ronda: any = this.rondas[i];
        if (!ronda) return [];
        const delJson = ((ronda.fotos as string[]) ?? []).map(a => ({ archivo: a }));
        const deTabla = this.fotosRondas.filter(f => f.rondaId === ronda.id).map(f => ({ id: f.id, archivo: f.archivo }));
        return [...delJson, ...deTabla];
    }

    // Cuántas fotos más caben en esa hora (guardadas + pendientes no pueden pasar de 5)
    cupoFotosRonda(i: number): number {
        const pendientes = this.fotosSeleccionadasRonda[i]?.length ?? 0;
        return Math.max(0, this.maxFotosRonda - this.fotosGuardadasRonda(i).length - pendientes);
    }

    totalFotosRonda(i: number): number {
        return this.fotosGuardadasRonda(i).length + (this.fotosSeleccionadasRonda[i]?.length ?? 0);
    }

    async seleccionarFotosRonda(event: any, rondaIndex: number) {
        const archivos: File[] = Array.from(event.target.files || []);
        event.target.value = '';   // permite elegir la misma foto otra vez
        if (archivos.length === 0) return;

        const cupo = this.cupoFotosRonda(rondaIndex);
        if (cupo === 0) {
            alert('Esta hora ya tiene el máximo de ' + this.maxFotosRonda + ' fotos');
            return;
        }
        if (archivos.length > cupo) {
            alert('Solo puede agregar ' + cupo + ' foto(s) más en esta hora (máximo ' + this.maxFotosRonda + ')');
        }

        if (!this.fotosSeleccionadasRonda[rondaIndex]) this.fotosSeleccionadasRonda[rondaIndex] = [];
        if (!this.previewsFotosRonda[rondaIndex]) this.previewsFotosRonda[rondaIndex] = [];

        for (const f of archivos.slice(0, cupo)) {
            const comprimida = await this.comprimirImagen(f);
            this.fotosSeleccionadasRonda[rondaIndex].push(comprimida);
            this.previewsFotosRonda[rondaIndex].push(URL.createObjectURL(comprimida));
        }
    }
    // Identificador estable de cada hora: así las fotos no cambian de hora si alguien quita una
    private nuevoRondaId(): string {
        return 'r' + Date.now().toString(36) + Math.random().toString(36).slice(2, 8);
    }

    // Las horas guardadas antes de este cambio no tienen identificador: se les asigna uno.
    // Queda guardado en el registro la primera vez que se suben fotos (ver guardarVerificacionRonda)
    private asegurarIdsRondas() {
        (this.rondas as any[]).forEach(r => { if (!r.id) r.id = this.nuevoRondaId(); });
    }

    cargarFotosRondas(registroId: number) {
        this.service.getFotosRondas(registroId).subscribe({
            next: (d) => this.fotosRondas = d ?? [],
            error: () => this.fotosRondas = []
        });
    }

    // Todas las fotos de una hora: las que venían dentro del registro y las de la tabla
    fotosDeRonda(ronda: any): string[] {
        const delJson: string[] = ronda?.fotos ?? [];
        const deTabla = this.fotosRondas.filter(f => f.rondaId === ronda?.id).map(f => f.archivo);
        return [...delJson, ...deTabla];
    }

    eliminarFotoRondaGuardada(f: { id?: number; archivo: string }) {
        if (!f.id || !this.editandoId) return;
        if (!confirm('¿Eliminar esta foto?')) return;
        this.service.eliminarFotoRonda(this.editandoId, f.id).subscribe({
            next: () => { this.fotosRondas = this.fotosRondas.filter(x => x.id !== f.id); },
            error: (e) => alert(e?.error?.mensaje || 'No se pudo eliminar la foto')
        });
    }

    quitarFotoPendienteRonda(rondaIndex: number, k: number) {
        const previews = this.previewsFotosRonda[rondaIndex] ?? [];
        if (previews[k]) URL.revokeObjectURL(previews[k]);
        previews.splice(k, 1);
        (this.fotosSeleccionadasRonda[rondaIndex] ?? []).splice(k, 1);
    }

    // Quita de pendientes las primeras N (las que ya se subieron)
    private quitarPrimerasPendientes(rondaIndex: number, cantidad: number) {
        const previews = this.previewsFotosRonda[rondaIndex] ?? [];
        previews.slice(0, cantidad).forEach(u => URL.revokeObjectURL(u));
        this.previewsFotosRonda[rondaIndex] = previews.slice(cantidad);
        this.fotosSeleccionadasRonda[rondaIndex] = (this.fotosSeleccionadasRonda[rondaIndex] ?? []).slice(cantidad);
    }

    private limpiarFotosRondaPendientes() {
        Object.values(this.previewsFotosRonda).forEach(lista => lista.forEach(u => URL.revokeObjectURL(u)));
        this.fotosSeleccionadasRonda = {};
        this.previewsFotosRonda = {};
    }

    // Sube las fotos de a una: cada una pesa poco y no se depende del límite de tamaño de Nginx
    async guardarVerificacionRonda(rondaIndex: number) {
        const lote = [...(this.fotosSeleccionadasRonda[rondaIndex] ?? [])];
        if (lote.length === 0) {
            alert('Seleccione al menos una foto antes de guardar');
            return;
        }
        if (!this.editandoId) { alert('Guarde primero el registro'); return; }
        if (this.subiendoFotosRonda !== null) return;
        this.subiendoFotosRonda = rondaIndex;

        try {
            // Primero se guarda el registro con el identificador de esta hora; sin eso las fotos quedarían sin hora
            this.asegurarIdsRondas();
            if (!(await this.persistirResultadosActuales())) return;

            const rondaId = (this.rondas[rondaIndex] as any).id as string;
            let subidas = 0;

            for (const f of lote) {
                try {
                    const res: any[] = await firstValueFrom(this.service.subirFotosRonda(this.editandoId, rondaId, [f]));
                    (res ?? []).forEach(x => this.fotosRondas.push({ id: x.id, rondaId: x.rondaId, archivo: x.archivo }));
                    subidas++;
                } catch (e: any) {
                    console.error(e);
                    alert(e?.error?.mensaje || 'Error subiendo una foto');
                    break;
                }
            }

            if (subidas > 0) {
                this.quitarPrimerasPendientes(rondaIndex, subidas);
                if (subidas === lote.length) alert('Fotos guardadas correctamente');
            }
        } finally {
            this.subiendoFotosRonda = null;
        }
    }

    private async persistirResultadosActuales(): Promise<boolean> {
        if (!this.editandoId) return false;

        const resultadosData = {
            rondas: this.rondas,
            observaciones: this.observaciones,
            caracteristicas: this.caracteristicas.map(c => ({ id: c.id, descripcion: c.descripcion }))
        };

        const dto = {
            tipoFormatoId: this.tipoFormato.id,
            ordenProduccion: this.form.ordenProduccion,
            cliente: this.form.cliente,
            referencia: this.form.referencia,
            operarios: this.form.operarios,
            hora: this.form.hora,
            maquina: this.form.maquina,
            variablesCriticasJson: this.tipoFormato.tieneVariablesCriticas
                ? JSON.stringify({ ...this.variablesCriticas, motivoCambio: this.motivoCambioParametros })
                : null,
            resultadosJson: JSON.stringify(resultadosData)
        };

        try {
            await firstValueFrom(this.service.editarRegistro(this.editandoId, dto));
            return true;
        } catch (e) {
            console.error(e);
            alert('Error guardando la verificación');
            return false;
        }
    }

    private crearRondaHora() {
        const ahora = new Date();
        const horaTexto = ahora.toLocaleTimeString('es-CO', { hour: '2-digit', minute: '2-digit', hour12: true });

        const valores: { [key: number]: 'cumple' | 'noCumple' | 'na' | null } = {};
        const valoresNumericos: { [key: number]: string } = {};
        const rondaAnterior = this.rondas.length > 0 ? this.rondas[this.rondas.length - 1] : null;

        for (const c of this.caracteristicas) {
            valores[c.id] = (rondaAnterior && rondaAnterior.valores[c.id] === 'na') ? 'na' : null;
            valoresNumericos[c.id] = '';
        }

        this.rondas.push({
            id: this.nuevoRondaId(),
            hora: horaTexto, final: false, operario: this.operarioRondaNueva,
            cierreDeOperario: false, kilosDesperdicio: '', valores, valoresNumericos
        });
        this.operarioRondaNueva = '';
    }

    cerrarTurnoOperario(indice: number) {
        const ronda = this.rondas[indice];
        const dialogRef = this.dialog.open(DialogInput, {
            data: {
                titulo: `Cerrar turno de ${ronda.operario}`,
                label: 'Ingrese los kilos de desperdicio',
                placeholder: 'Ej: 5.5',
                textoConfirmar: 'Aceptar'
            }
        });

        dialogRef.afterClosed().subscribe((kilos: string | null) => {
            if (kilos === null) return;
            ronda.cierreDeOperario = true;
            ronda.kilosDesperdicio = kilos;
        });
    }
    anchoColumnaHora(): string {
        const totalRondas = this.rondas.length || 1;
        // Reserva 18% para Característica y 20% para Observaciones, el resto se reparte entre las horas
        const disponible = 62;
        const porColumna = disponible / totalRondas;
        return `${Math.max(porColumna, 6)}%`; // nunca menos de 6% para que no desaparezca
    }
    esCaracteristicaNumerica(descripcion: string): boolean {
        const desc = descripcion.toLowerCase();
        return desc.includes('medida de película') || desc.includes('medida de pelicula') ||
            desc.includes('calibre de película') || desc.includes('calibre de pelicula');
    }

    marcarRondaFinal(indice: number) {
        const dialogRef = this.dialog.open(DialogConfirmacion, {
            data: {
                titulo: 'Marcar verificación final',
                mensaje: '¿Marcar esta hora como la verificación FINAL? Ya no se podrán agregar más horas.',
                textoConfirmar: 'Aceptar',
                textoCancelar: 'Cancelar',
                tipo: 'warning'
            }
        });

        dialogRef.afterClosed().subscribe((confirmado: boolean) => {
            if (!confirmado) return;
            this.rondas.forEach((r, i) => r.final = (i === indice));
            this.tieneRondaFinal = true;
        });
    }

    quitarRonda(indice: number) {
        const dialogRef = this.dialog.open(DialogConfirmacion, {
            data: {
                titulo: 'Quitar verificación',
                mensaje: '¿Quitar esta ronda de verificación?',
                textoConfirmar: 'Quitar',
                textoCancelar: 'Cancelar',
                tipo: 'warning'
            }
        });

        dialogRef.afterClosed().subscribe((confirmado: boolean) => {
            if (!confirmado) return;
            this.rondas.splice(indice, 1);
            if (!this.rondas.some(r => r.final)) this.tieneRondaFinal = false;
        });
    }

    marcarValor(rondaIndice: number, caracteristicaId: number, valor: 'cumple' | 'noCumple' | 'na') {
        const actual = this.rondas[rondaIndice].valores[caracteristicaId];
        const nuevoValor = (actual === valor) ? null : valor;
        this.rondas[rondaIndice].valores[caracteristicaId] = nuevoValor;

        if (nuevoValor === 'noCumple') {
            const dialogRef = this.dialog.open(DialogConfirmacion, {
                data: {
                    titulo: 'Material fuera de especificación',
                    mensaje: 'Este lote debe separarse de inmediato. Informa a producción o al jefe de planta antes de continuar.',
                    textoConfirmar: 'Entendido, separar',
                    textoCancelar: 'Deshacer',
                    tipo: 'danger'
                }
            });

            dialogRef.afterClosed().subscribe((confirmado: boolean) => {
                if (!confirmado) {
                    this.rondas[rondaIndice].valores[caracteristicaId] = null;
                }
            });
        }
    }

    cerrarModalNoCumple(revertir: boolean) {
        if (revertir && this.rondaModalPendiente !== null && this.caracteristicaModalPendiente !== null) {
            this.rondas[this.rondaModalPendiente].valores[this.caracteristicaModalPendiente] = null;
        }
        this.mostrarModalNoCumple = false;
        this.rondaModalPendiente = null;
        this.caracteristicaModalPendiente = null;
    }

    cargar() {
        this.cargando = true;
        this.service.getRegistros(this.codigoFormato, this.filtroDesde, this.filtroHasta, this.filtroOP).subscribe({
            next: (d) => {
                this.registros = (this.esSupervisor || !this.operarioSesion)
                    ? d
                    : d.filter((r: any) => (r.operarios || '').includes(this.operarioSesion!));
                this.cargando = false;
            },
            error: (e) => { console.error(e); this.cargando = false; }
        });
    }

    buscarDesperdicio() {
        if (!this.filtroOPDesperdicio || !this.tipoFormato) return;
        this.service.buscarDesperdicioPorOrden(this.filtroOPDesperdicio, this.tipoFormato.id).subscribe({
            next: (data) => { this.resultadoDesperdicio = data; },
            error: () => { this.resultadoDesperdicio = null; alert('No se encontró información para esa orden'); }
        });
    }

    get ventanaHoraActual(): string {
        const ahora = new Date();
        const horaActual = ahora.getHours();
        const inicio = horaActual % 12 === 0 ? 12 : horaActual % 12;
        const finHora = (horaActual + 1) % 24;
        const fin = finHora % 12 === 0 ? 12 : finHora % 12;
        const sufijoInicio = horaActual < 12 ? 'am' : 'pm';
        const sufijoFin = finHora < 12 ? 'am' : 'pm';
        return `${inicio}:00 ${sufijoInicio} - ${fin}:00 ${sufijoFin}`;
    }

    get faltaVerificacionEstaHora(): boolean {
        if (this.tieneRondaFinal || this.rondas.length === 0) return false;
        const ultimaRonda = this.rondas[this.rondas.length - 1];
        return this.caracteristicas.some(c => !ultimaRonda.valores[c.id]);
    }

    get totalKilosDesperdicioVer(): number {
        if (!this.registroSeleccionado?.rondasParsed) return 0;
        return this.registroSeleccionado.rondasParsed
            .filter((r: any) => r.cierreDeOperario)
            .reduce((sum: number, r: any) => sum + (parseFloat(r.kilosDesperdicio) || 0), 0);
    }
    buscarOP() {
        if (!this.form.ordenProduccion || !this.tipoFormato) return;
        this.cargarMezcla();
        this.service.buscarOP(this.form.ordenProduccion, this.tipoFormato.id).subscribe({
            next: (data: any) => {
                if (data.estado === 'Liberado') {
                    alert('Esta orden ya fue liberada/cerrada. No se puede continuar trabajando en ella.');
                    this.opExistente = null;
                    return;
                }
                this.opExistente = true;
                this.editar(data);
                if (this.operarioSesion) this.form.operarios = this.operarioSesion;
            },
            error: () => {
                this.opExistente = null;
                this.ordenesService.buscar(this.form.ordenProduccion).subscribe({
                    next: (data: any) => {
                        this.form.cliente = data.cliente;
                        this.form.referencia = data.referencia;
                        this.clienteDetectado = data.cliente;
                        this.referenciaDetectada = data.referencia;
                        this.descripcionDetectada = data.descripcion;
                        this.cantidadDetectada = data.cantidadOP;
                        this.unidadDetectada = data.unidad;
                    },
                    error: () => { this.clienteDetectado = null; this.referenciaDetectada = null; this.descripcionDetectada = null; this.cantidadDetectada = null; this.unidadDetectada = null; }
                });
            }
        });
    }
    cargarMezcla() {
        const op = (this.form.ordenProduccion || '').trim();
        this.mezclaOrden = [];
        if (!op) return;
        this.service.getMezclaOrden(op).subscribe({
            next: (d) => {
                // Si en el campo ya se escribió otra orden, esta respuesta se descarta
                if (op === (this.form.ordenProduccion || '').trim()) this.mezclaOrden = d ?? [];
            },
            error: () => { this.mezclaOrden = []; }
        });
    }

    nombreProceso(codigo: string): string {
        const mapa: { [k: string]: string } = { PEXTR: 'Extrusión', PDOBL: 'Doblado', PSELL: 'Sellado' };
        return mapa[(codigo || '').toUpperCase()] ?? codigo;
    }
    sugerenciaAplicada = false;
    valoresOriginalesSugeridos: any = null;
    motivoCambioParametros: string | null = null;

    buscarSugerenciaMejorRendimiento() {
        if (!this.tipoFormato?.tieneVariablesCriticas) return; // solo aplica a Extrusión
        if (!this.form.referencia || !this.form.maquina) return;

        this.service.buscarMejorRendimiento(this.form.referencia, this.form.maquina).subscribe({
            next: (data: any) => {
                if (!data.mejor || !data.mejor.variablesCriticasJson) return;
                const vc = JSON.parse(data.mejor.variablesCriticasJson);
                this.variablesCriticas = vc;
                this.sugerenciaAplicada = true;
                this.valoresOriginalesSugeridos = JSON.parse(JSON.stringify(vc));
                this.motivoCambioParametros = null;
            },
            error: () => { /* sin historial, no pasa nada */ }
        });
    }

    onCambioVariableCritica() {
        if (!this.sugerenciaAplicada || !this.valoresOriginalesSugeridos) return;

        const cambiaron = JSON.stringify(this.variablesCriticas) !== JSON.stringify(this.valoresOriginalesSugeridos);

        if (cambiaron && !this.motivoCambioParametros) {
            const dialogRef = this.dialog.open(DialogInput, {
                data: {
                    titulo: 'Cambio en los parámetros sugeridos',
                    mensaje: 'Se detectó un cambio en los parámetros sugeridos.',
                    label: 'Explique el motivo del cambio',
                    placeholder: 'Ej: Ajuste por variación de material',
                    textoConfirmar: 'Aceptar'
                }
            });

            dialogRef.afterClosed().subscribe((motivo: string | null) => {
                this.motivoCambioParametros = motivo?.trim() || 'No especificado';
            });
        }
    }
    verificarSugerenciaAlAbrir() {
        if (!this.tipoFormato?.tieneVariablesCriticas) return;
        if (!this.form.referencia || !this.form.maquina) return;

        this.service.buscarMejorRendimiento(this.form.referencia, this.form.maquina).subscribe({
            next: (data: any) => {
                if (!data.mejor || !data.mejor.variablesCriticasJson) return;
                const sugerencia = JSON.parse(data.mejor.variablesCriticasJson);

                this.sugerenciaAplicada = true;
                this.valoresOriginalesSugeridos = JSON.parse(JSON.stringify(sugerencia));

                const motivoYaRegistrado = this.motivoCambioParametros;
                const difiereDeLaSugerencia = JSON.stringify(this.variablesCriticas) !== JSON.stringify(sugerencia);

                if (difiereDeLaSugerencia && !motivoYaRegistrado) {
                    const dialogRef = this.dialog.open(DialogInput, {
                        data: {
                            titulo: 'Los parámetros no coinciden con la sugerencia',
                            mensaje: 'Los valores actuales de esta orden son distintos a la mejor versión/fijada para esta referencia y máquina.',
                            label: 'Explique el motivo del cambio',
                            placeholder: 'Ej: Ajuste por variación de material',
                            textoConfirmar: 'Aceptar'
                        }
                    });

                    dialogRef.afterClosed().subscribe((motivo: string | null) => {
                        this.motivoCambioParametros = motivo?.trim() || 'No especificado';
                    });
                }
            },
            error: () => { /* sin historial, no pasa nada */ }
        });
    }
    nuevo() {
        const ahora = new Date();
        const horaTexto = ahora.toLocaleTimeString('es-CO', { hour: '2-digit', minute: '2-digit', hour12: true });
        this.form = {
            ordenProduccion: '', cliente: '', referencia: '', operarios: '',
            hora: horaTexto, maquina: '', puedeLiberarse: null, explicacionNoLiberado: '', cargoFirma: '', produccionKgHora: '',
            desperdicioTotalKg: null,
            fechaRealProduccion: ''
        };
        this.limpiarFotosRondaPendientes();
        this.form.operarios = this.operarioSesion || '';
        this.inicializarResultados();
        this.variablesCriticas = {
            corona: '', molde: '',
            temperaturas: { zona1: '', zona2: '', zona3: '', zona4: '', zona5: '', zona6: '' },
            velocidades: { maquina: '', halador: '', bobinador: '' },
            aire: '', amperaje: '', alturaBurbuja: '', produccionKgHora: ''
        };
        this.firmaDataUrl = null;
        this.opExistente = null;
        this.clienteDetectado = null;
        this.mezclaOrden = [];
        this.editandoId = null;
        this.vista = 'nuevo';
        this.fotosRondas = [];
        this.iniciarAutoguardado();

        setTimeout(() => this.iniciarCanvas(), 300);
    }
    capturarEstadoActual(): any {
        return {
            form: { ...this.form },
            variablesCriticas: JSON.parse(JSON.stringify(this.variablesCriticas)),
            rondas: JSON.parse(JSON.stringify(this.rondas)),
            observaciones: { ...this.observaciones },
            tieneRondaFinal: this.tieneRondaFinal,
            editandoId: this.editandoId,
            firmaDataUrl: this.firmaDataUrl,
            vista: this.vista,
            opExistente: this.opExistente,
            sugerenciaAplicada: this.sugerenciaAplicada,
            valoresOriginalesSugeridos: this.valoresOriginalesSugeridos ? JSON.parse(JSON.stringify(this.valoresOriginalesSugeridos)) : null,
            motivoCambioParametros: this.motivoCambioParametros,
            registrosOperarioDisponibles: this.registrosOperarioDisponibles
        };
    }
    restaurarEstado(estado: any) {
        this.limpiarFotosRondaPendientes();
        this.form = { ...estado.form };
        this.variablesCriticas = JSON.parse(JSON.stringify(estado.variablesCriticas));
        this.rondas = JSON.parse(JSON.stringify(estado.rondas));
        this.observaciones = { ...estado.observaciones };
        this.tieneRondaFinal = estado.tieneRondaFinal;
        this.editandoId = estado.editandoId;
        this.asegurarIdsRondas();
        if (this.editandoId) this.cargarFotosRondas(this.editandoId); else this.fotosRondas = [];
        this.firmaDataUrl = estado.firmaDataUrl;
        this.vista = estado.vista;
        this.opExistente = estado.opExistente;
        this.sugerenciaAplicada = estado.sugerenciaAplicada;
        this.valoresOriginalesSugeridos = estado.valoresOriginalesSugeridos ? JSON.parse(JSON.stringify(estado.valoresOriginalesSugeridos)) : null;
        this.motivoCambioParametros = estado.motivoCambioParametros;
        this.registrosOperarioDisponibles = estado.registrosOperarioDisponibles;

        setTimeout(() => this.iniciarCanvas(), 300);
    }
    abrirNuevaPestana() {
        if (this.pestanaActivaIndice >= 0) {
            this.pestanasAbiertas[this.pestanaActivaIndice].estado = this.capturarEstadoActual();
        }

        this.nuevo();
        this.pestanasAbiertas.push({ titulo: 'Nueva orden', estado: this.capturarEstadoActual() });
        this.pestanaActivaIndice = this.pestanasAbiertas.length - 1;

    }

    cambiarPestana(indice: number) {
        if (indice === this.pestanaActivaIndice) return;

        if (this.pestanaActivaIndice >= 0) {
            this.pestanasAbiertas[this.pestanaActivaIndice].estado = this.capturarEstadoActual();
            this.pestanasAbiertas[this.pestanaActivaIndice].titulo = this.form.ordenProduccion
                ? `OP ${this.form.ordenProduccion}${this.form.maquina ? ' — ' + this.form.maquina : ''}`
                : 'Nueva orden';
        }

        this.pestanaActivaIndice = indice;
        this.restaurarEstado(this.pestanasAbiertas[indice].estado);
    }

    cerrarPestana(indice: number, event: Event) {
        event.stopPropagation();
        this.pestanasAbiertas.splice(indice, 1);

        if (this.pestanasAbiertas.length === 0) {
            this.pestanaActivaIndice = -1;
            this.vista = 'lista';
            return;
        }

        const nuevoIndice = indice >= this.pestanasAbiertas.length ? this.pestanasAbiertas.length - 1 : indice;
        this.cambiarPestana(nuevoIndice === this.pestanaActivaIndice ? -1 : nuevoIndice);
    }
    iniciarCanvas() {
        const canvas = document.getElementById('firmaCanvasGenerico') as HTMLCanvasElement;
        if (!canvas) return;
        const ctx = canvas.getContext('2d')!;
        let dibujando = false;
        canvas.addEventListener('mousedown', e => { dibujando = true; ctx.beginPath(); ctx.moveTo(e.offsetX, e.offsetY); });
        canvas.addEventListener('mousemove', e => { if (!dibujando) return; ctx.lineTo(e.offsetX, e.offsetY); ctx.stroke(); });
        canvas.addEventListener('mouseup', () => { dibujando = false; this.firmaDataUrl = canvas.toDataURL(); });
        canvas.addEventListener('touchstart', e => { e.preventDefault(); dibujando = true; const t = e.touches[0]; const r = canvas.getBoundingClientRect(); ctx.beginPath(); ctx.moveTo(t.clientX - r.left, t.clientY - r.top); });
        canvas.addEventListener('touchmove', e => { e.preventDefault(); if (!dibujando) return; const t = e.touches[0]; const r = canvas.getBoundingClientRect(); ctx.lineTo(t.clientX - r.left, t.clientY - r.top); ctx.stroke(); });
        canvas.addEventListener('touchend', () => { dibujando = false; this.firmaDataUrl = canvas.toDataURL(); });
    }
    urlFotoRonda(nombreArchivo: string): string {
        return `${environment.fotosUrl}/formatos-calidad/${nombreArchivo}`;
    }
    parsearJson(json: string | null): any {
        if (!json) return null;
        try { return JSON.parse(json); } catch { return null; }
    }
    // Convierte las variables críticas (de cualquier formato) en una lista legible; omite lo vacío
    parametrosLista(vc: any): { etiqueta: string; valor: string }[] {
        const lista: { etiqueta: string; valor: string }[] = [];
        if (!vc) return lista;
        const poner = (etiqueta: string, valor: any) => {
            if (valor === null || valor === undefined) return;
            const t = String(valor).trim();
            if (t !== '') lista.push({ etiqueta, valor: t });
        };

        poner('Molde', vc.corona);
        poner('Medida del molde', vc.molde);
        for (let z = 1; z <= 6; z++) poner('Zona ' + z + ' (°C)', vc.temperaturas?.['zona' + z]);
        poner('Motor / Motor 1 (Hz)', vc.velocidades?.maquina);
        poner('Motor 2 (Hz)', vc.velocidades?.maquina2);
        poner('Halador (Hz)', vc.velocidades?.halador);
        poner('Bobinador (Hz)', vc.velocidades?.bobinador);
        poner('Aire (H)', vc.aire);
        poner('Amperaje (A)', vc.amperaje);
        poner('Altura burbuja (cm)', vc.alturaBurbuja);
        // Antes de corregirlo, "Vela" se guardaba en produccionKgHora
        const vela = vc.vela || (['Con vela', 'Sin vela'].includes(vc.produccionKgHora) ? vc.produccionKgHora : '');
        poner('Vela', vela);
        // Otros formatos
        poner('Máquina', vc.maquinaImpresion || vc.maquinaSellado || vc.maquinaPrecorte);
        poner('Metros por minuto', vc.metrosPorMinuto);
        poner('Velocidad máquina (Hz)', vc.velocidadMaquinaHz);
        poner('Golpes por minuto', vc.golpesPorMinuto);
        poner('Bolsas por minuto', vc.bolsasPorMinuto);
        poner('Temp. superior (°C)', vc.temperaturaSuperior);
        poner('Temp. inferior (°C)', vc.temperaturaInferior);
        poner('Temperatura (°C)', vc.temperaturaPrecorte);
        return lista;
    }
    limpiarFirma() {
        const canvas = document.getElementById('firmaCanvasGenerico') as HTMLCanvasElement;
        if (canvas) canvas.getContext('2d')!.clearRect(0, 0, canvas.width, canvas.height);
        this.firmaDataUrl = null;
    }

    guardar() {
        if (!this.form.ordenProduccion.trim()) { alert('Ingrese la orden de produccion'); return; }

        const resultadosData = {
            rondas: this.rondas,
            observaciones: this.observaciones,
            caracteristicas: this.caracteristicas.map(c => ({ id: c.id, descripcion: c.descripcion }))
        };

        const dto = {
            tipoFormatoId: this.tipoFormato.id,
            ordenProduccion: this.form.ordenProduccion,
            cliente: this.form.cliente,
            referencia: this.form.referencia,
            operarios: this.form.operarios,
            hora: this.form.hora,
            maquina: this.form.maquina,
            variablesCriticasJson: this.tipoFormato.tieneVariablesCriticas
                ? JSON.stringify({ ...this.variablesCriticas, motivoCambio: this.motivoCambioParametros })
                : null,
            resultadosJson: JSON.stringify(resultadosData),
            fechaRealProduccion: this.form.fechaRealProduccion || null
        };

        const peticion = this.editandoId
            ? this.service.editarRegistro(this.editandoId, dto)
            : this.service.crearRegistro(dto);

        peticion.subscribe({
            next: (data: any) => {
                if (!this.editandoId) this.editandoId = data.id;
                this.autoguardarParametrosOperario();
                this.iniciarAutoguardado();
                this.vista = 'lista'; this.editandoId = null; this.cargar();
                this.detenerAutoguardado();
            },
            error: (e) => { console.error(e); alert('Error guardando el registro'); }
        });
    }

    iniciarAutoguardado() {
        this.detenerAutoguardado();
        this.intervaloAutoguardado = setInterval(() => {
            this.autoguardarTodasLasPestanas();
        }, 10 * 60 * 1000);
    }

    detenerAutoguardado() {
        if (this.intervaloAutoguardado) {
            clearInterval(this.intervaloAutoguardado);
            this.intervaloAutoguardado = null;
        }
    }

    autoguardarParametrosOperario() {
        if (!this.form.operarios || !this.form.ordenProduccion) return;

        if (!this.editandoId) {
            const dto = {
                tipoFormatoId: this.tipoFormato.id,
                ordenProduccion: this.form.ordenProduccion,
                cliente: this.form.cliente,
                referencia: this.form.referencia,
                operarios: this.form.operarios,
                hora: this.form.hora,
                maquina: this.form.maquina,
                variablesCriticasJson: this.tipoFormato.tieneVariablesCriticas
                    ? JSON.stringify({ ...this.variablesCriticas, motivoCambio: this.motivoCambioParametros })
                    : null,
                resultadosJson: JSON.stringify({
                    rondas: this.rondas,
                    observaciones: this.observaciones,
                    caracteristicas: this.caracteristicas.map(c => ({ id: c.id, descripcion: c.descripcion }))
                })
            };
            this.service.crearRegistro(dto).subscribe({
                next: (data: any) => {
                    this.editandoId = data.id;
                    this.guardarParametrosOperarioReal();
                },
                error: (e) => console.error('Error creando registro en autoguardado', e)
            });
            return;
        }

        this.guardarParametrosOperarioReal();
    }
    autoguardarTodasLasPestanas() {
        if (this.pestanaActivaIndice >= 0 && this.pestanasAbiertas[this.pestanaActivaIndice]) {
            this.pestanasAbiertas[this.pestanaActivaIndice].estado = this.capturarEstadoActual();
        }

        this.pestanasAbiertas.forEach((pestana, indice) => {
            this.autoguardarEstadoPestana(pestana.estado, indice);
        });
    }

    private autoguardarEstadoPestana(estado: any, indicePestana: number) {
        if (!estado.form.operarios || !estado.form.ordenProduccion) return;

        if (!estado.editandoId) {
            const dto = {
                tipoFormatoId: this.tipoFormato.id,
                ordenProduccion: estado.form.ordenProduccion,
                cliente: estado.form.cliente,
                referencia: estado.form.referencia,
                operarios: estado.form.operarios,
                hora: estado.form.hora,
                maquina: estado.form.maquina,
                variablesCriticasJson: this.tipoFormato.tieneVariablesCriticas
                    ? JSON.stringify({ ...estado.variablesCriticas, motivoCambio: estado.motivoCambioParametros })
                    : null,
                resultadosJson: JSON.stringify({
                    rondas: estado.rondas,
                    observaciones: estado.observaciones,
                    caracteristicas: this.caracteristicas.map(c => ({ id: c.id, descripcion: c.descripcion }))
                }),
                fechaRealProduccion: estado.form.fechaRealProduccion || null
            };
            this.service.crearRegistro(dto).subscribe({
                next: (data: any) => {
                    estado.editandoId = data.id;
                    if (indicePestana === this.pestanaActivaIndice) this.editandoId = data.id;
                    this.guardarParametrosOperarioDeEstado(estado);
                },
                error: (e) => console.error('Error creando registro en autoguardado', e)
            });
            return;
        }

        this.guardarParametrosOperarioDeEstado(estado);
    }

    private guardarParametrosOperarioDeEstado(estado: any) {
        this.service.guardarParametrosOperario(estado.editandoId, {
            operarioNombre: estado.form.operarios,
            variablesCriticasJson: JSON.stringify(estado.variablesCriticas),
            motivoCambio: estado.motivoCambioParametros
        }).subscribe({ error: (e) => console.error('Error en autoguardado', e) });
    }
    private guardarParametrosOperarioReal() {
        this.service.guardarParametrosOperario(this.editandoId!, {
            operarioNombre: this.form.operarios,
            variablesCriticasJson: JSON.stringify(this.variablesCriticas),
            motivoCambio: this.motivoCambioParametros
        }).subscribe({ error: (e) => console.error('Error en autoguardado', e) });
    }

    guardarLiberacion() {
        if (this.form.puedeLiberarse === null) { alert('Seleccione SI o NO'); return; }

        const dto = {
            puedeLiberarse: this.form.puedeLiberarse,
            explicacionNoLiberado: this.form.explicacionNoLiberado,
            firmaDigital: this.firmaDataUrl,
            cargoFirma: this.form.cargoFirma,
            produccionKgHora: this.form.produccionKgHora,
            desperdicioTotalKg: this.form.desperdicioTotalKg
        };

        this.service.liberarRegistro(this.editandoId!, dto).subscribe({
            next: () => { this.vista = 'lista'; this.editandoId = null; this.cargar(); },
            error: (e) => { console.error(e); alert('Error al liberar el registro'); }
        });
    }

    ver(r: any) {
        this.registroSeleccionado = r;
        const datos = JSON.parse(r.resultadosJson || '{"rondas":[],"observaciones":{},"caracteristicas":[]}');
        this.registroSeleccionado.rondasParsed = datos.rondas || [];
        this.registroSeleccionado.observacionesParsed = datos.observaciones || {};
        this.registroSeleccionado.caracteristicasParsed = datos.caracteristicas || [];

        const vc = this.parsearJson(r.variablesCriticasJson);
        this.registroSeleccionado.variablesCriticasParsed = vc;
        this.parametrosVer = this.parametrosLista(vc);
        this.motivoVer = vc?.motivoCambio ?? null;

        this.operariosVer = [];
        this.mezclaVer = [];
        this.fotosRondas = [];

        this.service.getParametrosOperario(r.id).subscribe({
            next: (lista: any[]) => {
                this.operariosVer = (lista ?? []).map(reg => {
                    const v = this.parsearJson(reg.variablesCriticasJson);
                    return {
                        nombre: reg.operarioNombre,
                        fecha: reg.fechaGuardado,
                        motivo: reg.motivoCambio ?? v?.motivoCambio ?? null,
                        parametros: this.parametrosLista(v)
                    };
                });
            },
            error: () => { this.operariosVer = []; }
        });

        this.service.getMezclaOrden(r.ordenProduccion).subscribe({
            next: (d: any[]) => { this.mezclaVer = d ?? []; },
            error: () => { this.mezclaVer = []; }
        });

        this.cargarFotosRondas(r.id);
        this.vista = 'ver';
    }
    editar(r: any) {
        this.editandoId = r.id;
        this.form = {
            ordenProduccion: r.ordenProduccion, cliente: r.cliente ?? '', referencia: r.referencia ?? '',
            operarios: r.operarios ?? '', hora: r.hora ?? '', maquina: r.maquina ?? '',
            puedeLiberarse: r.puedeLiberarse, explicacionNoLiberado: r.explicacionNoLiberado ?? '',
            cargoFirma: r.cargoFirma ?? '', produccionKgHora: r.produccionKgHora ?? '',
            desperdicioTotalKg: r.desperdicioTotalKg ?? null,
            fechaRealProduccion: ''
        };
        this.limpiarFotosRondaPendientes();
        this.inicializarResultados();
        const datosGuardados = JSON.parse(r.resultadosJson || '{"rondas":[],"observaciones":{}}');
        this.rondas = datosGuardados.rondas || [];
        this.asegurarIdsRondas();
        this.cargarFotosRondas(r.id);
        this.observaciones = datosGuardados.observaciones || {};
        this.tieneRondaFinal = this.rondas.some((rr: any) => rr.final);

        if (r.variablesCriticasJson) {
            const vcParsed = JSON.parse(r.variablesCriticasJson);
            this.motivoCambioParametros = vcParsed.motivoCambio ?? null;
            delete vcParsed.motivoCambio;
            this.variablesCriticas = vcParsed;
        }
        this.firmaDataUrl = r.firmaDigital ?? null;
        this.vista = 'editar';
        this.cargarFotosRondas(r.id);
        this.service.getParametrosOperario(r.id).subscribe({
            next: (data) => this.registrosOperarioDisponibles = data,
            error: (e) => console.error(e)
        });
        this.iniciarAutoguardado();
        this.cargarMezcla();
        this.verificarSugerenciaAlAbrir();
        setTimeout(() => this.iniciarCanvas(), 300);
    }

    abrirLiberar(r: any) {
        this.limpiarFotosRondaPendientes();
        this.editandoId = r.id;
        this.registroSeleccionado = r;
        this.form.puedeLiberarse = r.puedeLiberarse ?? null;
        this.form.explicacionNoLiberado = r.explicacionNoLiberado ?? '';
        this.form.cargoFirma = r.cargoFirma ?? '';
        this.form.produccionKgHora = r.produccionKgHora ?? '';
        this.form.desperdicioTotalKg = r.desperdicioTotalKg ?? null;
        this.firmaDataUrl = r.firmaDigital ?? null;


        this.inicializarResultados();
        const datosGuardados = JSON.parse(r.resultadosJson || '{"rondas":[],"observaciones":{}}');
        this.rondas = datosGuardados.rondas || [];
        this.asegurarIdsRondas();
        this.cargarFotosRondas(r.id);
        this.observaciones = datosGuardados.observaciones || {};
        this.tieneRondaFinal = this.rondas.some((rr: any) => rr.final);

        if (r.variablesCriticasJson) this.variablesCriticas = JSON.parse(r.variablesCriticasJson);

        this.service.getParametrosOperario(r.id).subscribe({
            next: (data) => this.registrosOperarioDisponibles = data,
            error: (e) => console.error(e)
        });
        this.vista = 'liberar';
        setTimeout(() => this.iniciarCanvas(), 300);
    }
    eliminar(id: number) {
        const dialogRef = this.dialog.open(DialogConfirmacion, {
            data: {
                titulo: 'Eliminar registro',
                mensaje: '¿Está seguro de eliminar este registro? Esta acción no se puede deshacer.',
                textoConfirmar: 'Eliminar',
                textoCancelar: 'Cancelar',
                tipo: 'danger'
            }
        });

        dialogRef.afterClosed().subscribe((confirmado: boolean) => {
            if (!confirmado) return;
            this.service.eliminarRegistro(id).subscribe({ next: () => this.cargar() });
        });
    }
    private imagenParaPdf(url: string, maxLado = 700, calidad = 0.7): Promise<{ data: string; w: number; h: number } | null> {
        return new Promise((resolve) => {
            const img = new Image();
            img.crossOrigin = 'anonymous';
            img.onload = () => {
                const escala = Math.min(1, maxLado / Math.max(img.width, img.height));
                const w = Math.max(1, Math.round(img.width * escala));
                const h = Math.max(1, Math.round(img.height * escala));
                const canvas = document.createElement('canvas');
                canvas.width = w;
                canvas.height = h;
                const ctx = canvas.getContext('2d');
                if (!ctx) { resolve(null); return; }
                ctx.fillStyle = '#ffffff';
                ctx.fillRect(0, 0, w, h);
                ctx.drawImage(img, 0, 0, w, h);
                resolve({ data: canvas.toDataURL('image/jpeg', calidad), w, h });
            };
            img.onerror = () => resolve(null);
            img.src = url + (url.includes('?') ? '&' : '?') + 't=' + Date.now();
        });
    }

    // PDF completo de un registro: datos, mezcla, parámetros, rondas, fotos de cada hora y liberación
    private async pdfRegistroCompleto(r: any) {
        if (this.generandoPdf) return;
        this.generandoPdf = true;
        try {
            const [mezcla, operarios, fotosTabla] = await Promise.all([
                firstValueFrom(this.service.getMezclaOrden(r.ordenProduccion)).catch(() => [] as any[]),
                firstValueFrom(this.service.getParametrosOperario(r.id)).catch(() => [] as any[]),
                firstValueFrom(this.service.getFotosRondas(r.id)).catch(() => [] as any[])
            ]);

            const datos = this.parsearJson(r.resultadosJson) ?? {};
            const rondas: any[] = datos.rondas || [];
            const caracteristicas: any[] = datos.caracteristicas || [];
            const observaciones: any = datos.observaciones || {};
            const vc = this.parsearJson(r.variablesCriticasJson);

            // Fotos de cada hora: las que venían dentro del registro y las de la tabla
            const fotosPorRonda: string[][] = rondas.map((ronda: any) => [
                ...((ronda.fotos as string[]) ?? []),
                ...fotosTabla.filter((f: any) => f.rondaId === ronda.id).map((f: any) => f.archivo)
            ]);

            // Las fotos se descargan antes de armar el documento (de 6 en 6)
            const archivos = Array.from(new Set(fotosPorRonda.flat()));
            const imagenes: { [archivo: string]: { data: string; w: number; h: number } | null } = {};
            for (let i = 0; i < archivos.length; i += 6) {
                const lote = archivos.slice(i, i + 6);
                const res = await Promise.all(lote.map(a => this.imagenParaPdf(this.urlFotoRonda(a))));
                lote.forEach((a, k) => { imagenes[a] = res[k]; });
            }

            const doc = new jsPDF('p', 'mm', 'letter');
            const W = 216, M = 10, PIE = 262;
            const GRIS: [number, number, number] = [225, 225, 225];
            let y = M;

            const nuevaPagina = () => { doc.addPage(); y = M; };
            const hayEspacio = (alto: number) => { if (y + alto > PIE) nuevaPagina(); };
            const titulo = (t: string) => {
                hayEspacio(16);
                doc.setDrawColor(0); doc.setLineWidth(0.4); doc.setFillColor(GRIS[0], GRIS[1], GRIS[2]);
                doc.rect(M, y, W - M * 2, 6, 'FD');
                doc.setFont('helvetica', 'bold'); doc.setFontSize(8); doc.setTextColor(0, 0, 0);
                doc.text(t, M + 2, y + 4.2);
                y += 8;
            };
            const tabla = (opciones: any) => {
                autoTable(doc, {
                    theme: 'grid',
                    margin: { left: M, right: M, bottom: 18 },
                    styles: { fontSize: 7, lineColor: [0, 0, 0], lineWidth: 0.3, textColor: [0, 0, 0], cellPadding: 1.5 },
                    headStyles: { fillColor: GRIS, textColor: [0, 0, 0], fontStyle: 'bold' },
                    ...opciones,
                    startY: y
                });
                y = (doc as any).lastAutoTable.finalY + 4;
            };
            const nota = (texto: string) => {
                doc.setFont('helvetica', 'normal'); doc.setFontSize(7); doc.setTextColor(0, 0, 0);
                const lineas: string[] = doc.splitTextToSize(texto, W - M * 2);
                hayEspacio(lineas.length * 3.2 + 2);
                lineas.forEach((l, i) => doc.text(l, M, y + 3 + i * 3.2));
                y += lineas.length * 3.2 + 3;
            };
            const filasParametros = (lista: { etiqueta: string; valor: string }[]): string[][] => {
                const filas: string[][] = [];
                for (let i = 0; i < lista.length; i += 3) {
                    const fila: string[] = [];
                    for (let k = 0; k < 3; k++) {
                        const it = lista[i + k];
                        fila.push(it ? it.etiqueta : '', it ? it.valor : '');
                    }
                    filas.push(fila);
                }
                return filas;
            };
            const estiloParametros = {
                0: { fontStyle: 'bold', fillColor: [245, 245, 245] },
                2: { fontStyle: 'bold', fillColor: [245, 245, 245] },
                4: { fontStyle: 'bold', fillColor: [245, 245, 245] }
            };

            // Encabezado
            doc.setDrawColor(0); doc.setLineWidth(0.4);
            doc.rect(M, y, W - M * 2, 20);
            doc.setFontSize(11); doc.setTextColor(0, 0, 0); doc.setFont('helvetica', 'bold');
            doc.text('EMPAQUES PLASTICOS S.A.S', W / 2, y + 7, { align: 'center' });
            doc.setFontSize(7); doc.setFont('helvetica', 'normal');
            doc.text('NIT:816000992-1', W / 2, y + 12, { align: 'center' });
            doc.setFontSize(9); doc.setFont('helvetica', 'bold');
            doc.text('FORMATO: CONTROL Y LIBERACION DE PRODUCTO - ' + (this.tipoFormato?.nombre ?? '').toUpperCase(), W / 2, y + 17, { align: 'center' });
            doc.rect(W - M - 42, y, 42, 20);
            doc.setFontSize(7); doc.setFont('helvetica', 'normal');
            doc.text('Codigo: ' + this.codigoFormato, W - M - 40, y + 6);
            doc.text('Version: 002', W - M - 40, y + 13);
            doc.text('Fecha: 15/09/2024', W - M - 40, y + 18);
            y += 24;

            // Datos generales
            titulo('DATOS GENERALES');
            tabla({
                body: [
                    ['Orden de producción', r.ordenProduccion || '-', 'Fecha', r.fecha ? new Date(r.fecha).toLocaleString('es-CO') : '-'],
                    ['Cliente', r.cliente || '-', 'Referencia', r.referencia || '-'],
                    ['Operario(s)', r.operarios || '-', 'Máquina', r.maquina || '-'],
                    ['Hora', r.hora || '-', 'Estado', r.estado === 'Liberado' ? 'Liberado' : 'Pendiente de liberación']
                ],
                columnStyles: {
                    0: { fontStyle: 'bold', fillColor: [245, 245, 245], cellWidth: 34 },
                    2: { fontStyle: 'bold', fillColor: [245, 245, 245], cellWidth: 26 }
                }
            });

            // Mezcla de la orden
            if (mezcla.length > 0) {
                titulo('MEZCLA DE LA ORDEN');
                tabla({
                    head: [['Proceso', 'Referencia', 'Descripción', 'Cantidad', '%']],
                    body: mezcla.map((m: any) => [
                        this.nombreProceso(m.proceso), m.referencia, m.descripcion,
                        (Number(m.cantidad) || 0).toLocaleString('es-CO', { maximumFractionDigits: 2 }) + ' ' + (m.unidad || ''),
                        (Number(m.porcentaje) || 0).toLocaleString('es-CO', { maximumFractionDigits: 2 }) + '%'
                    ])
                });
            }

            // Parámetros: el último guardado y el de cada operario
            const lista = this.parametrosLista(vc);
            if (lista.length > 0) {
                titulo('PARÁMETROS REGISTRADOS (ÚLTIMO GUARDADO)');
                tabla({ body: filasParametros(lista), columnStyles: estiloParametros });
                if (vc?.motivoCambio) nota('Motivo del cambio de parámetros: ' + vc.motivoCambio);
            }

            for (const op of operarios as any[]) {
                const v = this.parsearJson(op.variablesCriticasJson);
                const l = this.parametrosLista(v);
                if (l.length === 0) continue;
                titulo('PARÁMETROS DE ' + String(op.operarioNombre || '-').toUpperCase() + ' — guardado ' +
                    (op.fechaGuardado ? new Date(op.fechaGuardado).toLocaleString('es-CO') : '-'));
                tabla({ body: filasParametros(l), columnStyles: estiloParametros });
                const motivo = op.motivoCambio || v?.motivoCambio;
                if (motivo) nota('Motivo del cambio: ' + motivo);
            }

            // Verificación por hora
            if (rondas.length > 0 && caracteristicas.length > 0) {
                titulo('RESULTADOS DE VERIFICACIÓN POR HORA');
                const encabezados = [
                    'Característica',
                    ...rondas.map((ro: any) => {
                        let t = ro.hora + (ro.final ? ' (F)' : '');
                        if (ro.operario) t += '\n' + ro.operario;
                        if (ro.cierreDeOperario) t += '\nDesp. ' + (ro.kilosDesperdicio || 0) + ' kg';
                        return t;
                    }),
                    'Observación'
                ];
                const filas = caracteristicas.map((c: any) => {
                    const fila: string[] = [c.descripcion];
                    for (const ro of rondas) {
                        const v = ro.valores?.[c.id];
                        let t = v === 'cumple' ? 'OK' : v === 'noCumple' ? 'NO' : v === 'na' ? 'N/A' : '-';
                        const medida = ro.valoresNumericos?.[c.id];
                        if (medida) t += ' (' + medida + ')';
                        fila.push(t);
                    }
                    fila.push(observaciones[c.id] || '-');
                    return fila;
                });
                tabla({
                    head: [encabezados],
                    body: filas,
                    styles: { fontSize: 6.5, lineColor: [0, 0, 0], lineWidth: 0.3, textColor: [0, 0, 0], cellPadding: 1.2, halign: 'center' },
                    columnStyles: { 0: { halign: 'left', cellWidth: 36 }, [rondas.length + 1]: { halign: 'left', cellWidth: 28 } },
                    didParseCell: (d: any) => {
                        if (d.section === 'body' && d.column.index > 0 && d.column.index <= rondas.length) {
                            const t = String(d.cell.raw);
                            if (t.startsWith('NO')) { d.cell.styles.textColor = [180, 28, 28]; d.cell.styles.fontStyle = 'bold'; }
                            else if (t.startsWith('OK')) d.cell.styles.textColor = [22, 101, 52];
                        }
                    }
                });
            }

            // Fotos de evidencia de cada hora
            const conFotos = rondas
                .map((ro: any, i: number) => ({ ro, fotos: fotosPorRonda[i] }))
                .filter(x => x.fotos.length > 0);

            if (conFotos.length > 0) {
                titulo('EVIDENCIA FOTOGRÁFICA');
                for (const { ro, fotos } of conFotos) {
                    hayEspacio(46);
                    doc.setFont('helvetica', 'bold'); doc.setFontSize(7); doc.setTextColor(0, 0, 0);
                    doc.text('Hora ' + ro.hora + (ro.operario ? ' — ' + ro.operario : '') + ' (' + fotos.length + ' foto' + (fotos.length > 1 ? 's' : '') + ')', M, y + 3);
                    y += 6;

                    let col = 0;
                    for (const archivo of fotos) {
                        if (col === 4) { col = 0; y += 38; }
                        if (y + 38 > PIE) { nuevaPagina(); col = 0; }
                        const x = M + col * 50;
                        const img = imagenes[archivo];
                        doc.setDrawColor(200, 200, 200); doc.setLineWidth(0.2);
                        doc.rect(x, y, 46, 34);
                        if (img) {
                            const esc = Math.min(46 / img.w, 34 / img.h);
                            const w = img.w * esc, h = img.h * esc;
                            try { doc.addImage(img.data, 'JPEG', x + (46 - w) / 2, y + (34 - h) / 2, w, h); }
                            catch (e) { doc.setFont('helvetica', 'normal'); doc.setFontSize(6); doc.text('No disponible', x + 23, y + 18, { align: 'center' }); }
                        } else {
                            doc.setFont('helvetica', 'normal'); doc.setFontSize(6); doc.setTextColor(0, 0, 0);
                            doc.text('No disponible', x + 23, y + 18, { align: 'center' });
                        }
                        col++;
                    }
                    y += 38;
                }
            }

            // Liberación
            titulo('LIBERACIÓN DEL PRODUCTO');
            tabla({
                body: [
                    ['¿Puede ser liberado?', r.puedeLiberarse === true ? 'SI' : r.puedeLiberarse === false ? 'NO' : '-',
                        'Producción (Kg/h)', r.produccionKgHora || '-'],
                    ['Desperdicio total (Kg)', r.desperdicioTotalKg != null ? String(r.desperdicioTotalKg) : '-',
                        'Cargo', r.cargoFirma || '-'],
                    ['Revisado por', r.revisadoPor || '-',
                        'Fecha de revisión', r.fechaRevision ? new Date(r.fechaRevision).toLocaleString('es-CO') : '-']
                ],
                columnStyles: {
                    0: { fontStyle: 'bold', fillColor: [245, 245, 245], cellWidth: 38 },
                    2: { fontStyle: 'bold', fillColor: [245, 245, 245], cellWidth: 34 }
                }
            });
            if (r.explicacionNoLiberado) nota('Motivo de no liberación: ' + r.explicacionNoLiberado);

            if (r.firmaDigital) {
                hayEspacio(26);
                doc.setFont('helvetica', 'bold'); doc.setFontSize(7); doc.setTextColor(0, 0, 0);
                doc.text('Firma de quien hace la liberación', M, y + 3);
                try { doc.addImage(r.firmaDigital, 'PNG', M, y + 5, 48, 14); } catch (e) { }
                y += 24;
            }

            nota('Nota: Los métodos de verificación de las características y los criterios de cumplimiento a tener en cuenta, están definidos en el "Procedimiento de Pruebas y Ensayos: ' + (this.tipoFormato?.nombre ?? '') + '" e "Instructivos para Pruebas y Ensayos".');

            hayEspacio(12);
            doc.setFont('helvetica', 'bold'); doc.setFontSize(10); doc.setTextColor(0, 0, 0);
            doc.text('FIN DEL DOCUMENTO', W / 2, y + 6, { align: 'center' });

            const paginas = doc.getNumberOfPages();
            for (let p = 1; p <= paginas; p++) {
                doc.setPage(p);
                doc.setFont('helvetica', 'normal'); doc.setFontSize(6); doc.setTextColor(0, 0, 0);
                doc.text('Página ' + p + ' de ' + paginas, W - M, 272, { align: 'right' });
            }
            doc.save(this.codigoFormato + '_OP' + r.ordenProduccion + '_' + new Date().toISOString().slice(0, 10) + '.pdf');
        } catch (e) {
            console.error(e);
            alert('No se pudo generar el PDF');
        } finally {
            this.generandoPdf = false;
        }
    }
    async exportarPDF(registroUnico?: any) {
        if (registroUnico) { await this.pdfRegistroCompleto(registroUnico); return; }
        const listaAExportar = registroUnico ? [registroUnico] : this.registros;
        if (listaAExportar.length === 0) { alert('No hay registros para exportar'); return; }

        const doc = new jsPDF('p', 'mm', 'letter');
        const NEGRO: [number, number, number] = [0, 0, 0];
        const GRIS: [number, number, number] = [225, 225, 225];
        const W = 216; const M = 10;
        let y = M;

        doc.setDrawColor(0); doc.setLineWidth(0.4);
        doc.rect(M, y, W - M * 2, 20);
        doc.setFontSize(11); doc.setTextColor(...NEGRO); doc.setFont('helvetica', 'bold');
        doc.text('EMPAQUES PLASTICOS S.A.S', W / 2, y + 7, { align: 'center' });
        doc.setFontSize(7); doc.setFont('helvetica', 'normal');
        doc.text('NIT:816000992-1', W / 2, y + 12, { align: 'center' });
        doc.setFontSize(9); doc.setFont('helvetica', 'bold');
        doc.text(`FORMATO: CONTROL Y LIBERACION DE PRODUCTO - ${this.tipoFormato?.nombre?.toUpperCase()}`, W / 2, y + 17, { align: 'center' });
        doc.rect(W - M - 42, y, 42, 20);
        doc.setFontSize(7); doc.setFont('helvetica', 'normal');
        doc.text('Codigo: ' + this.codigoFormato, W - M - 40, y + 6);
        doc.text('Version: 002', W - M - 40, y + 13);
        doc.text('Fecha: 15/09/2024', W - M - 40, y + 18);
        y += 24;

        doc.setFontSize(7); doc.setFont('helvetica', 'italic');
        doc.rect(M, y, W - M * 2, 12);
        doc.text('Nota: Las verificaciones consisten en comparar los datos obtenidos con los instrumentos de medición e inspecciones con los criterios de calidad y las especificaciones dadas en la Orden de producción adjunta a este formato, con el fin de comprobar que se cumplan.', M + 2, y + 3.5, { maxWidth: W - M * 2 - 4 });
        doc.text('Nota: Las verificaciones se registran cada 10% del avance en la ejecución de la orden de Producción y una al final.', M + 2, y + 9.5, { maxWidth: W - M * 2 - 4 });
        y += 15;

        for (const r of listaAExportar) {
            if (y > 240) { doc.addPage(); y = M; }

            doc.setFontSize(9); doc.setFont('helvetica', 'bold');
            doc.text(`Orden de Producción: ${r.ordenProduccion}`, M, y + 5);
            doc.setFont('helvetica', 'normal'); doc.setFontSize(8);
            doc.text(`Fecha: ${new Date(r.fecha).toLocaleDateString('es-CO')}   Cliente: ${r.cliente ?? '-'}   Operarios: ${r.operarios ?? '-'}   Máquina: ${r.maquina ?? '-'}   Hora: ${r.hora ?? '-'}`, M, y + 10);
            y += 14;

            if (r.variablesCriticasJson) {
                const vc = JSON.parse(r.variablesCriticasJson);
                doc.setFontSize(8); doc.setFont('helvetica', 'bold');
                doc.text('REGISTRO VARIABLES CRITICAS', M, y);
                y += 5;

                autoTable(doc, {
                    startY: y,
                    body: [
                        ['Corona', vc.corona || '-', 'Molde', vc.molde || '-'],
                    ],
                    theme: 'grid',
                    bodyStyles: { fontSize: 7, lineColor: NEGRO, lineWidth: 0.3 },
                    margin: { left: M, right: M }
                });
                y = (doc as any).lastAutoTable.finalY + 3;

                autoTable(doc, {
                    startY: y,
                    head: [['Temperaturas Máquina (°C)', '', 'Velocidades Máquina (Hz)', '']],
                    body: [
                        ['Zona 1', vc.temperaturas?.zona1 || '-', 'Máquina', vc.velocidades?.maquina || '-'],
                        ['Zona 2', vc.temperaturas?.zona2 || '-', 'Halador', vc.velocidades?.halador || '-'],
                        ['Zona 3', vc.temperaturas?.zona3 || '-', 'Bobinador', vc.velocidades?.bobinador || '-'],
                        ['Zona 4', vc.temperaturas?.zona4 || '-', '', ''],
                        ['Zona 5', vc.temperaturas?.zona5 || '-', '', ''],
                        ['Zona 6', vc.temperaturas?.zona6 || '-', '', ''],
                    ],
                    headStyles: { fillColor: GRIS, textColor: NEGRO, fontSize: 7, lineColor: NEGRO, lineWidth: 0.3 },
                    bodyStyles: { fontSize: 7, lineColor: NEGRO, lineWidth: 0.3 },
                    margin: { left: M, right: M }
                });
                y = (doc as any).lastAutoTable.finalY + 3;

                autoTable(doc, {
                    startY: y,
                    body: [
                        ['Aire (H)', vc.aire || '-', 'Amperaje (A)', vc.amperaje || '-', 'Altura Burbuja (Cm)', vc.alturaBurbuja || '-', 'Vela', vc.vela || '-'],
                    ],
                    theme: 'grid',
                    bodyStyles: { fontSize: 6.5, lineColor: NEGRO, lineWidth: 0.3 },
                    margin: { left: M, right: M }
                });
                y = (doc as any).lastAutoTable.finalY + 5;

                if (y > 240) { doc.addPage(); y = M; }
            }

            const datos = JSON.parse(r.resultadosJson || '{"rondas":[],"observaciones":{},"caracteristicas":[]}');
            const rondas = datos.rondas || [];
            const caracteristicasReg = datos.caracteristicas || [];
            const observacionesReg = datos.observaciones || {};

            const encabezados = ['Característica', ...rondas.map((rr: any) => rr.hora + (rr.final ? ' (F)' : '')), 'Observación'];
            const filas = caracteristicasReg.map((c: any) => {
                const fila = [c.descripcion];
                for (const rr of rondas) {
                    const v = rr.valores[c.id];
                    fila.push(v === 'cumple' ? 'OK' : v === 'noCumple' ? 'NO' : v === 'na' ? 'N/A' : '-');
                }
                fila.push(observacionesReg[c.id] || '-');
                return fila;
            });

            autoTable(doc, {
                startY: y,
                head: [encabezados],
                body: filas,
                headStyles: { fillColor: GRIS, textColor: NEGRO, fontSize: 6.5, lineColor: NEGRO, lineWidth: 0.3 },
                bodyStyles: { fontSize: 6.5, lineColor: NEGRO, lineWidth: 0.3, halign: 'center' },
                columnStyles: { 0: { halign: 'left' } },
                margin: { left: M, right: M }
            });
            y = (doc as any).lastAutoTable.finalY + 4;
            if (y > 250) { doc.addPage(); y = M; }
            doc.setFontSize(8); doc.setFont('helvetica', 'bold');
            const liberado = r.puedeLiberarse === true ? 'SI' : (r.puedeLiberarse === false ? 'NO' : '-');
            doc.text(`¿Puede ser liberado? ${liberado}`, M, y + 4);
            if (r.explicacionNoLiberado) {
                doc.setFont('helvetica', 'normal');
                doc.text(`Motivo: ${r.explicacionNoLiberado}`, M, y + 9, { maxWidth: W - M * 2 });
                y += 5;
            }
            doc.text(`Revisado por: ${r.revisadoPor || '-'}   Cargo: ${r.cargoFirma || '-'}`, M, y + 9);
            y += 16;

            doc.setDrawColor(200); doc.setLineWidth(0.2);
            doc.line(M, y, W - M, y);
            y += 6;
        }

        if (y > 250) { doc.addPage(); y = M; }
        doc.setFontSize(7); doc.setFont('helvetica', 'italic'); doc.setTextColor(...NEGRO);
        doc.text(`Nota: Los métodos de verificación de las características y los criterios de cumplimiento a tener en cuenta, están definidos en el "Procedimiento de Pruebas y Ensayos: ${this.tipoFormato?.nombre}" e "Instructivos para Pruebas y Ensayos"`, M, y + 3, { maxWidth: W - M * 2 });
        y += 10;

        if (y > 260) { doc.addPage(); y = M; }
        doc.setFont('helvetica', 'bold'); doc.setFontSize(10);
        doc.text('FIN DEL DOCUMENTO', W / 2, y + 4, { align: 'center' });
        doc.save(`${this.codigoFormato}_${new Date().toISOString().slice(0, 10)}.pdf`);
    }
}

