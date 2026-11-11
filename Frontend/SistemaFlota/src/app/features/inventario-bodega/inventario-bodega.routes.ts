import { Routes } from '@angular/router';

export const INVENTARIOBODEGA_ROUTES: Routes = [

    {
        path: '',
        redirectTo: 'mostrar-entradas',
        pathMatch: 'full'
    },

    {
        path: 'listar-inventario',
        loadComponent: () =>
            import('./inventario/listar-inventario/listar-inventario')
                .then(c => c.ListarInventario),
    },

    {
        path: 'mostrar-entradas',
        loadComponent: () =>
            import('./entradas-bodega/mostrar-entradas/mostrar-entradas')
                .then(c => c.MostrarEntradas),
    },

    {
        path: 'registro-salidas',
        loadComponent: () =>
            import('./registro-salida/registro-salida')
                .then(c => c.RegistroSalida),
    },

    {
        path: 'devoluciones',
        loadComponent: () =>
            import('./devoluciones/devoluciones')
                .then(c => c.Devoluciones),
    },

    {
        path: 'registrar-entrada',
        loadComponent: () =>
            import('./entradas-bodega/registrar-entrada/registrar-entrada')
                .then(c => c.RegistrarEntrada),
    }

];