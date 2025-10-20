namespace TallerBicicleta.Models
{
    /// <summary>
    /// Enumeración para los roles de usuario
    /// </summary>
    public enum RolUsuario
    {
        Administrador = 1,
        Empleado = 2,
        Cliente = 3
    }

    /// <summary>
    /// Clase base Usuario con anotaciones para Firestore
    /// </summary>
    [FirestoreData]
    public class Usuario
    {
        /// <summary>
        /// ID del usuario (se usa el ID del documento de Firestore)
        /// </summary>
        [FirestoreProperty]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string NombreUsuario { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Password { get; set; } = string.Empty;

        [FirestoreProperty]
        public int Rol { get; set; } = (int)RolUsuario.Cliente;

        [FirestoreProperty]
        public string CorreoElectronico { get; set; } = string.Empty;

        [FirestoreProperty]
        public string? NombreCompleto { get; set; }

        [FirestoreProperty]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Propiedad auxiliar para obtener el rol como enumeración
        /// </summary>
        public RolUsuario RolUsuario
        {
            get => (RolUsuario)Rol;
            set => Rol = (int)value;
        }
    }

}

