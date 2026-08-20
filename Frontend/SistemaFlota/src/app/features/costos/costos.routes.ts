import { Routes } from '@angular/router';

export const COSTOS_ROUTES: Routes = [

    {
        path: '',
        loadComponent: () =>
            import('./proveedores-materiales/proveedores-materiales')
                .then(c => c.ProveedoresMateriales),
        data: { animation: 'proveedores' }
    },

    {
        path: 'proveedores-materiales',
        loadComponent: () =>
            import('./proveedores-materiales/proveedores-materiales')
                .then(c => c.ProveedoresMateriales),
        data: { animation: 'proveedores' }
    },

    {
        path: 'orden-compra',
        loadComponent: () =>
            import('./ordenes-compra/listar-orden-compra/listar-orden-compra')
                .then(c => c.ListarOrdenCompra),
        data: { animation: 'proveedores' }
    },

    {
        path: 'recepcion-mercancia',
        loadComponent: () =>
            import('./recepcion-mercancia/listar-repmercancia/listar-repmercancia')
                .then(c => c.ListarRepmercancia),
        data: { animation: 'proveedores' }
    },

    {
        path: 'inventario',
        loadComponent: () =>
            import('./invenatrio/listar-inventario/listar-inventario')
                .then(c => c.ListarInventario),
        data: { animation: 'proveedores' }
    },

    {
        path: 'traslados',
        loadComponent: () =>
            import('./traslados/listar-traslado/listar-traslado')
                .then(c => c.ListarTraslado),
        data: { animation: 'proveedores' }
    },
];