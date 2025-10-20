namespace TallerBicicleta.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductoController : ControllerBase
    {
        private readonly ProductoService _productoService;
        private readonly ILogger<ProductoController> _logger;

        public ProductoController(ProductoService productoService, ILogger<ProductoController> logger)
        {
            _productoService = productoService;
            _logger = logger;
        }

        /// <summary>
        /// GET: api/producto
        /// Obtiene todos los productos activos
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<Producto>>> ObtenerProductos()
        {
            try
            {
                var productos = await _productoService.ObtenerProductosAsync();
                return Ok(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/producto/{id}
        /// Obtiene un producto específico por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Producto>> ObtenerProducto(string id)
        {
            try
            {
                var producto = await _productoService.ObtenerProductoPorIdAsync(id);
                if (producto == null)
                {
                    return NotFound(new { message = "Producto no encontrado" });
                }
                return Ok(producto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener producto {id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/producto
        /// Crea un nuevo producto
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Producto>> CrearProducto([FromBody] Producto producto)
        {
            try
            {
                var nuevoProducto = await _productoService.RegistrarProductoAsync(producto);
                return CreatedAtAction(nameof(ObtenerProducto), new { id = nuevoProducto.Id }, nuevoProducto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear producto");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// PUT: api/producto/{id}
        /// Actualiza un producto existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<Producto>> ActualizarProducto(string id, [FromBody] Producto producto)
        {
            try
            {
                producto.Id = id;
                var productoActualizado = await _productoService.ActualizarProductoAsync(producto);
                return Ok(productoActualizado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar producto {id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// DELETE: api/producto/{id}
        /// Elimina (desactiva) un producto
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarProducto(string id)
        {
            try
            {
                await _productoService.EliminarProductoAsync(id);
                return Ok(new { message = "Producto eliminado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar producto {id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// PUT: api/producto/{id}/stock
        /// Actualiza el stock de un producto
        /// </summary>
        [HttpPut("{id}/stock")]
        public async Task<IActionResult> ActualizarStock(string id, [FromBody] int nuevoStock)
        {
            try
            {
                await _productoService.ActualizarStockAsync(id, nuevoStock);
                return Ok(new { message = "Stock actualizado correctamente", nuevoStock });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar stock del producto {id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}