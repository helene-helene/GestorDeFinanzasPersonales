using System.Security.Claims;

namespace GestorFinanzas.Web.Services;

// Obtiene el Id del usuario que inició sesión leyendo el claim NameIdentifier de la cookie.
// El Id nunca se toma de la URL ni de un formulario, por eso nadie puede hacerse pasar por otro usuario.
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UsuarioId
    {
        get
        {
            var usuario = _httpContextAccessor.HttpContext?.User;
            var valor = usuario?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (usuario?.Identity?.IsAuthenticated != true || !int.TryParse(valor, out var id))
            {
                // Solo pasa si alguien llama a un servicio privado sin sesión iniciada.
                throw new UnauthorizedAccessException("No hay un usuario autenticado.");
            }

            return id;
        }
    }
}
