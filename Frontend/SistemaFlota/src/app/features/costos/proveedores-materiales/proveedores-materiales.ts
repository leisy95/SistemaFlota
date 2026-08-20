import { Component } from '@angular/core';
import { ListarProveedores } from './proveedores/listar-proveedores/listar-proveedores';
import { ListarMateriales } from './materiales/listar-materiales/listar-materiales';

@Component({
  selector: 'app-proveedores-materiales',
  standalone: true,
  imports: [
    ListarProveedores,
    ListarMateriales
  ],
  templateUrl: './proveedores-materiales.html',
  styleUrl: './proveedores-materiales.scss',
})
export class ProveedoresMateriales {
  vista: 'proveedores' | 'materiales' = 'proveedores';
}
