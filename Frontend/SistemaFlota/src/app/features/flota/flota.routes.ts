import { Routes } from '@angular/router';

export const FLOTA_ROUTES: Routes = [

    {
        path: '',
        loadComponent: () =>
            import('./dashboard/dashboard')
                .then(c => c.DashboardComponent),
        data: { animation: 'dashboard' }
    },
    {
        path: 'dashboard',
        loadComponent: () =>
            import('./dashboard/dashboard')
                .then(c => c.DashboardComponent),
        data: { animation: 'dashboard' }
    },
    {
        path: 'conductores',
        loadComponent: () =>
            import('./conductores/conductores.component')
                .then(c => c.ConductoresComponent),
        data: { animation: 'conductores' }
    },
    {
        path: 'vehiculos',
        loadComponent: () =>
            import('./vehiculos/vehiculos')
                .then(c => c.VehiculosComponent),
        data: { animation: 'vehiculos' }
    },

    {
        path: 'inspecciones',
        loadComponent: () =>
            import('./inspecciones/inspecciones')
                .then(c => c.InspeccionesComponent),
        data: { animation: 'inspecciones' }
    },
    {
        path: 'historial',
        loadComponent: () =>
            import('./inspecciones-historial/inspecciones-historial')
                .then(c => c.InspeccionesHistorialComponent),
        data: { animation: 'historial' }
    },
    {
        path: 'autorizaciones',
        loadComponent: () =>
            import('./autorizaciones/autorizaciones')
                .then(c => c.AutorizacionesComponent),
        data: { animation: 'autorizaciones' }
    },
    {
        path: 'mantenimiento',
        loadComponent: () =>
            import('./mantenimiento/mantenimiento')
                .then(c => c.MantenimientoComponent),
        data: { animation: 'mantenimiento' }
    },
    {
        path: 'cambio-ruta',
        loadComponent: () =>
            import('./cambio-ruta/cambio-ruta')
                .then(c => c.CambioRutaComponent),
        data: { animation: 'cambio-ruta' }
    },
    {
        path: 'reportes',
        loadComponent: () =>
            import('./reportes-ruta/reportes-ruta')
                .then(c => c.ReporteRutaComponent),
        data: { animation: 'reportes' }
    },
    {
        path: 'documentos',
        loadComponent: () =>
            import('./documentos/documentos')
                .then(c => c.DocumentosComponent),
        data: { animation: 'documentos' }
    },
    {
        path: 'checklist',
        loadComponent: () =>
            import('./checklist-editor/checklist-editor')
                .then(c => c.ChecklistEditorComponent),
        data: { animation: 'checklist' }
    }

];