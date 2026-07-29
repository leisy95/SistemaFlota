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
    {
        path: 'extrusion',
        loadComponent: () =>
            import('./formato-calidad-generico/formato-calidad-generico')
                .then(c => c.FormatoCalidadGenericoComponent),
        data: { animation: 'extrusion', codigo: 'F-GC-004' }
    },
    {
        path: 'impresion',
        loadComponent: () =>
            import('./formato-calidad-generico/formato-calidad-generico')
                .then(c => c.FormatoCalidadGenericoComponent),
        data: { animation: 'impresion', codigo: 'F-GC-005' }
    },
    {
        path: 'sellado',
        loadComponent: () =>
            import('./formato-calidad-generico/formato-calidad-generico')
                .then(c => c.FormatoCalidadGenericoComponent),
        data: { animation: 'sellado', codigo: 'F-GC-006' }
    },
    {
        path: 'precorte',
        loadComponent: () =>
            import('./formato-calidad-generico/formato-calidad-generico')
                .then(c => c.FormatoCalidadGenericoComponent),
        data: { animation: 'precorte', codigo: 'F-GC-007' }
    },

    {
        path: 'admin-opciones-formulario',
        loadComponent: () =>
            import('./opciones-formulario/opciones-formulario')
                .then(c => c.OpcionesFormularioComponent),
        data: { animation: 'admin-opciones-formulario' }
    },
];