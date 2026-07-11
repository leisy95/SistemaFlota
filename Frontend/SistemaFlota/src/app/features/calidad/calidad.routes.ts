import { Routes } from '@angular/router';

export const CALIDAD_ROUTES: Routes = [

    {
        path: '',
        redirectTo: 'cyreles',
        pathMatch: 'full'
    },

    {
        path: 'cyreles',
        loadComponent: () =>
            import('./Cyreles/cyreles.component')
            .then(c => c.CyrelesComponent),
        data: { animation: 'cyreles' }
    },

    {
        path: 'calidad-formatos',
        loadComponent: () =>
            import('./formato-fgc008/formato-fgc008')
            .then(c => c.FormatoFGC008Component),
        data: { animation: 'calidad-formatos' }
    },
];