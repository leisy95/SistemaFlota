import { Routes } from '@angular/router';

export const RRHH_ROUTES: Routes = [

    {
        path: '',
        loadComponent: () =>
            import('./seguimientos-rrhh/seguimientos-rrhh.component')
                .then(c => c.SeguimientosRrhhComponent),
        data: { animation: 'seguimientos-rrhh' }
    },

];