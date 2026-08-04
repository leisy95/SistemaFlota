import { Routes } from '@angular/router';

export const CONFIGURACION_ROUTES: Routes = [

    {
        path: 'vinculaciones-flotachat',
        loadComponent: () =>
            import('./vinculaciones-flotachat/vinculaciones-flotachat')
                .then(c => c.VinculacionesFlotaChatComponent),
        data: { animation: 'vinculaciones-flotachat' }
    },

    {
        path: '',
        loadComponent: () =>
            import('./configuracion-empresa/configuracion-empresa')
                .then(c => c.ConfiguracionEmpresaComponent),
        data: {
            animation: 'configuracion',
        }
    },

    {
        path: 'configuracion',
        loadComponent: () =>
            import('./configuracion-empresa/configuracion-empresa')
                .then(c => c.ConfiguracionEmpresaComponent),
        data: {
            animation: 'configuracion'
        }
    },

    {
        path: 'usuarios',
        loadComponent: () =>
            import('./usuarios/usuarios')
                .then(c => c.UsuariosComponent),
        data: { animation: 'configuracion' }
    },

    {
        path: 'contactos-notificacion',
        loadComponent: () =>
            import('./contactos-notificacion/contactos-notificacion')
                .then(c => c.ContactosNotificacionComponent),
        data: { animation: 'contactos-notificacion' }
    },

    {
        path: 'centro-informacion',
        loadComponent: () =>
            import('../flota/centro-informacion/centro-informacion')
                .then(c => c.CentroInformacionComponent),
        data: { animation: 'contactos-notificacion' }
    },

];