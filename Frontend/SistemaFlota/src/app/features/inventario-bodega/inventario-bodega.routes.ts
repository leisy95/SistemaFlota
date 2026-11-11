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
            import('./salidas-bodega/registro-salida/registro-salida')
                .then(c => c.RegistroSalida),
    },

    {
        path: 'listar-clientes',
        loadComponent: () =>
            import('./clientes/cliente-bodega/cliente-bodega')
                .then(c => c.ClienteBodega),
    },

    {
        path: 'devoluciones',
        loadComponent: () =>
            import('./entradas-bodega/devoluciones/devoluciones')
                .then(c => c.Devoluciones),
    },

    {
        path: 'ajuste-general',
        loadComponent: () =>
            import('./entradas-bodega/ajuste-entradas/ajuste-entradas')
                .then(c => c.AjusteEntradas),
    },

    {
        path: 'registrar-entrada',
        loadComponent: () =>
            import('./entradas-bodega/registrar-entrada/registrar-entrada')
                .then(c => c.RegistrarEntrada),
    }

];