
import { Routes } from '@angular/router';

export const PRODUCCION_ROUTES: Routes = [

    {
        path: '',
        redirectTo: 'control-retal',
        pathMatch: 'full'
    },

    {
        path: 'control-retal',
        loadComponent: () =>
            import('./control-retal/seleccionar-proceso/seleccionar-proceso')
                .then(c => c.SeleccionarProceso)
    },

    {
        path: 'control-retal/listar-retal',
        loadComponent: () =>
            import('./control-retal/listar-retal/listar-retal')
                .then(c => c.ListarRetal),
    },

    {
        path: 'control-retal/registrar-movimientos',
        loadComponent: () =>
            import('./control-retal/registrar-movimientos/registrar-movimientos')
                .then(c => c.RegistrarMovimientos)
    },

    {
        path: 'crispetas',
        loadComponent: () =>
            import('./control-crispetas/listar-crispetas/listar-crispetas')
                .then(c => c.ListarCrispetas),
    },

];
