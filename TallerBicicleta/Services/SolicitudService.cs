namespace TallerBicicleta.Services
{
    public class SolicitudService
    {
        private readonly FirebaseService _firebaseService;
        private readonly ILogger<SolicitudService> _logger;
        private const string COLLECTION_NAME = "solicitudes";

        public SolicitudService(FirebaseService firebaseService, ILogger<SolicitudService> logger)
        {
            _firebaseService = firebaseService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las solicitudes
        /// </summary>
        public async Task<List<SolicitudServicio>> ObtenerSolicitudesAsync()
        {
            try
            {
                var collection = _firebaseService.GetCollection(COLLECTION_NAME);
                var snapshot = await collection.GetSnapshotAsync();

                return [.. snapshot.Documents
                    .Select(doc =>
                    {
                        var solicitud = doc.ConvertTo<SolicitudServicio>();
                        solicitud.Id = doc.Id;
                        return solicitud;
                    })
                    .OrderByDescending(s => s.FechaSolicitud)];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener solicitudes");
                throw new Exception($"Error al obtener solicitudes: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene solicitudes pendientes (no asignadas)
        /// </summary>
        public async Task<List<SolicitudServicio>> ObtenerSolicitudesPendientesAsync()
        {
            try
            {
                var collection = _firebaseService.GetCollection(COLLECTION_NAME);
                var query = collection.WhereEqualTo("Estado", (int)EstadoSolicitud.Pendiente);
                var snapshot = await query.GetSnapshotAsync();

                return [.. snapshot.Documents
                    .Select(doc =>
                    {
                        var solicitud = doc.ConvertTo<SolicitudServicio>();
                        solicitud.Id = doc.Id;
                        return solicitud;
                    })
                    .OrderBy(s => s.FechaSolicitud)];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener solicitudes pendientes");
                throw new Exception($"Error al obtener solicitudes pendientes: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene solicitudes de un cliente específico
        /// </summary>
        public async Task<List<SolicitudServicio>> ObtenerSolicitudesPorClienteAsync(string clienteId)
        {
            try
            {
                var collection = _firebaseService.GetCollection(COLLECTION_NAME);
                var query = collection.WhereEqualTo("ClienteId", clienteId);
                var snapshot = await query.GetSnapshotAsync();

                return [.. snapshot.Documents
                    .Select(doc =>
                    {
                        var solicitud = doc.ConvertTo<SolicitudServicio>();
                        solicitud.Id = doc.Id;
                        return solicitud;
                    })
                    .OrderByDescending(s => s.FechaSolicitud)];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener solicitudes del cliente {clienteId}", clienteId);
                throw new Exception($"Error al obtener solicitudes del cliente: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene solicitudes asignadas a un empleado específico
        /// </summary>
        public async Task<List<SolicitudServicio>> ObtenerSolicitudesPorEmpleadoAsync(string empleadoId)
        {
            try
            {
                var collection = _firebaseService.GetCollection(COLLECTION_NAME);
                var query = collection.WhereEqualTo("EmpleadoId", empleadoId);
                var snapshot = await query.GetSnapshotAsync();

                return [.. snapshot.Documents
                    .Select(doc =>
                    {
                        var solicitud = doc.ConvertTo<SolicitudServicio>();
                        solicitud.Id = doc.Id;
                        return solicitud;
                    })
                    .OrderByDescending(s => s.FechaSolicitud)];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener solicitudes del empleado {empleadoId}", empleadoId);
                throw new Exception($"Error al obtener solicitudes del empleado: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene una solicitud por ID
        /// </summary>
        public async Task<SolicitudServicio?> ObtenerSolicitudPorIdAsync(string id)
        {
            try
            {
                var collection = _firebaseService.GetCollection(COLLECTION_NAME);
                var docRef = collection.Document(id);
                var snapshot = await docRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return null;
                }

                var solicitud = snapshot.ConvertTo<SolicitudServicio>();
                solicitud.Id = snapshot.Id;
                return solicitud;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener solicitud {id}",id);
                throw new Exception($"Error al obtener solicitud: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Crea una nueva solicitud de servicio (Cliente)
        /// </summary>
        public async Task<SolicitudServicio> CrearSolicitudAsync(SolicitudServicio solicitud)
        {
            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(solicitud.ClienteId))
                    throw new ArgumentException("El ID del cliente es requerido");

                if (string.IsNullOrWhiteSpace(solicitud.Descripcion))
                    throw new ArgumentException("La descripción es requerida");

                // ServicioId es opcional - el empleado lo asignará al tomar la solicitud
                solicitud.Estado = (int)EstadoSolicitud.Pendiente;
                solicitud.FechaSolicitud = DateTime.UtcNow;
                solicitud.EmpleadoId = null;
                solicitud.FechaAsignacion = null;
                solicitud.FechaCompletada = null;

                var collection = _firebaseService.GetCollection(COLLECTION_NAME);
                var docRef = await collection.AddAsync(solicitud);
                solicitud.Id = docRef.Id;

                _logger.LogInformation("Solicitud creada: {solicitud.Id}", solicitud.Id);
                return solicitud;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear solicitud");
                throw new Exception($"Error al crear solicitud: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Asigna una solicitud a un empleado (Empleado toma el servicio)
        /// </summary>
        public async Task AsignarSolicitudAsync(string solicitudId, string empleadoId, string empleadoNombre)
        {
            try
            {
                var collection = _firebaseService.GetCollection(COLLECTION_NAME);
                var docRef = collection.Document(solicitudId);

                // Verificar que la solicitud esté pendiente
                var snapshot = await docRef.GetSnapshotAsync();
                if (!snapshot.Exists)
                    throw new Exception("Solicitud no encontrada");

                var solicitud = snapshot.ConvertTo<SolicitudServicio>();
                if (solicitud.EstadoSolicitud != EstadoSolicitud.Pendiente)
                    throw new Exception("La solicitud ya no está disponible");

                await docRef.UpdateAsync(new Dictionary<string, object>
                {
                    { "EmpleadoId", empleadoId },
                    { "EmpleadoNombre", empleadoNombre },
                    { "Estado", (int)EstadoSolicitud.EnProceso },
                    { "FechaAsignacion", DateTime.UtcNow }
                });

                _logger.LogInformation("Solicitud {solicitudId} asignada al empleado {empleadoId}", solicitudId, empleadoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar solicitud {solicitudId}", solicitudId);
                throw new Exception($"Error al asignar solicitud: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Marca una solicitud como completada (cuando se genera la factura)
        /// </summary>
        public async Task CompletarSolicitudAsync(string solicitudId)
        {
            try
            {
                var collection = _firebaseService.GetCollection(COLLECTION_NAME);
                var docRef = collection.Document(solicitudId);

                await docRef.UpdateAsync(new Dictionary<string, object>
                {
                    { "Estado", (int)EstadoSolicitud.Completada },
                    { "FechaCompletada", DateTime.UtcNow }
                });

                _logger.LogInformation("Solicitud {solicitudId} completada", solicitudId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar solicitud {solicitudId}", solicitudId);
                throw new Exception($"Error al completar solicitud: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cancela una solicitud
        /// </summary>
        public async Task CancelarSolicitudAsync(string solicitudId)
        {
            try
            {
                var collection = _firebaseService.GetCollection(COLLECTION_NAME);
                var docRef = collection.Document(solicitudId);

                await docRef.UpdateAsync("Estado", (int)EstadoSolicitud.Cancelada);

                _logger.LogInformation("Solicitud {solicitudId} cancelada", solicitudId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar solicitud {solicitudId}", solicitudId);
                throw new Exception($"Error al cancelar solicitud: {ex.Message}", ex);
            }
        }
    }

}
