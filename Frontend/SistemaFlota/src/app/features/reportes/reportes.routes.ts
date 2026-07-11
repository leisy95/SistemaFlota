import { Routes } from '@angular/router';

export const REPORTES_ROUTES: Routes = [

    {
        path: '',
        loadComponent: () =>
            import('./auditoria/auditoria')
                .then(c => c.AuditoriaComponent),
        data: { animation: 'auditoria' }
    },

];