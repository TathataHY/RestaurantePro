using AutoMapper;
using RestaurantePro.Core.DTOs.Comanda;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Enums;
using System;

namespace RestaurantePro.Infrastructure.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Comanda, ComandaDto>()
                .ForMember(d => d.MesaNumero,
                          opt => opt.MapFrom(s => s.Mesa.Numero));

            CreateMap<ComandaCreateDto, Comanda>()
                .ForMember(d => d.FechaHora,
                          opt => opt.MapFrom(s => DateTime.UtcNow))
                .ForMember(d => d.Estado,
                          opt => opt.MapFrom(s => EstadoComanda.Pendiente));

            CreateMap<ComandaDto, Comanda>();
        }
    }
}