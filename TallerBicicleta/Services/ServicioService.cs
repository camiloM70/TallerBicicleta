namespace TallerBicicleta.Services
{
    public class ServicioService
    {
        private readonly FirebaseService _firebaseService;
        private readonly ILogger<ServicioService> _logger;
        private const string COLLECTION_NAME = "servicios";

        public ServicioService(FirebaseService firebaseService, ILogger<ServicioService> logger)
        {
            _firebaseService = firebaseService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los servicios activos
        /// </summary>
        public async Task<List<Servicio>> ObtenerServiciosAsync()
        {
            try
            {
                var collection = _firebaseService.GetCollection(COLLECTION_NAME);
                var query = collection.WhereEqualTo("Activo", true);
                var snapshot = await query.GetSnapshotAsync();

                return [.. snapshot.Documents
                    .Select(doc =>
                    {
                        var servicio = doc.ConvertTo<Servicio>();
                        servicio.Id = doc.Id;
                        return servicio;
                    })
                    .OrderBy(s => s.Nombre)];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener servicios");
                throw new Exception($"Error al obtener servicios: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene un servicio por ID
        /// </summary>
        public async Task<Servicio?> ObtenerServicioPorIdAsync(string id)
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

                var servicio = snapshot.ConvertTo<Servicio>();
                servicio.Id = snapshot.Id;
                return servicio;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener servicio {id}",id);
                throw new Exception($"Error al obtener servicio: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Registra un nuevo servicio
        /// </summary>
        public async Task<Servicio> RegistrarServicioAsync(Servicio servicio)
        {
            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(servicio.Nombre))
                    throw new ArgumentException("El nombre del servicio es requerido");

                if (servicio.PrecioBase <= 0)
                    throw new ArgumentException("El precio base debe ser mayor a cero");

                servicio.FechaCreacion = DateTime.UtcNow;
                servicio.Activo = true;

                var collection = _firebaseService.GetCollection(COLLECTION_NAME);
                var docRef = await collection.AddAsync(servicio);
                servicio.Id = docRef.Id;

                _logger.LogInformation("Servicio registrado: {servicio.Nombre}", servicio.Nombre);
                return servicio;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar servicio");
                throw new Exception($"Error al registrar servicio: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Actualiza un servicio existente
        /// </summary>
        public async Task<Servicio> ActualizarServicioAsync(Servicio servicio)
        {
            try
            {
                if (string.IsNullOrEmpty(servicio.Id))
                    throw new ArgumentException("El ID del servicio es requerido");

                // Validaciones
                if (string.IsNullOrWhiteSpace(servicio.Nombre))
                    throw new ArgumentException("El nombre del servicio es requerido");

                if (servicio.PrecioBase <= 0)
                    throw new ArgumentException("El precio base debe ser mayor a cero");

                servicio.FechaModificacion = DateTime.UtcNow;

                var collection = _firebaseService.GetCollection(COLLECTION_NAME);
                var docRef = collection.Document(servicio.Id);
                await docRef.SetAsync(servicio, SetOptions.MergeAll);

                _logger.LogInformation("Servicio actualizado: {servicio.Nombre}", servicio.Nombre);
                return servicio;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar servicio {servicio.Id}", servicio.Id);
                throw new Exception($"Error al actualizar servicio: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Elimina un servicio (soft delete)
        /// </summary>
        public async Task EliminarServicioAsync(string id)
        {
            try
            {
                var collection = _firebaseService.GetCollection(COLLECTION_NAME);
                var docRef = collection.Document(id);

                await docRef.UpdateAsync("Activo", false);
                await docRef.UpdateAsync("FechaModificacion", DateTime.UtcNow);

                _logger.LogInformation("Servicio eliminado: {id}",id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar servicio {id}",id);
                throw new Exception($"Error al eliminar servicio: {ex.Message}", ex);
            }
        }
    }
}
