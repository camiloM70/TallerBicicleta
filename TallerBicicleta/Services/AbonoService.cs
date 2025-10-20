using TallerBicicleta.Data;

namespace TallerBicicleta.Services
{
    public class AbonoService
    {
        private readonly FirebaseService _firebaseService;
        private readonly FacturaService _facturaService;
        private readonly ILogger<AbonoService> _logger;
        private const string COLLECTION_NAME = "abonos";

        public AbonoService(
            FirebaseService firebaseService,
            FacturaService facturaService,
            ILogger<AbonoService> logger)
        {
            _firebaseService = firebaseService;
            _facturaService = facturaService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los abonos de una factura
        /// </summary>
        public async Task<List<Abono>> ObtenerAbonosPorFacturaAsync(string facturaId)
        {
            try
            {
                var collection = _firebaseService.GetCollection(COLLECTION_NAME);
                var query = collection.WhereEqualTo("FacturaId", facturaId);
                var snapshot = await query.GetSnapshotAsync();

                return [.. snapshot.Documents
                    .Select(doc =>
                    {
                        var abono = doc.ConvertTo<Abono>();
                        abono.Id = doc.Id;
                        return abono;
                    })
                    .OrderByDescending(a => a.FechaAbono)];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener abonos de la factura {facturaId}", facturaId);
                throw new Exception($"Error al obtener abonos: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene todos los abonos de un cliente
        /// </summary>
        public async Task<List<Abono>> ObtenerAbonosPorClienteAsync(string clienteId)
        {
            try
            {
                var collection = _firebaseService.GetCollection(COLLECTION_NAME);
                var query = collection.WhereEqualTo("ClienteId", clienteId);
                var snapshot = await query.GetSnapshotAsync();

                return [.. snapshot.Documents
                    .Select(doc =>
                    {
                        var abono = doc.ConvertTo<Abono>();
                        abono.Id = doc.Id;
                        return abono;
                    })
                    .OrderByDescending(a => a.FechaAbono)];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener abonos del cliente {clienteId}", clienteId);
                throw new Exception($"Error al obtener abonos del cliente: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Registra un nuevo abono a una factura
        /// </summary>
        public async Task<Abono> RegistrarAbonoAsync(Abono abono)
        {
            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(abono.FacturaId))
                    throw new ArgumentException("El ID de la factura es requerido");

                if (abono.Monto <= 0)
                    throw new ArgumentException("El monto debe ser mayor a cero");

                // Verificar que la factura existe y obtener datos
                var factura = await _facturaService.ObtenerFacturaPorIdAsync(abono.FacturaId);
                if (factura is not { })
                    throw new Exception("Factura no encontrada");

                if (factura.Pagada)
                    throw new Exception("La factura ya está completamente pagada");

                if (abono.Monto > factura.Saldo)
                    throw new ArgumentException($"El monto del abono ({abono.Monto}) excede el saldo de la factura ({factura.Saldo})");

                // Completar datos del abono
                abono.NumeroFactura = factura.NumeroFactura;
                abono.ClienteId = factura.ClienteId;
                abono.ClienteNombre = factura.ClienteNombre;
                abono.FechaAbono = DateTime.UtcNow;

                // Guardar el abono
                var collection = _firebaseService.GetCollection(COLLECTION_NAME);
                var docRef = await collection.AddAsync(abono);
                abono.Id = docRef.Id;

                // Actualizar el saldo de la factura
                await _facturaService.RegistrarAbonoEnFacturaAsync(abono.FacturaId, abono.Monto);

                _logger.LogInformation("Abono registrado: {abono.Id} - Factura: {abono.NumeroFactura} - Monto: {abono.Monto}", abono.Id, abono.NumeroFactura, abono.Monto);
                return abono;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar abono");
                throw new Exception($"Error al registrar abono: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene un abono por ID
        /// </summary>
        public async Task<Abono?> ObtenerAbonoPorIdAsync(string id)
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

                var abono = snapshot.ConvertTo<Abono>();
                abono.Id = snapshot.Id;
                return abono;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener abono {id}", id);
                throw new Exception($"Error al obtener abono: {ex.Message}", ex);
            }
        }
    }
}