using MediatR;

namespace RestaurantePro.Application.Features.Categorias.Commands.CrearCategoria
{
    public class CrearCategoriaCommand : IRequest<int>
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string ImagenUrl { get; set; }
        public int Orden { get; set; }
    }
} 