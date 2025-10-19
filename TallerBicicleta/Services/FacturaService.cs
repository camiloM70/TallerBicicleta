namespace TallerBicicleta.Services
{
    public class FacturaService
    {
        private readonly FirebaseService _firebaseService;
        private readonly SolicitudService _solicitudService;
        private readonly ProductoService _productoService;
        private readonly EmailService _emailService;
        private readonly ILogger<FacturaService> _logger;
        private const string COLLECTION_NAME = "facturas";

        public FacturaService(
            FirebaseService firebaseService,
            SolicitudService solicitudService,
            ProductoService productoService,
            EmailService emailService,
            ILogger<FacturaService> logger)
        {
            _firebaseService = firebaseService;
            _solicitudService = solicitudService;
            _productoService = productoService;
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las facturas
        /// </summary>
        public async Task<List<Factura>> ObtenerFacturasAsync()
        {
            try
            {
                var collection = _firebaseService.GetCollection(COLLECTION_NAME);
                var snapshot = await collection.GetSnapshotAsync();

                return [.. snapshot.Documents
                    .Select(doc =>
                    {
                        var factura = doc.ConvertTo<Factura>();
                        factura.Id = doc.Id;
                        return factura;
                    })
                    .OrderByDescending(f => f.FechaEmision)];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener facturas");
                throw new Exception($"Error al obtener facturas: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene facturas de un cliente específico
        /// </summary>
        public async Task<List<Factura>> ObtenerFacturasPorClienteAsync(string clienteId)
        {
            try
            {
                var collection = _firebaseService.GetCollection(COLLECTION_NAME);
                var query = collection.WhereEqualTo("ClienteId", clienteId);
                var snapshot = await query.GetSnapshotAsync();

                return [.. snapshot.Documents
                    .Select(doc =>
                    {
                        var factura = doc.ConvertTo<Factura>();
                        factura.Id = doc.Id;
                        return factura;
                    })
                    .OrderByDescending(f => f.FechaEmision)];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener facturas del cliente {clienteId}", clienteId);
                throw new Exception($"Error al obtener facturas del cliente: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene una factura por ID
        /// </summary>
        public async Task<Factura?> ObtenerFacturaPorIdAsync(string id)
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

                var factura = snapshot.ConvertTo<Factura>();
                factura.Id = snapshot.Id;
                return factura;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener factura {id}", id);
                throw new Exception($"Error al obtener factura: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Crea una factura directamente (sin generar desde solicitud)
        /// </summary>
        public async Task<Factura> CrearFacturaAsync(Factura factura)
        {
            try
            {
                // Validaciones básicas
                if (string.IsNullOrWhiteSpace(factura.ClienteId))
                    throw new ArgumentException("El ClienteId es requerido");

                if (string.IsNullOrWhiteSpace(factura.EmpleadoId))
                    throw new ArgumentException("El EmpleadoId es requerido");

                if (factura.Detalles == null || factura.Detalles.Count == 0)
                    throw new ArgumentException("Debe agregar al menos un producto");

                // Verificar y reducir stock de productos
                foreach (var detalle in factura.Detalles)
                {
                    await _productoService.ReducirStockAsync(detalle.ProductoId, detalle.Cantidad);
                }

                // Asegurar que tenga número de factura
                if (string.IsNullOrWhiteSpace(factura.NumeroFactura))
                {
                    factura.NumeroFactura = await GenerarNumeroFacturaAsync();
                }

                // Asegurar que tenga fecha
                if (factura.FechaEmision == default)
                {
                    factura.FechaEmision = DateTime.UtcNow;
                }

                // Guardar en Firestore
                var collection = _firebaseService.GetCollection(COLLECTION_NAME);
                var docRef = await collection.AddAsync(factura);
                factura.Id = docRef.Id;

                _logger.LogInformation("Factura creada: {factura.NumeroFactura}", factura.NumeroFactura);
                return factura;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear factura");
                throw new Exception($"Error al crear factura: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Genera una factura a partir de una solicitud
        /// </summary>
        public async Task<Factura> GenerarFacturaAsync(string solicitudId, List<DetalleFactura> detalles, double porcentajeComision)
        {
            try
            {
                // Obtener la solicitud
                var solicitud = await _solicitudService.ObtenerSolicitudPorIdAsync(solicitudId);
                if (solicitud is not { })
                    throw new Exception("Solicitud no encontrada");

                if (solicitud.EstadoSolicitud != EstadoSolicitud.EnProceso)
                    throw new Exception("La solicitud debe estar en proceso para generar factura");

                // Validar y calcular subtotales de productos
                foreach (var detalle in detalles)
                {
                    // Verificar stock y reducirlo
                    await _productoService.ReducirStockAsync(detalle.ProductoId, detalle.Cantidad);
                    detalle.CalcularSubtotal();
                }

                // Crear la factura
                var factura = new Factura
                {
                    NumeroFactura = await GenerarNumeroFacturaAsync(),
                    ClienteId = solicitud.ClienteId,
                    ClienteNombre = solicitud.ClienteNombre,
                    EmpleadoId = solicitud.EmpleadoId ?? string.Empty,
                    EmpleadoNombre = solicitud.EmpleadoNombre,
                    SolicitudId = solicitudId,
                    ServicioNombre = solicitud.ServicioNombre,
                    PrecioServicio = 0, // Se debe obtener del servicio
                    Detalles = detalles,
                    FechaEmision = DateTime.UtcNow
                };

                // Calcular totales
                factura.CalcularTotales(porcentajeComision);

                // Guardar en Firestore
                var collection = _firebaseService.GetCollection(COLLECTION_NAME);
                var docRef = await collection.AddAsync(factura);
                factura.Id = docRef.Id;

                // Marcar la solicitud como completada
                await _solicitudService.CompletarSolicitudAsync(solicitudId);

                // Intentar obtener el email del cliente y enviar factura
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // Obtener el email del cliente desde Firebase
                        var usuariosCollection = _firebaseService.GetCollection("usuarios");
                        var clienteDoc = await usuariosCollection.Document(solicitud.ClienteId).GetSnapshotAsync();

                        if (clienteDoc.Exists)
                        {
                            var clienteEmail = clienteDoc.GetValue<string>("CorreoElectronico");
                            if (!string.IsNullOrEmpty(clienteEmail))
                            {
                                await _emailService.EnviarFacturaPorCorreoAsync(factura, clienteEmail);
                                _logger.LogInformation("Factura {factura.NumeroFactura} enviada por correo a {clienteEmail}", factura.NumeroFactura, clienteEmail);
                            }
                        }
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogWarning(emailEx, "No se pudo enviar el email de la factura {factura.NumeroFactura}", factura.NumeroFactura);
                    }
                });

                _logger.LogInformation("Factura generada: {factura.NumeroFactura}", factura.NumeroFactura);
                return factura;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar factura");
                throw new Exception($"Error al generar factura: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Registra un abono/pago en una factura
        /// </summary>
        public async Task RegistrarAbonoEnFacturaAsync(string facturaId, double montoAbono)
        {
            try
            {
                var factura = await ObtenerFacturaPorIdAsync(facturaId);
                if (factura is not { })
                    throw new Exception("Factura no encontrada");

                if (factura.Pagada)
                    throw new InvalidOperationException("La factura ya está pagada");

                if (montoAbono <= 0)
                    throw new ArgumentOutOfRangeException(nameof(montoAbono), "El monto del abono debe ser mayor a cero");

                if (montoAbono > factura.Saldo)
                    throw new ArgumentOutOfRangeException(nameof(montoAbono), $"El monto del abono ({montoAbono}) excede el saldo ({factura.Saldo})");

                double nuevoSaldo = factura.Saldo - montoAbono;
                bool estaPagada = nuevoSaldo == 0;

                var updates = new Dictionary<string, object>
                {
                    ["Saldo"] = nuevoSaldo,
                    ["Pagada"] = estaPagada
                };

                if (estaPagada)
                {
                    updates["FechaPago"] = DateTime.UtcNow;
                }

                await _firebaseService.GetCollection(COLLECTION_NAME)
                                      .Document(facturaId)
                                      .UpdateAsync(updates);

                _logger.LogInformation("Abono registrado en factura {FacturaId}: {MontoAbono}", facturaId, montoAbono);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar abono en factura {FacturaId}", facturaId);
                throw;
            }
        }

        /// <summary>
        /// Genera un número de factura único
        /// </summary>
        private async Task<string> GenerarNumeroFacturaAsync()
        {
            try
            {
                var collection = _firebaseService.GetCollection(COLLECTION_NAME);
                var snapshot = await collection.GetSnapshotAsync();

                int count = snapshot.Count + 1;
                string numero = $"FAC-{DateTime.UtcNow:yyyyMMdd}-{count:D5}";

                return numero;
            }
            catch
            {
                // Si falla, usar timestamp
                return $"FAC-{DateTime.UtcNow:yyyyMMddHHmmss}";
            }
        }
    }
}