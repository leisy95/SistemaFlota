import { Routes } from '@angular/router';

export const CALIDAD_ROUTES: Routes = [

    {
        path: '',
        redirectTo: 'calidad-cyreles',
        pathMatch: 'full'
    },

    {
        path: 'calidad-cyreles',
        loadComponent: () =>
            import('./Cyreles/cyreles.component')
            .then(c => c.CyrelesComponent),
        data: { animation: 'calidad-cyreles' }
    },

    {
        path: 'calidad-formatos',
        loadComponent: () =>
            import('./formato-fgc008/formato-fgc008')
            .then(c => c.FormatoFGC008Component),
        data: { animation: 'calidad-formatos' }
    },
];