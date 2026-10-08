import { Routes } from '@angular/router';

export const HISTORIALODO_ROUTES: Routes = [

    {
        path: '',
        loadComponent: () =>
            import('./movimiento-producto/movimiento-producto')
                .then(c => c.MovimientoProducto),
        data: {
            animation: 'historial-odo',
        }
    },

    {
        path: 'pedido-compra',
        loadComponent: () =>
            import('./pedido-compra/pedido-compra')
                .then(c => c.PedidoCompra),
        data: {
            animation: 'historial-odo'
        }
    }

];