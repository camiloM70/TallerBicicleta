namespace TallerBicicleta.Services
{
    public class GananciaService
    {
        private readonly FirebaseService _firebaseService;
        private readonly ILogger<GananciaService> _logger;

        public GananciaService(FirebaseService firebaseService, ILogger<GananciaService> logger)
        {
            _firebaseService = firebaseService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene el reporte de ganancias por mes
        /// </summary>
        public async Task<ReporteGanancias> ObtenerGananciasPorMesAsync(int mes, int anio)
        {
            try
            {
                var collection = _firebaseService.GetCollection("facturas");
                var snapshot = await collection.GetSnapshotAsync();

                var facturasMes = snapshot.Documents
                    .Select(doc => doc.ConvertTo<Factura>())
                    .Where(f => f.FechaEmision.Year == anio && f.FechaEmision.Month == mes)
                    .ToList();

                var reporte = new ReporteGanancias
                {
                    Mes = mes,
                    Anio = anio,
                    TotalFacturas = facturasMes.Count,
                    FacturasPagadas = facturasMes.Count(f => f.Pagada)
                };

                foreach (var factura in facturasMes)
                {
                    // Ganancias por servicios
                    reporte.GananciaServicios += factura.PrecioServicio;

                    // Ganancias por productos (solo de facturas pagadas)
                    if (factura.Pagada && factura.Detalles != null)
                    {
                        reporte.GananciaProductos += factura.Detalles.Sum(d => d.Subtotal);
                    }

                    // Comisión del empleado (si existe la propiedad)
                    if (factura.ComisionEmpleado > 0)
                    {
                        reporte.ComisionEmpleado += factura.ComisionEmpleado;
                    }
                }

                // Total ganado = servicios + productos - comisión
                reporte.TotalGanado = reporte.GananciaServicios + reporte.GananciaProductos;
                reporte.GananciaNeta = reporte.TotalGanado - reporte.ComisionEmpleado;

                return reporte;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ganancias del mes {mes}/{anio}", mes, anio);
                throw;
            }
        }

        /// <summary>
        /// Obtiene el reporte de ganancias de todo el año (consolidado)
        /// </summary>
        public async Task<ReporteGanancias> ObtenerGananciasPorAnioAsync(int anio)
        {
            try
            {
                var collection = _firebaseService.GetCollection("facturas");
                var snapshot = await collection.GetSnapshotAsync();

                var facturasAnio = snapshot.Documents
                    .Select(doc => doc.ConvertTo<Factura>())
                    .Where(f => f.FechaEmision.Year == anio)
                    .ToList();

                var reporte = new ReporteGanancias
                {
                    Mes = 0, // 0 indica todo el año
                    Anio = anio,
                    TotalFacturas = facturasAnio.Count,
                    FacturasPagadas = facturasAnio.Count(f => f.Pagada)
                };

                foreach (var factura in facturasAnio)
                {
                    // Ganancias por servicios
                    reporte.GananciaServicios += factura.PrecioServicio;

                    // Ganancias por productos (solo de facturas pagadas)
                    if (factura.Pagada && factura.Detalles != null)
                    {
                        reporte.GananciaProductos += factura.Detalles.Sum(d => d.Subtotal);
                    }

                    // Comisiones de empleados
                    if (factura.ComisionEmpleado > 0)
                    {
                        reporte.ComisionEmpleado += factura.ComisionEmpleado;
                    }
                }

                reporte.TotalGanado = reporte.GananciaServicios + reporte.GananciaProductos;
                reporte.GananciaNeta = reporte.TotalGanado - reporte.ComisionEmpleado;

                return reporte;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ganancias del año {Anio}", anio);
                throw;
            }
        }

        /// <summary>
        /// Obtiene resumen de ganancias de todos los tiempos
        /// </summary>
        public async Task<ReporteGanancias> ObtenerGananciasTotalesAsync()
        {
            try
            {
                var collection = _firebaseService.GetCollection("facturas");
                var snapshot = await collection.GetSnapshotAsync();

                var todasFacturas = snapshot.Documents
                    .Select(doc => doc.ConvertTo<Factura>())
                    .ToList();

                var reporte = new ReporteGanancias
                {
                    Mes = 0,
                    Anio = 0,
                    TotalFacturas = todasFacturas.Count,
                    FacturasPagadas = todasFacturas.Count(f => f.Pagada)
                };

                foreach (var factura in todasFacturas)
                {
                    reporte.GananciaServicios += factura.PrecioServicio;

                    if (factura.Pagada && factura.Detalles != null)
                    {
                        reporte.GananciaProductos += factura.Detalles.Sum(d => d.Subtotal);
                    }

                    if (factura.ComisionEmpleado > 0)
                    {
                        reporte.ComisionEmpleado += factura.ComisionEmpleado;
                    }
                }

                reporte.TotalGanado = reporte.GananciaServicios + reporte.GananciaProductos;
                reporte.GananciaNeta = reporte.TotalGanado - reporte.ComisionEmpleado;

                return reporte;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ganancias totales");
                throw;
            }
        }
    }

    /// <summary>
    /// Modelo para el reporte de ganancias
    /// </summary>
    public class ReporteGanancias
    {
        public int Mes { get; set; }
        public int Anio { get; set; }
        public int TotalFacturas { get; set; }
        public int FacturasPagadas { get; set; }
        public double GananciaServicios { get; set; }
        public double GananciaProductos { get; set; }
        public double ComisionEmpleado { get; set; }
        public double TotalGanado { get; set; }
        public double GananciaNeta { get; set; }

        public string NombreMes => Mes > 0 ? new DateTime(Anio, Mes, 1).ToString("MMMM") : "Total";
    }
}