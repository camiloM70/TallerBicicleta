namespace TallerBicicleta.Models
{
    /// <summary>
    /// Clase Cliente con anotaciones para Firestore
    /// Hereda de Usuario y añade propiedades específicas de cliente
    /// </summary>
    [FirestoreData]
    public class Cliente : Usuario
    {
        [FirestoreProperty]
        public string? Telefono { get; set; }

        [FirestoreProperty]
        public string? Direccion { get; set; }

        [FirestoreProperty]
        public bool Activo { get; set; } = true;

        [FirestoreProperty]
        public DateTime? FechaModificacion { get; set; }

        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public Cliente()
        {
            // Establecer el rol como Cliente por defecto
            RolUsuario = RolUsuario.Cliente;
        }
    }

}

