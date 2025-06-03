namespace RestaurantePro.Application.Config.Mappings;
using System.Reflection;

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
        ConfigurarMapeosOrdenesCompra();
    }

    /// <summary>
    /// Configura los mapeos específicos para Ingredientes
    /// </summary>
    private void ConfigurarMapeosIngredientes()
    {
        // Ingrediente Entity -> IngredienteDto
        CreateMap<Ingrediente, IngredienteDto>()
            .ForMember(dest => dest.UnidadMedida, opt => opt.MapFrom(src => src.UnidadMedida))
            .ForMember(dest => dest.UnidadMedidaTexto, opt => opt.MapFrom(src => MapearUnidadMedidaTexto(src.UnidadMedida)))
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => src.FechaCreacion))
            // Mapeo de propiedades básicas (AutoMapper las mapea automáticamente por nombre)
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
            .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.Descripcion ?? string.Empty))
            // Propiedades calculadas - las ignoramos y se asignan en el handler
            .ForMember(dest => dest.StockActual, opt => opt.Ignore())
            .ForMember(dest => dest.StockMaximo, opt => opt.Ignore())
            .ForMember(dest => dest.CostoUnitario, opt => opt.Ignore())
            .ForMember(dest => dest.NombreProveedorPrincipal, opt => opt.Ignore())
            .ForMember(dest => dest.Activo, opt => opt.Ignore())
            .ForMember(dest => dest.RequiereRefrigeracion, opt => opt.Ignore())
            .ForMember(dest => dest.DiasVencimiento, opt => opt.Ignore())
            .ForMember(dest => dest.EstadoStock, opt => opt.Ignore())
            .ForMember(dest => dest.ColorEstado, opt => opt.Ignore())
            .ForMember(dest => dest.EstaBajoMinimo, opt => opt.Ignore())
            .ForMember(dest => dest.DiasEstimadosDuracion, opt => opt.Ignore())
            .ForMember(dest => dest.ValorTotalStock, opt => opt.Ignore())
            .ForMember(dest => dest.ResumenEstado, opt => opt.Ignore())
            .ForMember(dest => dest.MovimientosRecientes, opt => opt.Ignore())
            .ForMember(dest => dest.FechaModificacion, opt => opt.Ignore())
            .ForMember(dest => dest.CreadoPor, opt => opt.Ignore())
            .ForMember(dest => dest.ModificadoPor, opt => opt.Ignore());

        // Ingrediente Entity -> IngredienteSummaryDto (para listas)
        CreateMap<Ingrediente, IngredienteSummaryDto>()
            .ForMember(dest => dest.FechaRegistro, opt => opt.MapFrom(src => src.FechaCreacion))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
            .ForMember(dest => dest.UnidadMedida, opt => opt.MapFrom(src => MapearUnidadMedidaTexto(src.UnidadMedida)))
            // Propiedades calculadas - las ignoramos y se asignan en el handler
            .ForMember(dest => dest.Categoria, opt => opt.Ignore())
            .ForMember(dest => dest.StockActual, opt => opt.Ignore())
            .ForMember(dest => dest.StockMaximo, opt => opt.Ignore())
            .ForMember(dest => dest.CostoUnitario, opt => opt.Ignore())
            .ForMember(dest => dest.Activo, opt => opt.Ignore())
            .ForMember(dest => dest.RegistradoPor, opt => opt.Ignore())
            .ForMember(dest => dest.FechaUltimoMovimiento, opt => opt.Ignore())
            .ForMember(dest => dest.TipoUltimoMovimiento, opt => opt.Ignore())
            .ForMember(dest => dest.FechaVencimiento, opt => opt.Ignore())
            .ForMember(dest => dest.TotalRecetas, opt => opt.Ignore())
            .ForMember(dest => dest.ConsumoPromedioMensual, opt => opt.Ignore());

        // IngredienteCreateDto -> CrearIngredienteCommand
        CreateMap<IngredienteCreateDto, CrearIngredienteCommand>()
            .ForMember(dest => dest.StockInicial, opt => opt.MapFrom(src => src.StockInicial))
            .ForMember(dest => dest.CostoInicial, opt => opt.MapFrom(src => src.CostoInicial))
            .ForMember(dest => dest.UsuarioId, opt => opt.Ignore()) // Se asigna desde el contexto del usuario
            .ForMember(dest => dest.MotivoStockInicial, opt => opt.MapFrom(src => "Stock inicial al crear ingrediente"));

        // TODO: Implementar cuando existan estos DTOs
        // CreateMap<IngredienteUpdateDto, ActualizarIngredienteCommand>();
    }

    /// <summary>
    /// Mapea UnidadMedida a texto según las expectativas de los tests
    /// </summary>
    private static string MapearUnidadMedidaTexto(UnidadMedida unidad)
    {
        return unidad switch
        {
            UnidadMedida.Unidad => "Unidad",
            UnidadMedida.Piezas => "Piezas", 
            UnidadMedida.Gramo => "Gramo",
            UnidadMedida.Kilogramo => "Kilogramo",
            UnidadMedida.Litro => "Litro",
            UnidadMedida.Mililitro => "Mililitro",
            UnidadMedida.Cucharada => "Cucharada",
            UnidadMedida.Cucharadita => "Cucharadita",
            UnidadMedida.Taza => "Taza",
            UnidadMedida.Paquete => "Paquete",
            _ => unidad.ToString()
        };
    }

    /// <summary>
    /// Configura los mapeos específicos para MovimientosInventario
    /// </summary>
    private void ConfigurarMapeosMovimientosInventario()
    {
        // MovimientoInventario -> MovimientoInventarioDto
        CreateMap<MovimientoInventario, RestaurantePro.Application.Inventario.MovimientosInventario.DTOs.MovimientoInventarioDto>()
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => src.Fecha))
            .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.TipoMovimiento))
            .ForMember(dest => dest.Cantidad, opt => opt.MapFrom(src => src.Cantidad))
            .ForMember(dest => dest.Motivo, opt => opt.MapFrom(src => src.Motivo))
            .ForMember(dest => dest.IngredienteId, opt => opt.MapFrom(src => src.IngredienteId))
            // Propiedades que no existen en la entidad - ignorar y asignar en handler
            .ForMember(dest => dest.CostoUnitario, opt => opt.Ignore())
            .ForMember(dest => dest.StockAnterior, opt => opt.Ignore())
            .ForMember(dest => dest.Observaciones, opt => opt.Ignore())
            .ForMember(dest => dest.UsuarioId, opt => opt.Ignore())
            .ForMember(dest => dest.NumeroDocumento, opt => opt.Ignore())
            .ForMember(dest => dest.ProveedorId, opt => opt.Ignore())
            .ForMember(dest => dest.FechaModificacion, opt => opt.Ignore())
            .ForMember(dest => dest.CreadoPor, opt => opt.Ignore())
            .ForMember(dest => dest.ModificadoPor, opt => opt.Ignore())
            .ForMember(dest => dest.Activo, opt => opt.Ignore())
            // Propiedades calculadas - las ignoramos y se asignan en el handler
            .ForMember(dest => dest.StockResultante, opt => opt.Ignore())
            .ForMember(dest => dest.NombreUsuario, opt => opt.Ignore())
            .ForMember(dest => dest.NombreIngrediente, opt => opt.Ignore())
            .ForMember(dest => dest.NombreProveedor, opt => opt.Ignore())
            .ForMember(dest => dest.ColorTipo, opt => opt.Ignore())
            .ForMember(dest => dest.IconoTipo, opt => opt.Ignore())
            .ForMember(dest => dest.FechaTexto, opt => opt.Ignore())
            .ForMember(dest => dest.CantidadTexto, opt => opt.Ignore());
    }

    /// <summary>
    /// Configura los mapeos específicos para Órdenes de Compra
    /// </summary>
    private void ConfigurarMapeosOrdenesCompra()
    {
        // TODO: Implementar cuando los DTOs estén disponibles
        // OrdenCompraDto y DetalleOrdenCompraDto no existen actualmente
        
        /*
        // OrdenCompra Entity -> OrdenCompraDto
        CreateMap<OrdenCompra, OrdenCompraDto>()
            .ForMember(dest => dest.NumeroOrden, opt => opt.MapFrom(src => src.NumeroOrden.Value))
            .ForMember(dest => dest.FechaOrden, opt => opt.MapFrom(src => src.FechaOrden))
            .ForMember(dest => dest.FechaEntregaEsperada, opt => opt.MapFrom(src => src.FechaEntregaEsperada))
            .ForMember(dest => dest.EstadoTexto, opt => opt.MapFrom(src => src.Estado.ToString()))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total.Amount))
            .ForMember(dest => dest.ProveedorId, opt => opt.MapFrom(src => src.ProveedorId))
            .ForMember(dest => dest.ProveedorNombre, opt => opt.MapFrom(src => "")) // TODO: Mapear cuando tengamos navegación
            .ForMember(dest => dest.Observaciones, opt => opt.MapFrom(src => src.Observaciones))
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => src.FechaCreacion))
            .ForMember(dest => dest.CantidadItems, opt => opt.MapFrom(src => src.Detalles.Count));

        // DetalleOrdenCompra Entity -> DetalleOrdenCompraDto
        CreateMap<DetalleOrdenCompra, DetalleOrdenCompraDto>()
            .ForMember(dest => dest.IngredienteId, opt => opt.MapFrom(src => src.IngredienteId))
            .ForMember(dest => dest.IngredienteNombre, opt => opt.MapFrom(src => "")) // TODO: Mapear cuando tengamos navegación
            .ForMember(dest => dest.CantidadSolicitada, opt => opt.MapFrom(src => src.CantidadSolicitada))
            .ForMember(dest => dest.PrecioUnitario, opt => opt.MapFrom(src => src.PrecioUnitario.Amount))
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Subtotal.Amount))
            .ForMember(dest => dest.CantidadRecibida, opt => opt.MapFrom(src => src.CantidadRecibida))
            .ForMember(dest => dest.EstaPendiente, opt => opt.MapFrom(src => src.CantidadRecibida < src.CantidadSolicitada))
            .ForMember(dest => dest.EstaCompleto, opt => opt.MapFrom(src => src.CantidadRecibida >= src.CantidadSolicitada));
        */
    }
} 