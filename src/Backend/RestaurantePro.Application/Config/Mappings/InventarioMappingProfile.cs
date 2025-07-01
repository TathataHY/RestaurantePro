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
            .ForMember(dest => dest.Categoria, opt => opt.Ignore())
            .ForMember(dest => dest.StockActual, opt => opt.MapFrom(src => src.Stock))
            .ForMember(dest => dest.StockMaximo, opt => opt.Ignore())
            .ForMember(dest => dest.StockMinimo, opt => opt.MapFrom(src => src.StockMinimo))
            .ForMember(dest => dest.CostoUnitario, opt => opt.Ignore())
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.EstaActivo))
            .ForMember(dest => dest.RegistradoPor, opt => opt.MapFrom(src => src.CreatedBy))
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
        // OrdenCompra Entity -> OrdenCompraDto
        CreateMap<RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra, RestaurantePro.Application.Inventario.OrdenesCompra.DTOs.OrdenCompraDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.NumeroOrden, opt => opt.MapFrom(src => src.Id.ToString())) // Usar ID como número de orden
            .ForMember(dest => dest.ProveedorId, opt => opt.MapFrom(src => src.ProveedorId))
            .ForMember(dest => dest.NombreProveedor, opt => opt.MapFrom(src => string.Empty)) // Se llenará desde el handler
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => src.FechaEmision))
            .ForMember(dest => dest.FechaEntregaEsperada, opt => opt.MapFrom(src => src.FechaEntregaEstimada))
            .ForMember(dest => dest.FechaEntregaReal, opt => opt.MapFrom(src => src.FechaRecepcion))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total))
            .ForMember(dest => dest.Observaciones, opt => opt.MapFrom(src => src.Observaciones))
            .ForMember(dest => dest.UsuarioId, opt => opt.MapFrom(src => (Guid?)null)) // No existe en la entidad
            .ForMember(dest => dest.NombreUsuario, opt => opt.MapFrom(src => string.Empty)) // No existe en la entidad
            .ForMember(dest => dest.FechaAprobacion, opt => opt.MapFrom(src => (DateTime?)null)) // No existe en la entidad
            .ForMember(dest => dest.FechaRechazo, opt => opt.MapFrom(src => (DateTime?)null)) // No existe en la entidad
            .ForMember(dest => dest.MotivoRechazo, opt => opt.MapFrom(src => (string?)null)) // No existe en la entidad
            .ForMember(dest => dest.FechaRecepcion, opt => opt.MapFrom(src => src.FechaRecepcion))
            .ForMember(dest => dest.NotasRecepcion, opt => opt.MapFrom(src => src.ObservacionesRecepcion))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        // OrdenCompraItem Entity -> OrdenCompraItemDto
        CreateMap<RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities.ItemOrdenCompra, RestaurantePro.Application.Inventario.OrdenesCompra.DTOs.OrdenCompraItemDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.IngredienteId, opt => opt.MapFrom(src => src.IngredienteId))
            .ForMember(dest => dest.NombreIngrediente, opt => opt.MapFrom(src => src.NombreIngrediente))
            .ForMember(dest => dest.Cantidad, opt => opt.MapFrom(src => (int)src.Cantidad))
            .ForMember(dest => dest.PrecioUnitario, opt => opt.MapFrom(src => src.PrecioUnitario))
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Subtotal))
            .ForMember(dest => dest.Observaciones, opt => opt.MapFrom(src => (string?)null)); // No existe en la entidad
    }
} 