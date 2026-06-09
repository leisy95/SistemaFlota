// =====================================================
// models/seguimiento-rrhh.model.ts
// =====================================================

export interface SeguimientoRrhhFoto {
  id: number;
  nombreArchivo: string;
  tipoFoto: 'evidencia' | 'seguimiento';
  fechaSubida: string;
}

export interface SeguimientoRrhh {
  id: number;
  area: string;
  mes: number;
  anio: number;
  fuente: string;
  areas?: string;
  descripcion: string;
  planAccionSugerido?: string;
  factorRiesgo?: string;
  prioridad: 'Alta' | 'Media' | 'Baja';
  responsable?: string;
  fechaEjecucion?: string;
  fechaSeguimiento?: string;
  estado: 'Ejecutado' | 'En proceso' | 'Pendiente';
  observaciones?: string;
  creadoPor: number;
  nombreCreadoPor?: string;
  fechaCreacion: string;
  modificadoPor?: number;
  fechaModificacion?: string;
  fotos: SeguimientoRrhhFoto[];
}

export interface CrearSeguimientoRrhhDto {
  area: string;
  mes: number;
  anio: number;
  fuente: string;
  areas?: string;
  descripcion: string;
  planAccionSugerido?: string;
  factorRiesgo?: string;
  prioridad: string;
  responsable?: string;
  fechaEjecucion?: string;
  fechaSeguimiento?: string;
  estado: string;
  observaciones?: string;
}

export const MESES_RRHH = [
  { valor: 1,  nombre: 'Enero' },   { valor: 2,  nombre: 'Febrero' },
  { valor: 3,  nombre: 'Marzo' },   { valor: 4,  nombre: 'Abril' },
  { valor: 5,  nombre: 'Mayo' },    { valor: 6,  nombre: 'Junio' },
  { valor: 7,  nombre: 'Julio' },   { valor: 8,  nombre: 'Agosto' },
  { valor: 9,  nombre: 'Septiembre' }, { valor: 10, nombre: 'Octubre' },
  { valor: 11, nombre: 'Noviembre' }, { valor: 12, nombre: 'Diciembre' },
];

export const PRIORIDADES_RRHH  = ['Alta', 'Media', 'Baja'];
export const ESTADOS_RRHH      = ['Ejecutado', 'En proceso', 'Pendiente'];