import { Routes } from '@angular/router';

export const CONFIGURACION_ROUTES: Routes = [

    {
        path: '',
        loadComponent: () =>
            import('./configuracion-empresa/configuracion-empresa')
                .then(c => c.ConfiguracionEmpresaComponent),
        data: { animation: 'configuracion' }
    },

     {
        path: 'configuracion',
        loadComponent: () =>
            import('./configuracion-empresa/configuracion-empresa')
                .then(c => c.ConfiguracionEmpresaComponent),
        data: { animation: 'configuracion' }
    },

    {
        path: 'usuarios',
        loadComponent: () =>
            import('./usuarios/usuarios')
                .then(c => c.UsuariosComponent),
        data: { animation: 'configuracion' }
    },

     {
        path: 'contactos',
        loadComponent: () =>
            import('./contactos-notificacion/contactos-notificacion')
                .then(c => c.ContactosNotificacionComponent),
        data: { animation: 'contactos' }
    },

];