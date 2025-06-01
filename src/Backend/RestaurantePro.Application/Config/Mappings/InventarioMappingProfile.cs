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
        ConfigurarMapeosOrdenesCompra();
    }

    /// <summary>
    /// Configura los mapeos específicos para Ingredientes
    /// </summary>
    private void ConfigurarMapeosIngredientes()
    {
        // Ingrediente Entity -> IngredienteDto
        CreateMap<Ingrediente, IngredienteDto>()
            // .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre.Valor))
            // .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.Descripcion.Valor))
            // .ForMember(dest => dest.CantidadStock, opt => opt.MapFrom(src => src.Stock.Cantidad))
            .ForMember(dest => dest.UnidadMedida, opt => opt.MapFrom(src => src.UnidadMedida.ToString()))
            // .ForMember(dest => dest.CostoPromedio, opt => opt.MapFrom(src => src.Stock.CostoPromedio))
            // .ForMember(dest => dest.StockMinimo, opt => opt.MapFrom(src => src.Stock.StockMinimo))
            // .ForMember(dest => dest.StockMaximo, opt => opt.MapFrom(src => src.Stock.StockMaximo))
            // .ForMember(dest => dest.Categoria, opt => opt.MapFrom(src => src.Categoria.ToString()))
            // .ForMember(dest => dest.ProveedorPrincipalId, opt => opt.MapFrom(src => src.ProveedorPrincipal != null ? src.ProveedorPrincipal.Id : (Guid?)null))
            // .ForMember(dest => dest.ProveedorPrincipalNombre, opt => opt.MapFrom(src => src.ProveedorPrincipal != null ? src.ProveedorPrincipal.Nombre : string.Empty))
            // .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.Activo))
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => src.FechaCreacion));
            // .ForMember(dest => dest.CreadoPor, opt => opt.MapFrom(src => src.CreadoPor)); // TODO: Propiedad no existe

        // Ingrediente Entity -> IngredienteSummaryDto (para listas)
        CreateMap<Ingrediente, IngredienteSummaryDto>()
            // .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre.Valor))
            // .ForMember(dest => dest.CantidadStock, opt => opt.MapFrom(src => src.Stock.Cantidad))
            // .ForMember(dest => dest.UnidadMedidaTexto, opt => opt.MapFrom(src => src.UnidadMedida.ToString()))
            // .ForMember(dest => dest.CostoPromedio, opt => opt.MapFrom(src => src.Stock.CostoPromedio))
            // .ForMember(dest => dest.StockMinimo, opt => opt.MapFrom(src => src.Stock.StockMinimo))
            // .ForMember(dest => dest.Categoria, opt => opt.MapFrom(src => src.Categoria.ToString()))
            // .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.Activo))
            // Propiedades calculadas se asignan en el handler
            // .ForMember(dest => dest.EstadoStock, opt => opt.Ignore())
            // .ForMember(dest => dest.ColorEstado, opt => opt.Ignore())
            // .ForMember(dest => dest.PorcentajeStock, opt => opt.Ignore())
            // .ForMember(dest => dest.EstaBajoMinimo, opt => opt.Ignore())
            // .ForMember(dest => dest.DiasRestantesOptimo, opt => opt.Ignore())
            // .ForMember(dest => dest.FechaProximaReposicion, opt => opt.Ignore())
            // .ForMember(dest => dest.CantidadSugerida, opt => opt.Ignore())
            // .ForMember(dest => dest.ProveedorSugerido, opt => opt.Ignore())
            // .ForMember(dest => dest.UltimoMovimiento, opt => opt.Ignore())
            .ForMember(dest => dest.FechaRegistro, opt => opt.MapFrom(src => DateTime.UtcNow)); // ARREGLADO: Mapeo directo sin IncludeBase

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
    /// Configura los mapeos específicos para MovimientosInventario
    /// </summary>
    private void ConfigurarMapeosMovimientosInventario()
    {
        // MovimientoInventario -> MovimientoInventarioDto
        CreateMap<MovimientoInventario, MovimientoInventarioDto>()
            // .ForMember(dest => dest.TipoMovimientoTexto, opt => opt.MapFrom(src => src.TipoMovimiento.ToString()))
            .ForMember(dest => dest.Fecha, opt => opt.MapFrom(src => DateTime.UtcNow)); // ARREGLADO: Usar Fecha en lugar de FechaCreacion
            // .ForMember(dest => dest.FechaMovimiento, opt => opt.MapFrom(src => src.FechaMovimiento)); // TODO: Propiedad no existe
            // TODO: Reactivar cuando existan estas propiedades en MovimientoInventarioDto
            //.ForMember(dest => dest.NombreIngrediente, opt => opt.MapFrom(src => src.Ingrediente != null ? src.Ingrediente.Nombre : string.Empty))
            //.ForMember(dest => dest.NombreUsuario, opt => opt.MapFrom(src => src.Usuario != null ? src.Usuario.Nombre : string.Empty));
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