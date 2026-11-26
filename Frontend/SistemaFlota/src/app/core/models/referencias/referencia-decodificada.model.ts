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
export interface DescripcionInterpretada {
  descripcion: string;
  valida: boolean;
  errores: string[];
  advertencias: string[];
  material: 'Original' | 'PostIndustrial' | 'Posconsumo' | 'Compostable' | 'Mezcla' | null;
  biodegradable: boolean;
  impreso: boolean;
  fuelleLateral: number | null;
  fuelleFondo: number | null;
  fuelleSuperior: number | null;
  solapa: number | null;
  troquel: 'Ovalo' | 'Gancho' | 'Asa' | null;
  cintaSeguridad: boolean;
  cintaPegaDespega: boolean;
  codigosSinDefinir: string[];
}