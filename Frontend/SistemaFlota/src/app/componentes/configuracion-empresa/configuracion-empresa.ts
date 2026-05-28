import {
  Component,
  OnInit
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ConfiguracionService } from '../../services/configuracion.service';

@Component({
  selector: 'app-configuracion-empresa',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './configuracion-empresa.html',
  styleUrls: ['./configuracion-empresa.scss']
})

export class ConfiguracionEmpresaComponent implements OnInit {

  guardando   = false;
  guardado    = false;
  errorMsg    = '';
  logoPreview: string | null = null;
  logoArchivo: File | null   = null;

  config = {
    nombreEmpresa:    '',
    nit:              '',
    direccion:        '',
    telefono:         '',
    email:            '',
    colorCorporativo: 'var(--color-primario)',
    sitioWeb:         '',
    descripcion:      ''
  };

  readonly coloresSugeridos = [
    { label: 'Verde',    value: 'var(--color-primario)' },
    { label: 'Azul',     value: '#1d4ed8' },
    { label: 'Rojo',     value: '#dc2626' },
    { label: 'Naranja',  value: '#ea580c' },
    { label: 'Morado',   value: '#7c3aed' },
    { label: 'Negro',    value: '#0f172a' },
  ];

  constructor(
    private configuracionService: ConfiguracionService
  ) {}

  ngOnInit(): void {
    this.cargarConfiguracion();
  }

  cargarConfiguracion() {
    this.configuracionService.obtenerConfiguracion().subscribe({
      next: (data: any) => {
        this.config = {
          nombreEmpresa:    data.nombreEmpresa    ?? '',
          nit:              data.nit              ?? '',
          direccion:        data.direccion        ?? '',
          telefono:         data.telefono         ?? '',
          email:            data.email            ?? '',
          colorCorporativo: data.colorCorporativo ?? 'var(--color-primario)',
          sitioWeb:         data.sitioWeb         ?? '',
          descripcion:      data.descripcion      ?? ''
        };
        if (data.logo) {
          this.logoPreview = `https://localhost:7293/config/${data.logo}`;
        }
      },
      error: (err) => console.error(err)
    });
  }

  seleccionarLogo(event: any) {
    const archivo = event.target.files[0];
    if (!archivo) return;

    this.logoArchivo = archivo;

    const reader = new FileReader();
    reader.onload = (e: any) => {
      this.logoPreview = e.target.result;
    };
    reader.readAsDataURL(archivo);
  }

  seleccionarColor(color: string) {
    this.config.colorCorporativo = color;
  }

  guardar() {
    if (!this.config.nombreEmpresa) { alert('Ingrese el nombre de la empresa'); return; }
    if (!this.config.nit)           { alert('Ingrese el NIT'); return; }

    this.guardando = true;
    this.errorMsg  = '';

    const formData = new FormData();
    formData.append('NombreEmpresa',    this.config.nombreEmpresa);
    formData.append('NIT',              this.config.nit);
    formData.append('Direccion',        this.config.direccion);
    formData.append('Telefono',         this.config.telefono);
    formData.append('Email',            this.config.email);
    formData.append('ColorCorporativo', this.config.colorCorporativo);
    formData.append('SitioWeb',         this.config.sitioWeb);
    formData.append('Descripcion',      this.config.descripcion);

    if (this.logoArchivo) {
      formData.append('Logo', this.logoArchivo);
    }

    this.configuracionService.guardarConfiguracion(formData).subscribe({
      next: () => {
        this.guardando = false;
        this.guardado  = true;
        setTimeout(() => this.guardado = false, 3000);
      },
      error: (err) => {
        console.error(err);
        this.guardando = false;
        this.errorMsg  = 'Error guardando configuración';
      }
    });
  }

}