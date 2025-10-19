namespace TallerBicicleta.Services
{
     /// <summary>
    /// Servicio de lógica de negocio para la entidad Empleado
    /// Maneja operaciones CRUD con Firebase Firestore
    /// </summary>
    public class EmpleadoService
    {
        private readonly FirebaseService _firebaseService;
        private readonly CollectionReference _empleadosCollection;
        private const string COLECCION_EMPLEADOS = "empleados";

        public EmpleadoService(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
            _empleadosCollection = _firebaseService.GetCollection(COLECCION_EMPLEADOS);
        }

        /// <summary>
        /// Registra un nuevo empleado en Firestore
        /// </summary>
        /// <param name="empleado">Objeto empleado a registrar</param>
        /// <returns>Resultado de la operación</returns>
        public async Task<(bool Success, string Message, string? Id)> RegistrarEmpleadoAsync(Empleado empleado)
        {
            try
            {
                // Validaciones de campos obligatorios
                if (string.IsNullOrWhiteSpace(empleado.NombreUsuario))
                    return (false, "El nombre de usuario es obligatorio.", null);

                if (string.IsNullOrWhiteSpace(empleado.Password))
                    return (false, "La contraseña es obligatoria.", null);

                if (string.IsNullOrWhiteSpace(empleado.CorreoElectronico))
                    return (false, "El correo electrónico es obligatorio.", null);

                if (string.IsNullOrWhiteSpace(empleado.NombreCompleto))
                    return (false, "El nombre completo es obligatorio.", null);

                if (empleado.PorcentajeComision < 0 || empleado.PorcentajeComision > 1)
                    return (false, "El porcentaje de comisión debe estar entre 0 y 1.", null);

                // Validar que no exista un empleado con el mismo NombreUsuario
                if (await ExisteEmpleadoPorNombreUsuarioAsync(empleado.NombreUsuario))
                    return (false, "Ya existe un empleado con ese nombre de usuario.", null);

                // Validar que no exista un empleado con el mismo correo electrónico
                if (await ExisteEmpleadoPorCorreoAsync(empleado.CorreoElectronico))
                    return (false, "El correo electrónico ya está en uso.", null);

                // Hashear la contraseña antes de guardarla
                empleado.Password = HashPassword(empleado.Password);

                // Establecer valores por defecto
                empleado.FechaCreacion = DateTime.UtcNow;
                empleado.Activo = true;
                empleado.RolUsuario = RolUsuario.Empleado;

                // Agregar a Firestore
                DocumentReference docRef = await _empleadosCollection.AddAsync(empleado);
                empleado.Id = docRef.Id;

                // Actualizar el documento con el ID
                await docRef.SetAsync(empleado, SetOptions.MergeAll);

                return (true, "Empleado registrado exitosamente.", docRef.Id);
            }
            catch (Exception ex)
            {
                return (false, $"Error al registrar el empleado: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Obtiene todos los empleados activos de Firestore
        /// </summary>
        /// <returns>Lista de empleados</returns>
        public async Task<List<Empleado>> ObtenerEmpleadosAsync()
        {
            try
            {
                Query query = _empleadosCollection.WhereEqualTo("Activo", true);
                QuerySnapshot snapshot = await query.GetSnapshotAsync();

                List<Empleado> empleados = [];
                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    if (document.Exists)
                    {
                        Empleado empleado = document.ConvertTo<Empleado>();
                        empleado.Id = document.Id;
                        empleados.Add(empleado);
                    }
                }

                return [.. empleados.OrderBy(e => e.NombreCompleto)];
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener empleados: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene un empleado específico por su ID
        /// </summary>
        /// <param name="id">ID del empleado</param>
        /// <returns>Empleado encontrado o null</returns>
        public async Task<Empleado?> ObtenerEmpleadoPorIdAsync(string id)
        {
            try
            {
                DocumentReference docRef = _empleadosCollection.Document(id);
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

                if (snapshot.Exists)
                {
                    Empleado empleado = snapshot.ConvertTo<Empleado>();
                    empleado.Id = snapshot.Id;
                    return empleado;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener el empleado: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Actualiza un empleado existente en Firestore
        /// </summary>
        /// <param name="empleado">Objeto empleado con los datos actualizados</param>
        /// <returns>Resultado de la operación</returns>
        public async Task<(bool Success, string Message)> ActualizarEmpleadoAsync(Empleado empleado)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(empleado.Id))
                    return (false, "El ID del empleado es requerido.");

                // Validaciones de campos obligatorios
                if (string.IsNullOrWhiteSpace(empleado.NombreCompleto))
                    return (false, "El nombre completo es obligatorio.");

                if (string.IsNullOrWhiteSpace(empleado.CorreoElectronico))
                    return (false, "El correo electrónico es obligatorio.");

                if (empleado.PorcentajeComision < 0 || empleado.PorcentajeComision > 1)
                    return (false, "El porcentaje de comisión debe estar entre 0 y 1.");

                // Verificar que el empleado existe
                Empleado? empleadoExistente = await ObtenerEmpleadoPorIdAsync(empleado.Id);
                if (empleadoExistente == null)
                    return (false, "El empleado no existe.");

                // Validar que no exista otro empleado con el mismo correo (excepto el mismo)
                var empleadoConMismoCorreo = await BuscarEmpleadoPorCorreoAsync(empleado.CorreoElectronico);
                if (empleadoConMismoCorreo != null && empleadoConMismoCorreo.Id != empleado.Id)
                    return (false, "El correo electrónico ya está en uso por otro empleado.");

                // Si se está actualizando la contraseña y no está hasheada, hashearla
                // (La contraseña hasheada tiene 64 caracteres)
                if (!string.IsNullOrEmpty(empleado.Password) && empleado.Password.Length != 64)
                {
                    empleado.Password = HashPassword(empleado.Password);
                }

                // Actualizar fecha de modificación
                empleado.FechaModificacion = DateTime.UtcNow;

                // Actualizar en Firestore
                DocumentReference docRef = _empleadosCollection.Document(empleado.Id);
                await docRef.SetAsync(empleado, SetOptions.MergeAll);

                return (true, "Empleado actualizado exitosamente.");
            }
            catch (Exception ex)
            {
                return (false, $"Error al actualizar el empleado: {ex.Message}");
            }
        }

        /// <summary>
        /// Elimina (marca como inactivo) un empleado en Firestore
        /// </summary>
        /// <param name="id">ID del empleado a eliminar</param>
        /// <returns>Resultado de la operación</returns>
        public async Task<(bool Success, string Message)> EliminarEmpleadoAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return (false, "El ID del empleado es requerido.");

                // Verificar que el empleado existe
                Empleado? empleado = await ObtenerEmpleadoPorIdAsync(id);
                if (empleado == null)
                    return (false, "El empleado no existe.");

                // Marcar como inactivo en lugar de eliminar
                DocumentReference docRef = _empleadosCollection.Document(id);
                await docRef.UpdateAsync(new Dictionary<string, object>
                {
                    { "Activo", false },
                    { "FechaModificacion", DateTime.UtcNow }
                });

                return (true, "Empleado eliminado exitosamente.");
            }
            catch (Exception ex)
            {
                return (false, $"Error al eliminar el empleado: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica si existe un empleado con el nombre de usuario especificado
        /// </summary>
        private async Task<bool> ExisteEmpleadoPorNombreUsuarioAsync(string nombreUsuario)
        {
            try
            {
                Query query = _empleadosCollection.WhereEqualTo("NombreUsuario", nombreUsuario);
                QuerySnapshot snapshot = await query.GetSnapshotAsync();
                return snapshot.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Verifica si existe un empleado con el correo electrónico especificado
        /// </summary>
        private async Task<bool> ExisteEmpleadoPorCorreoAsync(string correo)
        {
            try
            {
                Query query = _empleadosCollection.WhereEqualTo("CorreoElectronico", correo);
                QuerySnapshot snapshot = await query.GetSnapshotAsync();
                return snapshot.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Busca un empleado por correo electrónico
        /// </summary>
        private async Task<Empleado?> BuscarEmpleadoPorCorreoAsync(string correo)
        {
            try
            {
                Query query = _empleadosCollection.WhereEqualTo("CorreoElectronico", correo);
                QuerySnapshot snapshot = await query.GetSnapshotAsync();

                if (snapshot.Count > 0)
                {
                    DocumentSnapshot document = snapshot.Documents[0];
                    Empleado empleado = document.ConvertTo<Empleado>();
                    empleado.Id = document.Id;
                    return empleado;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Hash de contraseña usando SHA256
        /// </summary>
        private static string HashPassword(string password)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}