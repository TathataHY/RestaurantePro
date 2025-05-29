using RestaurantePro.Application.Inventario.Ingredientes.DTOs;
using RestaurantePro.Application.Inventario.Ingredientes.Commands.CrearIngrediente;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Entities;

namespace RestaurantePro.Application.Config.Mappings;

/// <summary>
/// Profile de AutoMapper para el contexto Inventario
/// Configura los mapeos entre entidades de dominio y DTOs para Ingredientes y Movimientos
/// </summary>
public class InventarioMappingProfile : Profile
{
    public InventarioMappingProfile()
    {
        ConfigurarMapeosIngredientes();
        ConfigurarMapeosMovimientosInventario();
        // TODO: Agregar otros mapeos cuando estén implementados
        // ConfigurarMapeosOrdenesCompra();
    }

    /// <summary>
    /// Configura los mapeos específicos para Ingredientes
    /// </summary>
    private void ConfigurarMapeosIngredientes()
    {
        // Ingrediente Entity -> IngredienteDto
        CreateMap<Ingrediente, IngredienteDto>()
            .ForMember(dest => dest.CategoriaTexto, opt => opt.MapFrom(src => src.Categoria.ToString()))
            .ForMember(dest => dest.UnidadMedidaTexto, opt => opt.MapFrom(src => src.UnidadMedida.ToString()))
            .ForMember(dest => dest.NombreProveedorPrincipal, opt => opt.MapFrom(src => src.ProveedorPrincipal != null ? src.ProveedorPrincipal.Nombre : null))
            .ForMember(dest => dest.StockActual, opt => opt.MapFrom(src => src.Stock))
            .ForMember(dest => dest.Rotacion, opt => opt.MapFrom(src => src.Rotacion.ToString()))
            .ForMember(dest => dest.Temporada, opt => opt.MapFrom(src => src.Temporada.ToString()))
            .ForMember(dest => dest.MovimientosRecientes, opt => opt.Ignore()) // Se llena manualmente en el handler
            // Propiedades calculadas se inicializan en el handler
            .ForMember(dest => dest.EstadoStock, opt => opt.Ignore())
            .ForMember(dest => dest.ColorEstado, opt => opt.Ignore())
            .ForMember(dest => dest.PorcentajeStock, opt => opt.Ignore())
            .ForMember(dest => dest.EstaBajoMinimo, opt => opt.Ignore())
            .ForMember(dest => dest.DiasEstimadosDuracion, opt => opt.Ignore())
            .ForMember(dest => dest.ValorTotalStock, opt => opt.Ignore())
            .ForMember(dest => dest.ResumenEstado, opt => opt.Ignore())
            // Mapear campos de auditoría de BaseDto
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.EstaActivo));

        // Ingrediente Entity -> IngredienteSummaryDto
        CreateMap<Ingrediente, IngredienteSummaryDto>()
            .ForMember(dest => dest.UnidadMedida, opt => opt.MapFrom(src => src.UnidadMedida.ToString()))
            .ForMember(dest => dest.StockActual, opt => opt.MapFrom(src => src.Stock))
            .ForMember(dest => dest.Rotacion, opt => opt.MapFrom(src => src.Rotacion.ToString()))
            .ForMember(dest => dest.EstaActivo, opt => opt.MapFrom(src => src.EstaActivo))
            // Propiedades calculadas se llenan en el handler
            .ForMember(dest => dest.EstadoStock, opt => opt.Ignore())
            .ForMember(dest => dest.ColorEstado, opt => opt.Ignore())
            .ForMember(dest => dest.PorcentajeStock, opt => opt.Ignore())
            .ForMember(dest => dest.EstaBajoMinimo, opt => opt.Ignore())
            .ForMember(dest => dest.ValorTotalStock, opt => opt.Ignore())
            .ForMember(dest => dest.StockTexto, opt => opt.Ignore())
            .ForMember(dest => dest.ResumenEstado, opt => opt.Ignore())
            .ForMember(dest => dest.PrioridadReposicion, opt => opt.Ignore())
            .ForMember(dest => dest.ColorPrioridad, opt => opt.Ignore());

        // IngredienteCreateDto -> CrearIngredienteCommand
        CreateMap<IngredienteCreateDto, CrearIngredienteCommand>()
            .ForMember(dest => dest.StockInicial, opt => opt.MapFrom(src => src.StockInicial))
            .ForMember(dest => dest.CostoInicial, opt => opt.MapFrom(src => src.CostoInicial))
            .ForMember(dest => dest.UsuarioId, opt => opt.Ignore()) // Se asigna desde el contexto del usuario
            .ForMember(dest => dest.MotivoStockInicial, opt => opt.MapFrom(src => "Stock inicial al crear ingrediente"));

        CreateMap<IngredienteUpdateDto, ActualizarIngredienteCommand>();
    }

    /// <summary>
    /// Configura los mapeos específicos para MovimientosInventario
    /// </summary>
    private void ConfigurarMapeosMovimientosInventario()
    {
        // MovimientoInventario Entity -> MovimientoInventarioDto
        CreateMap<MovimientoInventario, MovimientoInventarioDto>()
            .ForMember(dest => dest.TipoMovimientoTexto, opt => opt.MapFrom(src => src.TipoMovimiento.ToString()))
            .ForMember(dest => dest.NombreIngrediente, opt => opt.MapFrom(src => src.Ingrediente != null ? src.Ingrediente.Nombre : string.Empty))
            .ForMember(dest => dest.NombreUsuario, opt => opt.MapFrom(src => src.Usuario != null ? src.Usuario.Nombre : string.Empty))
            .ForMember(dest => dest.StockResultante, opt => opt.MapFrom(src => src.CantidadFinal))
            .ForMember(dest => dest.UsuarioMovimiento, opt => opt.MapFrom(src => "Sistema")) // Temporal hasta que esté implementado
            // Propiedades de UI se llenan en el handler
            .ForMember(dest => dest.ColorTipo, opt => opt.Ignore())
            .ForMember(dest => dest.IconoTipo, opt => opt.Ignore())
            .ForMember(dest => dest.FechaTexto, opt => opt.Ignore())
            .ForMember(dest => dest.CantidadTexto, opt => opt.Ignore());
    }

    // TODO: Implementar mapeos para OrdenesCompra y DetallesOrdenCompra
} 