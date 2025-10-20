namespace TallerBicicleta.Models
{
    /// <summary>
    /// Clase Empleado con anotaciones para Firestore
    /// Hereda de Usuario y añade propiedades específicas de empleado
    /// </summary>
    [FirestoreData]
    public class Empleado : Usuario
    {
        [FirestoreProperty]
        public double PorcentajeComision { get; set; } = 0.80; // 80% por defecto (empleado se queda con 80%, dueño con 20%)

        [FirestoreProperty]
        public bool Activo { get; set; } = true;

        [FirestoreProperty]
        public DateTime? FechaModificacion { get; set; }

        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public Empleado()
        {
            // Establecer el rol como Empleado por defecto
            RolUsuario = RolUsuario.Empleado;
        }
    }

}

