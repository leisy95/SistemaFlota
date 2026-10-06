namespace SistemaFlota.DTOs.DashboardResumenInicio
{
    public class DashboardResumenInicioResponse
    {
        public int Vehiculos { get; set; }
        public int Empleados { get; set; }
        public int OrdenesCompra { get; set; }
        public int OrdenesPendientes { get; set; }
        public int Mantenimientos { get; set; }
        public int MantenimientosProximos { get; set; }

        public List<DashboardActividadResponse> Actividades { get; set; }
            = new();

        public List<DashboardVencimientoResponse> Vencimientos { get; set; }
            = new();
    }

    public class DashboardActividadResponse
    {
        public string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }

    public class DashboardVencimientoResponse
    {
        public string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Dias { get; set; }
    }
}