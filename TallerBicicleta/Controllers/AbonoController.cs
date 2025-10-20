namespace TallerBicicleta.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AbonoController : ControllerBase
    {
        private readonly AbonoService _abonoService;
        private readonly ILogger<AbonoController> _logger;

        public AbonoController(AbonoService abonoService, ILogger<AbonoController> logger)
        {
            _abonoService = abonoService;
            _logger = logger;
        }

        /// <summary>
        /// GET: api/abono/factura/{facturaId}
        /// Obtiene todos los abonos de una factura
        /// </summary>
        [HttpGet("factura/{facturaId}")]
        public async Task<ActionResult<List<Abono>>> ObtenerAbonosPorFactura(string facturaId)
        {
            try
            {
                var abonos = await _abonoService.ObtenerAbonosPorFacturaAsync(facturaId);
                return Ok(abonos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener abonos de la factura {facturaId}", facturaId);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/abono/cliente/{clienteId}
        /// Obtiene todos los abonos de un cliente
        /// </summary>
        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<List<Abono>>> ObtenerAbonosPorCliente(string clienteId)
        {
            try
            {
                var abonos = await _abonoService.ObtenerAbonosPorClienteAsync(clienteId);
                return Ok(abonos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener abonos del cliente {clienteId}", clienteId);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/abono/{id}
        /// Obtiene un abono específico por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Abono>> ObtenerAbono(string id)
        {
            try
            {
                var abono = await _abonoService.ObtenerAbonoPorIdAsync(id);
                if (abono == null)
                {
                    return NotFound(new { message = "Abono no encontrado" });
                }
                return Ok(abono);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener abono {id}",id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/abono
        /// Registra un nuevo abono
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Abono>> RegistrarAbono([FromBody] Abono abono)
        {
            try
            {
                var nuevoAbono = await _abonoService.RegistrarAbonoAsync(abono);
                return CreatedAtAction(nameof(ObtenerAbono), new { id = nuevoAbono.Id }, nuevoAbono);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar abono");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}