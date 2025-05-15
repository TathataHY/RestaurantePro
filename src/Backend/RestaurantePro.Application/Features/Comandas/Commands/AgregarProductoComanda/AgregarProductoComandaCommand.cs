using MediatR;
using System.Collections.Generic;

namespace RestaurantePro.Application.Features.Comandas.Commands.AgregarProductoComanda
{
    public class AgregarProductoComandaCommand : IRequest<bool>
    {
        public int ComandaId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public string NotasEspeciales { get; set; }
        public List<AgregarPersonalizacionDto> Personalizaciones { get; set; } = new List<AgregarPersonalizacionDto>();
    }

    public class AgregarPersonalizacionDto
    {
        public int IngredienteId { get; set; }
        public int AccionPersonalizacion { get; set; }
        public decimal Cantidad { get; set; }
    }
} 