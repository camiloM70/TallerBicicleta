using System.Security.Claims;

namespace TallerBicicleta.Services
{
    /// <summary>
    /// Proveedor de estado de autenticación personalizado
    /// Maneja el estado de autenticación del usuario en Blazor Server
    /// </summary>
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(_currentUser));
        }

        /// <summary>
        /// Marca al usuario como autenticado
        /// </summary>
        public void MarkUserAsAuthenticated(Usuario usuario)
        {
            var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, usuario.Id ?? string.Empty),
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim(ClaimTypes.Email, usuario.CorreoElectronico),
                new Claim(ClaimTypes.Role, usuario.RolUsuario.ToString()),
                new Claim("RolId", usuario.Rol.ToString())
            ], "CustomAuth");

            _currentUser = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }

        /// <summary>
        /// Marca al usuario como no autenticado
        /// </summary>
        public void MarkUserAsLoggedOut()
        {
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }

        /// <summary>
        /// Obtiene el usuario actual
        /// </summary>
        public ClaimsPrincipal GetCurrentUser()
        {
            return _currentUser;
        }
    }
}
