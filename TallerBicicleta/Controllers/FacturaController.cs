namespace TallerBicicleta.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FacturaController : ControllerBase
    {
        private readonly FacturaService _facturaService;
        private readonly EmailService _emailService;
        private readonly ILogger<FacturaController> _logger;

        public FacturaController(
            FacturaService facturaService,
            EmailService emailService,
            ILogger<FacturaController> logger)
        {
            _facturaService = facturaService;
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// GET: api/factura
        /// Obtiene todas las facturas
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<Factura>>> ObtenerFacturas()
        {
            try
            {
                var facturas = await _facturaService.ObtenerFacturasAsync();
                return Ok(facturas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener facturas");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/factura/cliente/{clienteId}
        /// Obtiene facturas de un cliente específico
        /// </summary>
        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<List<Factura>>> ObtenerFacturasPorCliente(string clienteId)
        {
            try
            {
                var facturas = await _facturaService.ObtenerFacturasPorClienteAsync(clienteId);
                return Ok(facturas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener facturas del cliente {clienteId}", clienteId);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/factura/{id}
        /// Obtiene una factura específica por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Factura>> ObtenerFactura(string id)
        {
            try
            {
                var factura = await _facturaService.ObtenerFacturaPorIdAsync(id);
                if (factura == null)
                {
                    return NotFound(new { message = "Factura no encontrada" });
                }
                return Ok(factura);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener factura {id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/factura
        /// Crea una nueva factura directamente
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Factura>> CrearFactura([FromBody] Factura factura)
        {
            try
            {
                _logger.LogInformation("Creando factura para cliente {factura.ClienteId}", factura.ClienteId);

                var facturaCreada = await _facturaService.CrearFacturaAsync(factura);

                // Intentar enviar la factura por correo (no bloquear si falla)
                if (!string.IsNullOrEmpty(factura.ClienteEmail))
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _emailService.EnviarFacturaPorCorreoAsync(facturaCreada, factura.ClienteEmail);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "No se pudo enviar el email de la factura {facturaCreada.NumeroFactura}", facturaCreada.NumeroFactura);
                        }
                    });
                }

                return CreatedAtAction(nameof(ObtenerFactura), new { id = facturaCreada.Id }, facturaCreada);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error de validación al crear factura");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear factura");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/factura/generar
        /// Genera una nueva factura a partir de una solicitud
        /// </summary>
        [HttpPost("generar")]
        public async Task<ActionResult<Factura>> GenerarFactura([FromBody] GenerarFacturaDto dto)
        {
            try
            {
                var factura = await _facturaService.GenerarFacturaAsync(
                    dto.SolicitudId,
                    dto.Detalles,
                    dto.PorcentajeComision);

                return CreatedAtAction(nameof(ObtenerFactura), new { id = factura.Id }, factura);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar factura");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/factura/{id}/abono
        /// Registra un abono en una factura
        /// </summary>
        [HttpPost("{id}/abono")]
        public async Task<IActionResult> RegistrarAbono(string id, [FromBody] AbonoDto dto)
        {
            try
            {
                await _facturaService.RegistrarAbonoEnFacturaAsync(id, dto.Monto);
                return Ok(new { message = "Abono registrado correctamente" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar abono en factura {id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }

    /// <summary>
    /// DTO para generar una factura
    /// </summary>
    public class GenerarFacturaDto
    {
        public string SolicitudId { get; set; } = string.Empty;
        public List<DetalleFactura> Detalles { get; set; } = [];
        public double PorcentajeComision { get; set; } = 0.80; // 80% para empleado, 20% para dueño
    }

    /// <summary>
    /// DTO para registrar un abono
    /// </summary>
    public class AbonoDto
    {
        public double Monto { get; set; }
    }
}