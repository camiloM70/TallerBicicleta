namespace TallerBicicleta.Models
{
    /// <summary>
    /// Clase Servicio con anotaciones para Firestore
    /// Representa un tipo de servicio que ofrece la empresa
    /// </summary>
    [FirestoreData]
    public class Servicio
    {
        [FirestoreProperty]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string Nombre { get; set; } = string.Empty;

        [FirestoreProperty]
        public string? Descripcion { get; set; }

        [FirestoreProperty]
        public double PrecioBase { get; set; }

        [FirestoreProperty]
        public bool Activo { get; set; } = true;

        [FirestoreProperty]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [FirestoreProperty]
        public DateTime? FechaModificacion { get; set; }
    }
}

