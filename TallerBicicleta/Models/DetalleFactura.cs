namespace TallerBicicleta.Models
{
    /// <summary>
    /// Clase DetalleFactura con anotaciones para Firestore
    /// Representa un producto incluido en una factura
    /// </summary>
    [FirestoreData]
    public class DetalleFactura
    {
        [FirestoreProperty]
        public string ProductoId { get; set; } = string.Empty;

        [FirestoreProperty]
        public string ProductoNombre { get; set; } = string.Empty;

        [FirestoreProperty]
        public int Cantidad { get; set; }

        [FirestoreProperty]
        public double PrecioUnitario { get; set; }

        [FirestoreProperty]
        public double Subtotal { get; set; }

        /// <summary>
        /// Calcula el subtotal del detalle
        /// </summary>
        public void CalcularSubtotal()
        {
            Subtotal = Cantidad * PrecioUnitario;
        }
    }

}

