using AutoMapper;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;

namespace RestaurantePro.Application.Common.Mappings
{
    /// <summary>
    /// Perfil de mapeo para Comandas e Items de Comanda
    /// </summary>
    public class ComandaMappingProfile : Profile
    {
        public ComandaMappingProfile()
        {
            // Mapeo de Comanda a ComandaDto
            CreateMap<Comanda, ComandaDto>()
                .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Total != null ? src.Total.Subtotal : 0))
                .ForMember(dest => dest.Impuestos, opt => opt.MapFrom(src => src.Total != null ? src.Total.Impuestos : 0))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total != null ? src.Total.Total : 0))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.NumeroMesa, opt => opt.Ignore())
                .ForMember(dest => dest.NombreMesero, opt => opt.Ignore())
                .ForMember(dest => dest.NombreCliente, opt => opt.Ignore());

            // Mapeo de ItemComanda a ItemComandaDto
            CreateMap<ItemComanda, ItemComandaDto>()
                .ForMember(dest => dest.Personalizaciones, opt => opt.MapFrom(src => src.Personalizaciones))
                .ForMember(dest => dest.NombreProducto, opt => opt.Ignore())
                .ForMember(dest => dest.Ingredientes, opt => opt.Ignore());

            // Mapeo de PersonalizacionItem a PersonalizacionItemDto
            CreateMap<PersonalizacionItem, PersonalizacionItemDto>()
                .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.Tipo.ToString()));
        }
    }
} 