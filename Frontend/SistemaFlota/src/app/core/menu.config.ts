export interface MenuItem {
    key: string;
    label: string;
    icon: string;
    ruta: string;
    modulo?: string;
}


export const MENU_MODULOS: MenuItem[] = [

    // FLOTA 

    {
        key: 'dashboard',
        label: 'Panel de control',
        icon: 'fa-solid fa-gauge-high',
        ruta: '/flota/dashboard',
        modulo: 'flota'
    },

    {
        key: 'conductores',
        label: 'Conductores',
        icon: 'fa-solid fa-id-card',
        ruta: '/flota/conductores',
        modulo: 'flota'
    },

    {
        key: 'vehiculos',
        label: 'Vehículos',
        icon: 'fa-solid fa-truck',
        ruta: '/flota/vehiculos',
        modulo: 'flota'
    },

    {
        key: 'inspecciones',
        label: 'Inspecciones',
        icon: 'fa-solid fa-clipboard-check',
        ruta: '/flota/inspecciones',
        modulo: 'flota'
    },

    {
        key: 'ver-inspecciones',
        label: 'Historial inspecciones',
        icon: 'fa-solid fa-clock-rotate-left',
        ruta: '/flota/ver-inspecciones',
        modulo: 'flota'
    },

    {
        key: 'autorizaciones',
        label: 'Autorizaciones',
        icon: 'fa-solid fa-file-circle-check',
        ruta: '/flota/autorizaciones',
        modulo: 'flota'
    },

    {
        key: 'reporte-ruta',
        label: 'Reporte en Ruta',
        icon: 'fa-solid fa-triangle-exclamation',
        ruta: '/flota/reporte-ruta',
        modulo: 'flota'
    },

    {
        key: 'incidentes',
        label: 'Incidentes',
        icon: 'fa-solid fa-triangle-exclamation',
        ruta: '/flota/incidentes',
        modulo: 'flota'
    },

    {
        key: 'cambio-ruta',
        label: 'Cambio Ruta',
        icon: 'fa-solid fa-id-card',
        ruta: '/flota/cambio-ruta',
        modulo: 'flota'
    },

    {
        key: 'solicitud-taller',
        label: 'Solicitud taller',
        icon: 'fa-solid fa-wrench',
        ruta: '/flota/solicitud-taller',
        modulo: 'flota'
    },

    {
        key: 'mantenimiento',
        label: 'Mantenimiento',
        icon: 'fa-solid fa-wrench',
        ruta: '/flota/mantenimiento',
        modulo: 'flota'
    },

    {
        key: 'documentos',
        label: 'Documentos',
        icon: 'fa-solid fa-folder-open',
        ruta: '/flota/documentos',
        modulo: 'flota'
    },

    {
        key: 'encuesta-fatiga',
        label: 'Encuesta fatiga',
        icon: 'fa-solid fa-folder-open',
        ruta: '/flota/encuesta-fatiga',
        modulo: 'flota'
    },

    {
        key: 'checklist',
        label: 'Checklist',
        icon: 'fa-solid fa-box',
        ruta: '/flota/checklist',
        modulo: 'flota'
    },

    {
        key: 'centro-informacion',
        label: 'Centro informacion',
        icon: 'fa-solid fa-file-lines',
        ruta: '/configuracion/centro-informacion',
        modulo: 'flota'
    },

    //  SST / RRHH 

    {
        key: 'rrhh-seguimientos',
        label: 'Seguimientos SST',
        icon: 'fa-solid fa-clipboard-list',
        ruta: '/rrhh/rrhh-seguimientos',
        modulo: 'rrhh'
    },

//  CALIDAD
    {
        key: 'calidad-cyreles',
        label: 'Cyreles',
        icon: 'fa-solid fa-box',
        ruta: '/calidad/calidad-cyreles',
        modulo: 'calidad'
    },
    {
        key: 'calidad-formatos',
        label: 'Formatos',
        icon: 'fa-solid fa-file-lines',
        ruta: '/calidad/calidad-formatos',
        modulo: 'calidad'
    },
    {
        key: 'calidad-extrusion',
        label: 'Extrusión',
        icon: 'fa-solid fa-industry',
        ruta: '/calidad/extrusion',
        modulo: 'calidad'
    },
    {
        key: 'calidad-impresion',
        label: 'Impresión',
        icon: 'fa-solid fa-print',
        ruta: '/calidad/impresion',
        modulo: 'calidad'
    },
    {
        key: 'calidad-sellado',
        label: 'Sellado',
        icon: 'fa-solid fa-stamp',
        ruta: '/calidad/sellado',
        modulo: 'calidad'
    },
   {
        key: 'calidad-precorte',
        label: 'Precorte',
        icon: 'fa-solid fa-scissors',
        ruta: '/calidad/precorte',
        modulo: 'calidad'
    },
    {
        key: 'admin-opciones-formulario',
        label: 'Opciones de Formularios',
        icon: 'fa-solid fa-sliders',
        ruta: '/calidad/admin-opciones-formulario',
        modulo: 'calidad'
    },
    {
        key: 'mejor-rendimiento',
        label: 'Mejor Rendimiento',
        icon: 'fa-solid fa-magnifying-glass-chart',
        ruta: '/calidad/mejor-rendimiento',
        modulo: 'calidad'
    },

    // Control envios 

    {
        key: 'trazabilidad',
        label: 'Trazabilidad',
        icon: 'fa-solid fa-file-lines',
        ruta: '/control-envios/trazabilidad',
        modulo: 'control-envios'
    },

    {
        key: 'costos-flete',
        label: 'Costos Flete',
        icon: 'fa-solid fa-file-lines',
        ruta: '/control-envios/costos-flete',
        modulo: 'control-envios'
    },

    {
        key: 'pedidos',
        label: 'Pedidos',
        icon: 'fa-solid fa-file-lines',
        ruta: '/control-envios/pedidos',
        modulo: 'control-envios'
    },

    // Reportes

    {
        key: 'auditoria',
        label: 'Auditoria',
        icon: 'fa-solid fa-file-lines',
        ruta: '/reportes/auditoria',
        modulo: 'reportes'
    },

    // Configuracion

    {
        key: 'configuracion',
        label: 'Configuracion',
        icon: 'fa-solid fa-file-lines',
        ruta: '/configuracion/configuracion',
        modulo: 'configuracion'
    },

    {
        key: 'contactos-notificacion',
        label: 'Contactos',
        icon: 'fa-solid fa-file-lines',
        ruta: '/configuracion/contactos-notificacion',
        modulo: 'configuracion'
    },

    {
        key: 'vinculaciones-flotachat',
        label: 'Vincular FlotaChat',
        icon: 'fa-solid fa-link',
        ruta: '/configuracion/vinculaciones-flotachat',
        modulo: 'configuracion'
    },

    {
        key: 'usuarios',
        label: 'Usuarios',
        icon: 'fa-solid fa-file-lines',
        ruta: '/configuracion/usuarios',
        modulo: 'configuracion'
    },
    {
        key: 'admin-opciones-formulario',
        label: 'Opciones de Formularios',
        icon: 'fa-solid fa-sliders',
        ruta: '/calidad/admin-opciones-formulario',
        modulo: 'admin-opciones-formulario'
    },

];