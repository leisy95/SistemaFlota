import { Routes } from '@angular/router';
import { Layout } from './core/layout/layout';

export const routes: Routes = [

  // LOGIN
  {
    path: '',
    loadComponent: () =>
      import('./features/login/login')
        .then(c => c.LoginComponent)
  },

  {
    path: 'inicio',
    loadChildren: () =>
      import('./features/inicio/inicio.routes')
        .then(r => r.INICIO_ROUTES)
  },


  // SISTEMA CON SIDEBAR
  {
    path: '',
    component: Layout,
    children: [

      {
        path: 'flota',
        loadChildren: () =>
          import('./features/flota/flota.routes')
            .then(r => r.FLOTA_ROUTES)
      },


      {
        path: 'rrhh',
        loadChildren: () =>
          import('./features/rrhh/rrhh.routes')
            .then(r => r.RRHH_ROUTES)
      },


      {
        path: 'calidad',
        loadChildren: () =>
          import('./features/calidad/calidad.routes')
            .then(r => r.CALIDAD_ROUTES)
      },

      {
        path: 'control-envios',
        loadChildren: () =>
          import('./features/control-envios/control-envios.routes')
            .then(r => r.CONTROL_ENVIOS_ROUTES)
      },

      {
        path: 'reportes',
        loadChildren: () =>
          import('./features/reportes/reportes.routes')
            .then(r => r.REPORTES_ROUTES)
      },

      {
        path: 'configuracion',
        loadChildren: () =>
          import('./features/configuracion/configuracion.routes')
            .then(r => r.CONFIGURACION_ROUTES)
      }

    ]
  },


  {
    path: '**',
    redirectTo: ''
  }

];