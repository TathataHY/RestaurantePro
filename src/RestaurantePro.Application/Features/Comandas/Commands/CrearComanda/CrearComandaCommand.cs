using MediatR;
using System.Collections.Generic;

namespace RestaurantePro.Application.Features.Comandas.Commands.CrearComanda
{
    public class CrearComandaCommand : IRequest<int>
    {
        public int MesaId { get; set; }
        public string Notas { get; set; }
        public List<CrearComandaDetalleDto> Detalles { get; set; } = new List<CrearComandaDetalleDto>();
    }

    public class CrearComandaDetalleDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public string NotasEspeciales { get; set; }
        public List<CrearComandaDetallePersonalizacionDto> Personalizaciones { get; set; } = new List<CrearComandaDetallePersonalizacionDto>();
    }

    public class CrearComandaDetallePersonalizacionDto
    {
        public int IngredienteId { get; set; }
        public int AccionPersonalizacion { get; set; }
        public decimal Cantidad { get; set; }
    }
} 