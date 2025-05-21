using AutoMapper;
using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;
using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;

namespace RestaurantePro.Application.Common.Mappings
{
    /// <summary>
    /// Perfil de mapeo para Reservaciones y Mesas
    /// </summary>
    public class ReservacionMappingProfile : Profile
    {
        public ReservacionMappingProfile()
        {
            // Mapeo de Reservacion a ReservacionDto
            CreateMap<Reservacion, ReservacionDto>()
                .ForMember(dest => dest.NombreCliente, opt => opt.Ignore())
                .ForMember(dest => dest.NumeroMesa, opt => opt.Ignore())
                .ForMember(dest => dest.TelefonoContacto, opt => opt.MapFrom(src => src.Telefono))
                .ForMember(dest => dest.EmailContacto, opt => opt.MapFrom(src => src.Email));

            // Mapeo de Mesa a MesaDto
            CreateMap<Mesa, MesaDto>()
                .ForMember(dest => dest.EstaReservada, opt => opt.MapFrom(src => src.Estado == Domain.Operaciones.Reservaciones.Mesas.Enums.EstadoMesa.Reservada))
                .ForMember(dest => dest.ComandaActivaId, opt => opt.Ignore())
                .ForMember(dest => dest.UltimaReservacion, opt => opt.Ignore());
        }
    }
} 