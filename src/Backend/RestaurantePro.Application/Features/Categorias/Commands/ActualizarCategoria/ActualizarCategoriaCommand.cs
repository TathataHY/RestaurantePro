using MediatR;

namespace RestaurantePro.Application.Features.Categorias.Commands.ActualizarCategoria
{
    public class ActualizarCategoriaCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string ImagenUrl { get; set; }
        public int Orden { get; set; }
    }
} 