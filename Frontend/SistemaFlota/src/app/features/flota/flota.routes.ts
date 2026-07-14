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
        path: 'ver-inspecciones',
        loadComponent: () =>
            import('./inspecciones-historial/inspecciones-historial')
                .then(c => c.InspeccionesHistorialComponent),
        data: { animation: 'ver-inspecciones' }
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
        path: 'solicitud-taller',
        loadComponent: () =>
            import('./solicitud-taller/solicitud-taller')
                .then(c => c.SolicitudTallerComponent),
        data: { animation: 'cambio-ruta' }
    },
    {
        path: 'reporte-ruta',
        loadComponent: () =>
            import('./reportes-ruta/reportes-ruta')
                .then(c => c.ReporteRutaComponent),
        data: { animation: 'reporte-ruta' }
    },
    {
        path: 'incidentes',
        loadComponent: () =>
            import('./incidentes/incidentes')
                .then(c => c.IncidentesComponent),
        data: { animation: 'reporte-ruta' }
    },
    {
        path: 'documentos',
        loadComponent: () =>
            import('./documentos/documentos')
                .then(c => c.DocumentosComponent),
        data: { animation: 'documentos' }
    },
    {
        path: 'encuesta-fatiga',
        loadComponent: () =>
            import('./encuesta-fatiga/encuesta-fatiga')
                .then(c => c.EncuestaFatigaComponent),
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