using AutoMapper;
using RestaurantePro.Application.Comercial.Clientes.DTOs;
using RestaurantePro.Domain.Comercial.Clientes.Entities;

namespace RestaurantePro.Application.Config.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapeos para Cliente
            CreateMap<Cliente, ClienteDto>();
            
            // Aquí se agregarán más mapeos a medida que se añadan nuevos DTOs
        }
    }
} 