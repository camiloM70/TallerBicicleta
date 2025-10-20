namespace TallerBicicleta.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// POST: api/auth/login
        /// Autentica un usuario
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var usuario = await _authService.LoginAsync(request.NombreUsuario, request.Password);

                if (usuario == null)
                {
                    return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
                }

                // En producción, aquí generarías un JWT token
                var response = new LoginResponse
                {
                    Success = true,
                    Message = "Login exitoso",
                    Usuario = usuario
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/auth/registro
        /// Registra un nuevo usuario
        /// </summary>
        [HttpPost("registro")]
        public async Task<ActionResult<Usuario>> Registro([FromBody] Usuario usuario)
        {
            try
            {
                var nuevoUsuario = await _authService.RegistrarUsuarioAsync(usuario);
                return CreatedAtAction(nameof(ObtenerUsuario), new { id = nuevoUsuario.Id }, nuevoUsuario);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/auth/usuario/{id}
        /// Obtiene un usuario por ID
        /// </summary>
        [HttpGet("usuario/{id}")]
        public async Task<ActionResult<Usuario>> ObtenerUsuario(string id)
        {
            try
            {
                var usuario = await _authService.ObtenerUsuarioPorIdAsync(id);
                if (usuario == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario {id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/auth/logout
        /// Cierra sesión (placeholder para futuro JWT)
        /// </summary>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "Sesión cerrada correctamente" });
        }
    }

    /// <summary>
    /// DTO para solicitud de login
    /// </summary>
    public class LoginRequest
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para respuesta de login
    /// </summary>
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Usuario? Usuario { get; set; }
        public string? Token { get; set; } // Para futuro JWT
    }
}