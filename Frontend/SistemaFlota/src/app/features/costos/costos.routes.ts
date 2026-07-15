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
];