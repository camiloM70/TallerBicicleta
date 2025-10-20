namespace TallerBicicleta.Models
{
    /// <summary>
    /// Clase Factura con anotaciones para Firestore
    /// Representa una factura generada por un empleado para un cliente
    /// </summary>
    [FirestoreData]
    public class Factura
    {
        [FirestoreProperty]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string NumeroFactura { get; set; } = string.Empty;

        [FirestoreProperty]
        public string ClienteId { get; set; } = string.Empty;

        [FirestoreProperty]
        public string? ClienteNombre { get; set; }

        [FirestoreProperty]
        public string? ClienteCorreo { get; set; }

        [FirestoreProperty]
        public string EmpleadoId { get; set; } = string.Empty;

        [FirestoreProperty]
        public string? EmpleadoNombre { get; set; }

        [FirestoreProperty]
        public string SolicitudId { get; set; } = string.Empty;

        [FirestoreProperty]
        public string? ServicioNombre { get; set; }

        [FirestoreProperty]
        public double PrecioServicio { get; set; }

        [FirestoreProperty]
        public List<DetalleFactura> Detalles { get; set; } = [];

        [FirestoreProperty]
        public double SubtotalProductos { get; set; }

        [FirestoreProperty]
        public double ComisionEmpleado { get; set; }

        [FirestoreProperty]
        public double Total { get; set; }

        [FirestoreProperty]
        public double Saldo { get; set; } // Lo que falta por pagar

        [FirestoreProperty]
        public bool Pagada { get; set; } = false;

        [FirestoreProperty]
        public string? CodigoBarras { get; set; } // Base64 del código de barras

        [FirestoreProperty]
        public DateTime FechaEmision { get; set; } = DateTime.UtcNow;

        [FirestoreProperty]
        public DateTime? FechaPago { get; set; }

        /// <summary>
        /// Propiedad auxiliar para recibir el email del cliente en el request (no se guarda en Firestore)
        /// </summary>
        public string? ClienteEmail { get; set; }

        /// <summary>
        /// Calcula los totales de la factura
        /// </summary>
        public void CalcularTotales(double porcentajeComision)
        {
            // Calcular subtotal de productos
            SubtotalProductos = Detalles.Sum(d => d.Subtotal);

            // Calcular comisión del empleado
            ComisionEmpleado = (PrecioServicio + SubtotalProductos) * porcentajeComision;

            // Calcular total
            Total = PrecioServicio + SubtotalProductos;

            // Inicializar saldo si es una nueva factura
            if (Saldo == 0)
            {
                Saldo = Total;
            }
        }
    }

}

