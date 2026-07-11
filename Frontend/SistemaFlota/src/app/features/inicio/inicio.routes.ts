import { Routes } from '@angular/router';

export const INICIO_ROUTES: Routes = [

     {
        path: '',
        loadComponent: () =>
            import('./inicio')
            .then(c => c.Inicio),
        data: { animation: 'inicio' }
    },

];