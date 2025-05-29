using RestaurantePro.Application.Comercial.Clientes.DTOs;
using RestaurantePro.Application.Comercial.Clientes.Commands.CrearCliente;
using RestaurantePro.Application.Comercial.Clientes.Commands.ActualizarCliente;
using RestaurantePro.Domain.Comercial.Clientes.Entities;

namespace RestaurantePro.Application.Config.Mappings;

/// <summary>
/// Profile de AutoMapper para el contexto Comercial
/// Configura los mapeos entre entidades de dominio y DTOs para Clientes
/// </summary>
public class ComercialMappingProfile : Profile
{
    public ComercialMappingProfile()
    {
        ConfigurarMapeosClientes();
        // TODO: Agregar otros mapeos cuando estén implementados
        // ConfigurarMapeosTarjetaFidelizacion();
        // ConfigurarMapeosFactura();
    }

    /// <summary>
    /// Configura los mapeos específicos para Clientes
    /// </summary>
    private void ConfigurarMapeosClientes()
    {
        // Cliente Entity -> ClienteDto
        CreateMap<Cliente, ClienteDto>()
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre.NombreCompleto))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Telefono.Value))
            .ForMember(dest => dest.Edad, opt => opt.MapFrom(src => src.CalcularEdad()))
            .ForMember(dest => dest.Segmento, opt => opt.MapFrom(src => src.Segmento.ToString()))
            .ForMember(dest => dest.TieneTarjetaFidelizacion, opt => opt.MapFrom(src => src.TarjetaFidelizacionPrincipalId.HasValue))
            .ForMember(dest => dest.EsClienteFrecuente, opt => opt.MapFrom(src => src.CantidadVisitas > 10))
            .ForMember(dest => dest.UltimaActividad, opt => opt.MapFrom(src => src.FechaActualizacion ?? src.FechaCreacion))
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.EstaActivo))
            .ForMember(dest => dest.TipoTexto, opt => opt.MapFrom(src => src.Tipo.ToString()))
            .ForMember(dest => dest.NivelFidelizacionTexto, opt => opt.MapFrom(src => src.TarjetaFidelizacion != null ? src.TarjetaFidelizacion.Nivel.ToString() : "Sin Tarjeta"))
            .ForMember(dest => dest.PuntosFidelizacion, opt => opt.MapFrom(src => src.TarjetaFidelizacion != null ? src.TarjetaFidelizacion.PuntosAcumulados : 0))
            .ForMember(dest => dest.TarjetaFidelizacionId, opt => opt.MapFrom(src => src.TarjetaFidelizacion != null ? src.TarjetaFidelizacion.Id : (Guid?)null));

        // Cliente Entity -> ClienteSummaryDto
        CreateMap<Cliente, ClienteSummaryDto>()
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre.NombreCompleto))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Telefono.Value))
            .ForMember(dest => dest.Segmento, opt => opt.MapFrom(src => src.Segmento.ToString()))
            .ForMember(dest => dest.TieneTarjetaFidelizacion, opt => opt.MapFrom(src => src.TarjetaFidelizacionPrincipalId.HasValue))
            .ForMember(dest => dest.EsClienteFrecuente, opt => opt.MapFrom(src => src.CantidadVisitas > 10));

        // ClienteCreateDto -> CrearClienteCommand
        CreateMap<ClienteCreateDto, CrearClienteCommand>();

        // ClienteUpdateDto -> ActualizarClienteCommand
        CreateMap<ClienteUpdateDto, ActualizarClienteCommand>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()); // El ID viene por separado

        // TarjetaFidelizacion mappings
        CreateMap<TarjetaFidelizacion, TarjetaFidelizacionDto>()
            .ForMember(dest => dest.NivelTexto, opt => opt.MapFrom(src => src.Nivel.ToString()))
            .ForMember(dest => dest.NombreCliente, opt => opt.MapFrom(src => src.Cliente != null ? $"{src.Cliente.Nombre} {src.Cliente.Apellido}".Trim() : string.Empty));

        // Factura mappings
        CreateMap<Factura, FacturaDto>()
            .ForMember(dest => dest.EstadoTexto, opt => opt.MapFrom(src => src.Estado.ToString()))
            .ForMember(dest => dest.TipoTexto, opt => opt.MapFrom(src => src.Tipo.ToString()))
            .ForMember(dest => dest.NombreCliente, opt => opt.MapFrom(src => src.Cliente != null ? $"{src.Cliente.Nombre} {src.Cliente.Apellido}".Trim() : "Cliente Anónimo"));
    }
} 