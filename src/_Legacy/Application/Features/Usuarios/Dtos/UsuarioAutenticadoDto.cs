namespace RestaurantePro.Application.Features.Usuarios.Dtos
{
    public class UsuarioAutenticadoDto
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; }
        public string Token { get; set; }
    }
} 