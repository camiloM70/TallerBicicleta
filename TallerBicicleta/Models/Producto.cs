namespace TallerBicicleta.Models
{
    /// <summary>
    /// Clase Producto con anotaciones para Firestore
    /// </summary>
    [FirestoreData]
    public class Producto
    {
        [FirestoreProperty]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string Nombre { get; set; } = string.Empty;

        [FirestoreProperty]
        public string? Descripcion { get; set; }

        [FirestoreProperty]
        public double Precio { get; set; }

        [FirestoreProperty]
        public int Stock { get; set; }

        [FirestoreProperty]
        public bool Activo { get; set; } = true;

        [FirestoreProperty]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [FirestoreProperty]
        public DateTime? FechaModificacion { get; set; }
    }

}
