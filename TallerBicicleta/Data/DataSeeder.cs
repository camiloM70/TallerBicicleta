namespace TallerBicicleta.Data
{
    /// <summary>
    /// Servicio para inicializar datos por defecto en la aplicación
    /// </summary>
    public class DataSeeder(AuthService authService, ILogger<DataSeeder> logger)
    {
        private readonly AuthService _authService = authService;
        private readonly ILogger<DataSeeder> _logger = logger;

        /// <summary>
        /// Crea el usuario administrador por defecto si no existe
        /// Usuario: admin
        /// Password: 2345
        /// </summary>
        public async Task SeedAdminUserAsync()
        {
            try
            {
                // Intentar hacer login con el admin por defecto
                var adminExiste = await _authService.LoginAsync("admin", "2345");

                if (adminExiste == null)
                {
                    // El admin no existe, crearlo
                    var admin = new Usuario
                    {
                        NombreUsuario = "admin",
                        Password = "2345",
                        CorreoElectronico = "admin@proyectotaller.com",
                        Rol = (int)RolUsuario.Administrador,
                        FechaCreacion = DateTime.UtcNow
                    };

                    await _authService.RegistrarUsuarioAsync(admin);
                    _logger.LogInformation("Usuario administrador creado exitosamente: admin / 2345");
                }
                else
                {
                    _logger.LogInformation("Usuario administrador ya existe");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario administrador por defecto");
            }
        }
    }
}
