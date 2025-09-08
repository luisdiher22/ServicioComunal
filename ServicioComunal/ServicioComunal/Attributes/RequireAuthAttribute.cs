using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ServicioComunal.Attributes
{
    /// <summary>
    /// Atributo de autorización que verifica si el usuario está autenticado
    /// y opcionalmente si tiene el rol requerido.
    /// </summary>
    public class RequireAuthAttribute : ActionFilterAttribute
    {
        private readonly string[]? _requiredRoles;

        /// <summary>
        /// Constructor para autorización básica (solo verificar autenticación)
        /// </summary>
        public RequireAuthAttribute()
        {
            _requiredRoles = null;
        }

        /// <summary>
        /// Constructor para autorización por rol específico
        /// </summary>
        /// <param name="requiredRoles">Roles que pueden acceder al recurso</param>
        public RequireAuthAttribute(params string[] requiredRoles)
        {
            _requiredRoles = requiredRoles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            
            // Verificar si el usuario está autenticado
            var usuarioIdentificacion = session.GetInt32("UsuarioIdentificacion");
            if (!usuarioIdentificacion.HasValue)
            {
                // Verificar si es una solicitud AJAX
                if (IsAjaxRequest(context.HttpContext.Request))
                {
                    context.Result = new JsonResult(new { success = false, message = "Sesión expirada. Por favor, inicia sesión nuevamente." })
                    {
                        StatusCode = 401
                    };
                    return;
                }
                
                // Usuario no autenticado - redirigir al login
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // Si se requieren roles específicos, verificarlos
            if (_requiredRoles != null && _requiredRoles.Length > 0)
            {
                var usuarioRol = session.GetString("UsuarioRol");
                if (string.IsNullOrEmpty(usuarioRol) || !_requiredRoles.Contains(usuarioRol))
                {
                    // Verificar si es una solicitud AJAX
                    if (IsAjaxRequest(context.HttpContext.Request))
                    {
                        context.Result = new JsonResult(new { success = false, message = $"Acceso denegado. Se requiere rol: {string.Join(", ", _requiredRoles)}" })
                        {
                            StatusCode = 403
                        };
                        return;
                    }
                    
                    // Usuario no tiene el rol requerido - mostrar acceso denegado
                    context.Result = new ViewResult
                    {
                        ViewName = "AccessDenied",
                        ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(
                            new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
                            new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary())
                        {
                            ["Message"] = $"Acceso denegado. Se requiere rol: {string.Join(", ", _requiredRoles)}"
                        }
                    };
                    return;
                }
            }

            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Determina si la solicitud es una solicitud AJAX
        /// </summary>
        private static bool IsAjaxRequest(HttpRequest request)
        {
            return request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                   request.Headers["Content-Type"].ToString().Contains("application/json") ||
                   request.Path.StartsWithSegments("/api");
        }
    }
}
