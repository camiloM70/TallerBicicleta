namespace TallerBicicleta.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ServicioController(ServicioService servicioService, ILogger<ServicioController> logger) : ControllerBase
    {
        private readonly ServicioService _servicioService = servicioService;
        private readonly ILogger<ServicioController> _logger = logger;

        /// <summary>
        /// GET: api/servicio
        /// Obtiene todos los servicios activos
        /// </summary>
        [HttpGet("")]
        public async Task<ActionResult<List<Servicio>>> ObtenerServicios()
        {
            try
            {
                var servicios = await _servicioService.ObtenerServiciosAsync();
                return Ok(servicios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener servicios");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/servicio/{id}
        /// Obtiene un servicio específico por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Servicio>> ObtenerServicio(string id)
        {
            try
            {
                var servicio = await _servicioService.ObtenerServicioPorIdAsync(id);
                if (servicio == null)
                {
                    return NotFound(new { message = "Servicio no encontrado" });
                }
                return Ok(servicio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener servicio {id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/servicio
        /// Crea un nuevo servicio
        /// </summary>
        [HttpPost("")]
        public async Task<ActionResult<Servicio>> CrearServicio([FromBody] Servicio servicio)
        {
            try
            {
                var nuevoServicio = await _servicioService.RegistrarServicioAsync(servicio);
                return CreatedAtAction(nameof(ObtenerServicio), new { id = nuevoServicio.Id }, nuevoServicio);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear servicio");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// PUT: api/servicio/{id}
        /// Actualiza un servicio existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<Servicio>> ActualizarServicio(string id, [FromBody] Servicio servicio)
        {
            try
            {
                servicio.Id = id;
                var servicioActualizado = await _servicioService.ActualizarServicioAsync(servicio);
                return Ok(servicioActualizado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar servicio {id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// DELETE: api/servicio/{id}
        /// Elimina (desactiva) un servicio
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarServicio(string id)
        {
            try
            {
                await _servicioService.EliminarServicioAsync(id);
                return Ok(new { message = "Servicio eliminado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar servicio {id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}