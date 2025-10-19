namespace TallerBicicleta.Services
{
    public class AuthService
    {
        private readonly FirebaseService _firebaseService;
        private readonly ILogger<AuthService> _logger;
        private const string COLLECTION_NAME = "usuarios";

        public AuthService(FirebaseService firebaseService, ILogger<AuthService> logger)
        {
            _firebaseService = firebaseService;
            _logger = logger;
        }

        /// <summary>
        /// Autentica un usuario con nombre de usuario y contraseña
        /// Busca en usuarios y empleados
        /// </summary>
        public async Task<Usuario?> LoginAsync(string nombreUsuario, string password)
        {
            try
            {
                Usuario? usuario = null;
                DocumentSnapshot? doc = null;

                // Primero buscar en usuarios
                var usuariosCollection = _firebaseService.GetCollection(COLLECTION_NAME);
                var queryUsuarios = usuariosCollection.WhereEqualTo("NombreUsuario", nombreUsuario);
                var snapshotUsuarios = await queryUsuarios.GetSnapshotAsync();

                if (snapshotUsuarios.Count > 0)
                {
                    doc = snapshotUsuarios.Documents[0];
                    usuario = doc.ConvertTo<Usuario>();
                    usuario.Id = doc.Id;
                }
                else
                {
                    // Si no está en usuarios, buscar en empleados
                    var empleadosCollection = _firebaseService.GetCollection("empleados");
                    var queryEmpleados = empleadosCollection.WhereEqualTo("NombreUsuario", nombreUsuario);
                    var snapshotEmpleados = await queryEmpleados.GetSnapshotAsync();

                    if (snapshotEmpleados.Count > 0)
                    {
                        doc = snapshotEmpleados.Documents[0];
                        var empleado = doc.ConvertTo<Empleado>();

                        // Convertir Empleado a Usuario para el login
                        usuario = new Usuario
                        {
                            Id = doc.Id,
                            NombreUsuario = empleado.NombreUsuario,
                            Password = empleado.Password,
                            CorreoElectronico = empleado.CorreoElectronico,
                            Rol = empleado.Rol,
                            NombreCompleto = empleado.NombreCompleto
                        };
                    }
                }

                // Verificar si se encontró el usuario
                if (usuario == null)
                {
                    _logger.LogWarning("Intento de login fallido: usuario '{nombreUsuario}' no encontrado", nombreUsuario);
                    return null;
                }

                // Verificar password
                if (usuario.Password != HashPassword(password))
                {
                    _logger.LogWarning("Intento de login fallido: contraseña incorrecta para '{NombreUsuario}'", nombreUsuario);
                    return null;
                }

                _logger.LogInformation("Login exitoso: {NombreUsuario} - Rol: {RolUsuario}", nombreUsuario, usuario.RolUsuario);
                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login");
                throw new Exception($"Error en login: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Registra un nuevo usuario
        /// </summary>
        public async Task<Usuario> RegistrarUsuarioAsync(Usuario usuario)
        {
            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(usuario.NombreUsuario))
                    throw new ArgumentException("El nombre de usuario es requerido");

                if (string.IsNullOrWhiteSpace(usuario.Password))
                    throw new ArgumentException("La contraseña es requerida");

                if (string.IsNullOrWhiteSpace(usuario.CorreoElectronico))
                    throw new ArgumentException("El correo electrónico es requerido");

                // Verificar que el nombre de usuario no exista
                var collection = _firebaseService.GetCollection(COLLECTION_NAME);
                var queryUsuario = collection.WhereEqualTo("NombreUsuario", usuario.NombreUsuario);
                var snapshotUsuario = await queryUsuario.GetSnapshotAsync();

                if (snapshotUsuario.Count > 0)
                    throw new Exception("El nombre de usuario ya existe");

                // Verificar que el correo no exista
                var queryCorreo = collection.WhereEqualTo("CorreoElectronico", usuario.CorreoElectronico);
                var snapshotCorreo = await queryCorreo.GetSnapshotAsync();

                if (snapshotCorreo.Count > 0)
                    throw new Exception("El correo electrónico ya está registrado");

                // Hash de la contraseña
                usuario.Password = HashPassword(usuario.Password);
                usuario.FechaCreacion = DateTime.UtcNow;

                // Guardar usuario
                var docRef = await collection.AddAsync(usuario);
                usuario.Id = docRef.Id;

                _logger.LogInformation("Usuario registrado: {usuario.NombreUsuario} - Rol: {usuario.RolUsuario}", usuario.NombreUsuario,usuario.RolUsuario);
                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario");
                throw new Exception($"Error al registrar usuario: {ex.Message}", ex);
            }
        }


        /// <summary>
        /// Obtiene un usuario por ID
        /// </summary>
        public async Task<Usuario?> ObtenerUsuarioPorIdAsync(string id)
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

                var usuario = snapshot.ConvertTo<Usuario>();
                usuario.Id = snapshot.Id;
                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario {id}",id);
                throw new Exception($"Error al obtener usuario: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Hash simple de contraseña usando SHA256
        /// En producción usar BCrypt o similar
        /// </summary>
        private static string HashPassword(string password)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }

}
