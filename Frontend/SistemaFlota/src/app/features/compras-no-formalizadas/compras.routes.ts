import { Routes } from '@angular/router';

export const COMPRAS_ROUTES: Routes = [
    {
        path: '',
        redirectTo: 'ordenes-compras',
        pathMatch: 'full'
    },
    {
        path: 'proveedores',
        loadComponent: () =>
            import('./proveedores/listar-proveedores/listar-proveedores')
                .then(c => c.ListarProveedores),
        data: { animation: 'proveedores' }
    },
    {
        path: 'materiales',
        loadComponent: () =>
            import('./materiales/listar-materiales/listar-materiales')
                .then(c => c.ListarMateriales),
        data: { animation: 'materiales' }
    },
    {
        path: 'ordenes-compras',
        loadComponent: () =>
            import('./ordenes-compras/listar-orden-compra/listar-orden-compra')
                .then(c => c.ListarOrdenCompra),
        data: { animation: 'materiales' }
    },
    {
        path: 'recepcion-mercancias',
        loadComponent: () =>
            import('./recepcion-mercancias/listar-repmercancia/listar-repmercancia')
                .then(c => c.ListarRepmercancia),
        data: { animation: 'materiales' }
    },
    {
        path: 'inventario',
        loadComponent: () =>
            import('./inventario/listar-inventario-no-formalizado/listar-inventario-no-formalizado')
                .then(c => c.ListarInventarioNoFormalizado),
        data: { animation: 'materiales' }
    },
    {
        path: 'traslados',
        loadComponent: () =>
            import('./traslados/listar-traslado-no-formalizado/listar-traslado-no-formalizado')
                .then(c => c.ListarTrasladoNoFormalizado),
        data: { animation: 'materiales' }
    }
];