import { Routes } from '@angular/router';
export const CONTROL_ENVIOS_ROUTES: Routes = [

    {
        path: '',
        loadComponent: () =>
            import('./pedidos/pedidos')
                .then(c => c.PedidosComponent),
        data: {
            animation: 'pedidos',
        }
    },

    {
        path: 'pedidos',
        loadComponent: () =>
            import('./pedidos/pedidos')
                .then(c => c.PedidosComponent),
        data: { animation: 'pedidos' }
    },

    {
        path: 'costos-flete',
        loadComponent: () =>
            import('./costos-fletes/costos-fletes')
                .then(c => c.CostosFleteComponent),
        data: { animation: 'costos-flete' }
    },

    {
        path: 'trazabilidad',
        loadComponent: () =>
            import('./trazabilidad/trazabilidad')
                .then(c => c.TrazabilidadComponent),
        data: { animation: 'trazabilidad' }
    },

];