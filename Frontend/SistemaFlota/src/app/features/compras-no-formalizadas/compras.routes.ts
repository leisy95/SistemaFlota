import { Routes } from '@angular/router';

export const COMPRAS_ROUTES: Routes = [
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
    }
];