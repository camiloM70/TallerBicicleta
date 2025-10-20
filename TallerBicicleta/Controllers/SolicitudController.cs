namespace TallerBicicleta.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SolicitudController : ControllerBase
    {
        private readonly SolicitudService _solicitudService;
        private readonly ILogger<SolicitudController> _logger;

        public SolicitudController(SolicitudService solicitudService, ILogger<SolicitudController> logger)
        {
            _solicitudService = solicitudService;
            _logger = logger;
        }

        /// <summary>
        /// GET: api/solicitud
        /// Obtiene todas las solicitudes
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<SolicitudServicio>>> ObtenerSolicitudes()
        {
            try
            {
                var solicitudes = await _solicitudService.ObtenerSolicitudesAsync();
                return Ok(solicitudes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener solicitudes");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/solicitud/pendientes
        /// Obtiene solicitudes pendientes (no asignadas)
        /// </summary>
        [HttpGet("pendientes")]
        public async Task<ActionResult<List<SolicitudServicio>>> ObtenerSolicitudesPendientes()
        {
            try
            {
                var solicitudes = await _solicitudService.ObtenerSolicitudesPendientesAsync();
                return Ok(solicitudes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener solicitudes pendientes");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/solicitud/cliente/{clienteId}
        /// Obtiene solicitudes de un cliente específico
        /// </summary>
        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<List<SolicitudServicio>>> ObtenerSolicitudesPorCliente(string clienteId)
        {
            try
            {
                var solicitudes = await _solicitudService.ObtenerSolicitudesPorClienteAsync(clienteId);
                return Ok(solicitudes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,  "Error al obtener solicitudes del cliente {clienteId}", clienteId);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/solicitud/empleado/{empleadoId}
        /// Obtiene solicitudes asignadas a un empleado específico
        /// </summary>
        [HttpGet("empleado/{empleadoId}")]
        public async Task<ActionResult<List<SolicitudServicio>>> ObtenerSolicitudesPorEmpleado(string empleadoId)
        {
            try
            {
                var solicitudes = await _solicitudService.ObtenerSolicitudesPorEmpleadoAsync(empleadoId);
                return Ok(solicitudes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener solicitudes del empleado {empleadoId}",empleadoId);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/solicitud/{id}
        /// Obtiene una solicitud específica por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<SolicitudServicio>> ObtenerSolicitud(string id)
        {
            try
            {
                var solicitud = await _solicitudService.ObtenerSolicitudPorIdAsync(id);
                if (solicitud == null)
                {
                    return NotFound(new { message = "Solicitud no encontrada" });
                }
                return Ok(solicitud);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener solicitud {id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/solicitud
        /// Crea una nueva solicitud de servicio
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<SolicitudServicio>> CrearSolicitud([FromBody] SolicitudServicio solicitud)
        {
            try
            {
                var nuevaSolicitud = await _solicitudService.CrearSolicitudAsync(solicitud);
                return CreatedAtAction(nameof(ObtenerSolicitud), new { id = nuevaSolicitud.Id }, nuevaSolicitud);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear solicitud");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// PUT: api/solicitud/{id}/asignar
        /// Asigna una solicitud a un empleado
        /// </summary>
        [HttpPut("{id}/asignar")]
        public async Task<IActionResult> AsignarSolicitud(string id, [FromBody] AsignarSolicitudDto dto)
        {
            try
            {
                await _solicitudService.AsignarSolicitudAsync(id, dto.EmpleadoId, dto.EmpleadoNombre);
                return Ok(new { message = "Solicitud asignada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar solicitud {id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// PUT: api/solicitud/{id}/completar
        /// Marca una solicitud como completada
        /// </summary>
        [HttpPut("{id}/completar")]
        public async Task<IActionResult> CompletarSolicitud(string id)
        {
            try
            {
                await _solicitudService.CompletarSolicitudAsync(id);
                return Ok(new { message = "Solicitud completada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar solicitud {id}",id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// PUT: api/solicitud/{id}/cancelar
        /// Cancela una solicitud
        /// </summary>
        [HttpPut("{id}/cancelar")]
        public async Task<IActionResult> CancelarSolicitud(string id)
        {
            try
            {
                await _solicitudService.CancelarSolicitudAsync(id);
                return Ok(new { message = "Solicitud cancelada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar solicitud {id}",id);
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }

    /// <summary>
    /// DTO para asignar una solicitud a un empleado
    /// </summary>
    public class AsignarSolicitudDto
    {
        public string EmpleadoId { get; set; } = string.Empty;
        public string EmpleadoNombre { get; set; } = string.Empty;
    }
}