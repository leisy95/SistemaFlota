import { Routes } from '@angular/router';

export const COMPRAS_ROUTES: Routes = [

    {
        path: '',
        loadComponent: () =>
            import('./proveedores/listar-proveedores/listar-proveedores')
                .then(c => c.ListarProveedores),
        data: { animation: 'proveedores' }
    },

    {
        path: 'crear',
        loadComponent: () =>
            import('./proveedores/crear-proveedor/crear-proveedor')
                .then(c => c.CrearProveedor),
        data: { animation: 'proveedores' }
    },
];