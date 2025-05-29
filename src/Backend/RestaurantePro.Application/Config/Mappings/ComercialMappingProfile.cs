using RestaurantePro.Application.Comercial.Clientes.DTOs;
using RestaurantePro.Application.Comercial.Clientes.Commands.CrearCliente;

namespace RestaurantePro.Application.Config.Mappings;

/// <summary>
/// Perfil de AutoMapper para el contexto Comercial
/// Configura los mapeos entre entidades de dominio y DTOs del contexto comercial
/// </summary>
public class ComercialMappingProfile : Profile
{
    public ComercialMappingProfile()
    {
        ConfigurarMapeosCliente();
        // TODO: Agregar otros mapeos cuando estén implementados
        // ConfigurarMapeosTarjetaFidelizacion();
        // ConfigurarMapeosFactura();
    }

    /// <summary>
    /// Configura los mapeos para la entidad Cliente
    /// </summary>
    private void ConfigurarMapeosCliente()
    {
        // Cliente → ClienteDto (mapeo completo con cálculos)
        CreateMap<Cliente, ClienteDto>()
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre.NombreCompleto))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Telefono.Value))
            .ForMember(dest => dest.Edad, opt => opt.MapFrom(src => src.CalcularEdad()))
            .ForMember(dest => dest.Segmento, opt => opt.MapFrom(src => src.Segmento.ToString()))
            .ForMember(dest => dest.TieneTarjetaFidelizacion, opt => opt.MapFrom(src => src.TieneTarjetaFidelizacion()))
            .ForMember(dest => dest.EsClienteFrecuente, opt => opt.MapFrom(src => src.CantidadVisitas > 10))
            .ForMember(dest => dest.UltimaActividad, opt => opt.MapFrom(src => src.FechaActualizacion ?? src.FechaCreacion))
            // Propiedades de BaseDto
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.EstaActivo))
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => src.FechaCreacion))
            .ForMember(dest => dest.FechaModificacion, opt => opt.MapFrom(src => src.FechaActualizacion));

        // Cliente → ClienteSummaryDto (mapeo resumido)
        CreateMap<Cliente, ClienteSummaryDto>()
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre.NombreCompleto))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Telefono.Value))
            .ForMember(dest => dest.Edad, opt => opt.MapFrom(src => src.CalcularEdad()))
            .ForMember(dest => dest.Segmento, opt => opt.MapFrom(src => src.Segmento.ToString()))
            .ForMember(dest => dest.EsClienteFrecuente, opt => opt.MapFrom(src => src.CantidadVisitas > 10))
            .ForMember(dest => dest.UltimaActividad, opt => opt.MapFrom(src => src.FechaActualizacion ?? src.FechaCreacion))
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => src.FechaCreacion));

        // ClienteCreateDto → CrearClienteCommand (DTO de entrada a comando)
        CreateMap<ClienteCreateDto, CrearClienteCommand>();

        // ClienteUpdateDto → ActualizarClienteCommand (cuando esté implementado)
        // CreateMap<ClienteUpdateDto, ActualizarClienteCommand>();
    }
} 