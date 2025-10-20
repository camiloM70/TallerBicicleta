namespace TallerBicicleta.Models
{
    /// <summary>
    /// Enumeración para los estados de una solicitud
    /// </summary>
    public enum EstadoSolicitud
    {
        Pendiente = 1,      // Esperando ser tomada por un empleado
        EnProceso = 2,      // Empleado trabajando en ella
        Completada = 3,     // Servicio terminado y facturado
        Cancelada = 4       // Cancelada por el cliente o admin
    }

    /// <summary>
    /// Clase SolicitudServicio con anotaciones para Firestore
    /// Representa una solicitud de servicio hecha por un cliente
    /// </summary>
    [FirestoreData]
    public class SolicitudServicio
    {
        [FirestoreProperty]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string ClienteId { get; set; } = string.Empty;

        [FirestoreProperty]
        public string? ClienteNombre { get; set; } // Para mostrar en listados

        [FirestoreProperty]
        public string? ServicioId { get; set; } // Opcional - El empleado lo asigna al tomar la solicitud

        [FirestoreProperty]
        public string? ServicioNombre { get; set; } // Para mostrar en listados

        [FirestoreProperty]
        public string? EmpleadoId { get; set; } // Null si no ha sido asignado

        [FirestoreProperty]
        public string? EmpleadoNombre { get; set; }

        [FirestoreProperty]
        public string Descripcion { get; set; } = string.Empty;

        [FirestoreProperty]
        public string? Detalle { get; set; }

        [FirestoreProperty]
        public int Estado { get; set; } = (int)EstadoSolicitud.Pendiente;

        [FirestoreProperty]
        public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;

        [FirestoreProperty]
        public DateTime? FechaAsignacion { get; set; }

        [FirestoreProperty]
        public DateTime? FechaCompletada { get; set; }

        /// <summary>
        /// Propiedad auxiliar para obtener el estado como enumeración
        /// </summary>
        public EstadoSolicitud EstadoSolicitud
        {
            get => (EstadoSolicitud)Estado;
            set => Estado = (int)value;
        }
    }

}
