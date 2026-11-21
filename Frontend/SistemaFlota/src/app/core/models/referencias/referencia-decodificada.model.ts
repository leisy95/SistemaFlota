export interface ReferenciaDecodificada {
  codigo: string;
  valida: boolean;
  errores: string[];
  advertencias: string[];
  tipoCodigo: number | null;
  tipo: string | null;
  origen: 'Original' | 'Recuperado' | null;
  densidad: 'Baja' | 'Alta' | null;
  colorCodigo: string | null;
  colorNombre: string | null;
  impreso: boolean;
  ancho: number | null;
  largo: number | null;
  calibre: number | null;
  sufijo: string | null;
  sufijoNombre: string | null;
  sufijoReconocido: boolean;
  esPosconsumo: boolean;
  esBiodegradable: boolean;
  esCompostable: boolean;
}

export interface ItemCatalogoReferencia {
  codigo: string;
  nombre: string;
}

export interface CatalogosReferencia {
  tipos: ItemCatalogoReferencia[];
  colores: ItemCatalogoReferencia[];
  sufijos: ItemCatalogoReferencia[];
}