using AutoMapper;
using RestaurantePro.Core.DTOs.Comanda;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Enums;

namespace RestaurantePro.Tests.Mapping
{
    public class ComandaMappingProfile : Profile
    {
        public ComandaMappingProfile()
        {
            CreateMap<ComandaCreateDto, Comanda>()
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(_ => EstadoComanda.Pendiente))
                .ForMember(dest => dest.FechaHora, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Detalles));

            CreateMap<ComandaDetalleCreateDto, ComandaDetalle>();
            CreateMap<ComandaDetalle, ComandaDetalleDto>();
            CreateMap<Comanda, ComandaDto>();
        }
    }
} 