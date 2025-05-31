using RestaurantePro.Application.Comercial.Clientes.DTOs;
using RestaurantePro.Application.Comercial.Clientes.Commands.CrearCliente;
using RestaurantePro.Application.Comercial.Clientes.Commands.ActualizarCliente;

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
            .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Telefono.Value));

        // Cliente Entity -> ClienteSummaryDto
        CreateMap<Cliente, ClienteSummaryDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Telefono.Value));

        // ClienteCreateDto -> CrearClienteCommand
        CreateMap<ClienteCreateDto, CrearClienteCommand>();

        // ClienteUpdateDto -> ActualizarClienteCommand
        CreateMap<ClienteUpdateDto, ActualizarClienteCommand>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()); // El ID viene por separado

        // TODO: Reactivar cuando existan estos DTOs y entidades en Domain
        // TarjetaFidelizacion mappings
        //CreateMap<TarjetaFidelizacion, TarjetaFidelizacionDto>()
        //    .ForMember(dest => dest.NivelTexto, opt => opt.MapFrom(src => src.Nivel.ToString()))
        //    .ForMember(dest => dest.NombreCliente, opt => opt.MapFrom(src => src.Cliente != null ? $"{src.Cliente.Nombre} {src.Cliente.Apellido}".Trim() : string.Empty));

        // Factura mappings
        //CreateMap<Factura, FacturaDto>()
        //    .ForMember(dest => dest.EstadoTexto, opt => opt.MapFrom(src => src.Estado.ToString()))
        //    .ForMember(dest => dest.TipoTexto, opt => opt.MapFrom(src => src.Tipo.ToString()))
        //    .ForMember(dest => dest.NombreCliente, opt => opt.MapFrom(src => src.Cliente != null ? $"{src.Cliente.Nombre} {src.Cliente.Apellido}".Trim() : "Cliente Anónimo"));
    }
} 