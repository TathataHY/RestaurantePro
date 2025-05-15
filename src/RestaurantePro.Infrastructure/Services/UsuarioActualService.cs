using Microsoft.AspNetCore.Http;
using RestaurantePro.Application.Common.Interfaces;
using System;
using System.Security.Claims;

namespace RestaurantePro.Infrastructure.Services
{
    public class UsuarioActualService : IUsuarioActualService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsuarioActualService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetUsuarioId()
        {
            var usuarioId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(usuarioId))
            {
                return 0;
            }

            // Intentar convertir el ID de usuario (que es un string en Identity) a int
            if (int.TryParse(usuarioId, out int id))
            {
                return id;
            }
            
            return 0;
        }

        public string GetUsuarioEmail()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
        }

        public string GetUsuarioRol()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
        }

        public bool EsUsuarioAutenticado()
        {
            return _httpContextAccessor.HttpContext?.User.Identity.IsAuthenticated ?? false;
        }
    }
} 