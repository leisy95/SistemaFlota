export interface Vehiculo {
  id:              number;
  placa:           string;
  marca:           string;
  modelo:          string;
  modeloAnio:      number;
  color:           string;
  estado:          string;
  foto?:           string;
  conductorId:     number;
  tipoVehiculoId?: number;
}