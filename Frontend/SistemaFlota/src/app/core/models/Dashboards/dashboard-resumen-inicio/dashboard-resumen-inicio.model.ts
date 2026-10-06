export interface DashboardActividad {
    tipo: string;
    descripcion: string;
    fecha: string;
}

export interface DashboardVencimiento {
    tipo: string;
    descripcion: string;
    dias: number;
}

export interface DashboardResumenInicio {
    vehiculos: number;
    empleados: number;
    ordenesCompra: number;
    ordenesPendientes: number;
    mantenimientos: number;
    mantenimientosProximos: number;

    actividades: DashboardActividad[];
    vencimientos: DashboardVencimiento[];
}