using MediatR;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Application.Core.Productos.DTOs;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache.Invalidation;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Domain.Core.Usuarios.Interfaces;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.CrearComanda;

public class CrearComandaCommandHandler : IRequestHandler<CrearComandaCommand, Result<ComandaDto>>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CrearComandaCommandHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IMesaRepository _mesaRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUsuarioRepository _usuarioRepository;

    public CrearComandaCommandHandler(
        IComandaRepository comandaRepository,
        IProductoRepository productoRepository,
        IMapper mapper,
        ILogger<CrearComandaCommandHandler> logger,
        ICacheService cacheService,
        IMesaRepository mesaRepository,
        ICurrentUserService currentUser,
        IUsuarioRepository usuarioRepository)
    {
        _comandaRepository = comandaRepository;
        _productoRepository = productoRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
        _mesaRepository = mesaRepository;
        _currentUser = currentUser;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<Result<ComandaDto>> Handle(CrearComandaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🍽️ Iniciando creación de comanda - Mesero: {MeseroId}, Mesa: {MesaId}, Tipo: {Tipo}", request.MeseroId, request.MesaId, request.Tipo);

        // Resolver SIEMPRE MeseroId desde el usuario autenticado; ignorar el valor provisto por el cliente
        var identityId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(identityId))
        {
            _logger.LogWarning("No hay IdentityId en el contexto de la petición");
            return Result.Failure<ComandaDto>("Usuario no autenticado");
        }

        var usuario = await _usuarioRepository.ObtenerPorIdentityIdAsync(identityId, cancellationToken);
        if (usuario == null || usuario.Id == Guid.Empty)
        {
            _logger.LogWarning("No se pudo mapear IdentityId {IdentityId} a Usuarios.Id. Intentando auto-provisión...", identityId);

            try
            {
                // Derivar datos desde claims del usuario actual
                var email = _currentUser.Email;
                var userName = _currentUser.UserName;
                var rolClaim = _currentUser.Rol; // puede ser null

                // Fallbacks sensatos
                if (string.IsNullOrWhiteSpace(email))
                    email = $"{identityId}@restaurantepro.local";
                if (string.IsNullOrWhiteSpace(userName))
                    userName = email.Split('@')[0];

                // Mapear rol de claim → enum (default Mesero)
                RolUsuario rolEnum;
                if (!Enum.TryParse(rolClaim, true, out rolEnum))
                {
                    rolEnum = RolUsuario.Mesero;
                }

                // 1) Si ya existe por email, asociar Identity y actualizar
                var existentePorEmail = await _usuarioRepository.ObtenerPorEmailAsync(email!, cancellationToken);
                if (existentePorEmail != null)
                {
                    existentePorEmail.AsociarIdentity(identityId);
                    existentePorEmail.ConfirmarCuenta();
                    existentePorEmail.Activar();
                    usuario = await _usuarioRepository.ActualizarAsync(existentePorEmail, cancellationToken);
                    _logger.LogInformation("👤 Usuario existente asociado a IdentityId: {UsuarioId}", usuario.Id);
                }
                else
                {
                    // 2) Crear nuevo asegurando unicidad de email y username
                    // Si email ya existe por alguna condición de competencia, regenerar con identityId
                    var emailParaCrear = email;
                    var yaExisteEmail = await _usuarioRepository.ExisteEmailAsync(emailParaCrear!, cancellationToken);
                    if (yaExisteEmail)
                    {
                        emailParaCrear = $"{identityId}@restaurantepro.local";
                    }

                    var usernameParaCrear = userName!;
                    var yaExisteUsername = await _usuarioRepository.ExisteNombreUsuarioAsync(usernameParaCrear!, cancellationToken);
                    if (yaExisteUsername)
                    {
                        usernameParaCrear = $"{userName}_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
                    }

                    var nuevoUsuario = Usuario.Crear(usernameParaCrear, usernameParaCrear, emailParaCrear!, rolEnum);
                    nuevoUsuario.AsociarIdentity(identityId);
                    nuevoUsuario.ConfirmarCuenta();
                    nuevoUsuario.Activar();

                    usuario = await _usuarioRepository.AgregarAsync(nuevoUsuario, cancellationToken);
                    _logger.LogInformation("👤 Usuario auto-provisionado: {UsuarioId} para IdentityId {IdentityId}", usuario.Id, identityId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-provisionando usuario para IdentityId {IdentityId}", identityId);
                return Result.Failure<ComandaDto>("No se encontró un usuario válido para el token actual");
            }
        }

        request.MeseroId = usuario.Id;
        _logger.LogInformation("👤 MeseroId resuelto desde usuario actual: {MeseroId}", request.MeseroId);

        // Validación básica (más validaciones profundas en el Validator)
        if (request.MeseroId == Guid.Empty)
            return Result.Failure<ComandaDto>("El mesero es obligatorio");
        if (request.Tipo == RestaurantePro.Domain.Operaciones.Comandas.Enums.TipoComanda.Mesa)
        {
            if (request.MesaId == null || request.MesaId == Guid.Empty)
                return Result.Failure<ComandaDto>("La mesa es obligatoria para comandas de tipo Mesa");
        }
        if (request.ProductosIniciales == null || !request.ProductosIniciales.Any())
            return Result.Failure<ComandaDto>("Debe agregar al menos un producto a la comanda");

        // 0. Validar que la mesa exista si viene especificada (evita FK 547)
        if (request.MesaId.HasValue && request.MesaId.Value != Guid.Empty)
        {
            var mesaExistente = await _mesaRepository.ObtenerPorIdAsync(request.MesaId.Value, cancellationToken);
            if (mesaExistente == null)
            {
                _logger.LogWarning("❌ Mesa no encontrada: {MesaId}", request.MesaId);
                return Result.Failure<ComandaDto>($"La mesa {request.MesaId} no existe");
            }
        }

        // 1. Crear la comanda usando el factory del dominio
        var comanda = Comanda.Crear(
            request.MeseroId,
            request.ClienteId,
            request.MesaId,
            request.Observaciones,
            null,
            request.Tipo,
            request.NombreEntrega,
            request.DireccionEntrega,
            request.TelefonoEntrega
        );

        // 2. Agregar productos iniciales
        foreach (var prod in request.ProductosIniciales)
        {
            // Validar producto
            var producto = await _productoRepository.ObtenerPorIdAsync(prod.ProductoId, cancellationToken);
            if (producto == null)
                return Result.Failure<ComandaDto>($"El producto con ID {prod.ProductoId} no existe");
            if (!producto.EstaActivo)
                return Result.Failure<ComandaDto>($"El producto '{producto.Nombre}' no está activo");
            if (prod.Cantidad <= 0)
                return Result.Failure<ComandaDto>($"La cantidad para el producto '{producto.Nombre}' debe ser mayor a cero");

            // Agregar al dominio (usando método del agregado)
            try
            {
                comanda.AgregarItem(
                    producto.Id,
                    producto.Nombre ?? "Producto sin nombre",
                    prod.Cantidad,
                    producto.Precio?.Valor ?? 0,
                    prod.Observaciones
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar producto a la comanda");
                return Result.Failure<ComandaDto>($"Error al agregar producto '{producto.Nombre}': {ex.Message}");
            }
        }

        // 3. Guardar en BD
        await _comandaRepository.AgregarAsync(comanda, cancellationToken);
        await _comandaRepository.GuardarCambiosAsync(cancellationToken);

        // 3.1. Si hay mesa asociada, marcarla como Ocupada
        if (request.MesaId.HasValue && request.MesaId.Value != Guid.Empty)
        {
            try
            {
                var mesa = await _mesaRepository.ObtenerPorIdAsync(request.MesaId.Value, cancellationToken);
                if (mesa != null)
                {
                    mesa.MarcarComoOcupada();
                    await _mesaRepository.ActualizarAsync(mesa, cancellationToken);
                    await _mesaRepository.GuardarCambiosAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo marcar la mesa {MesaId} como Ocupada al crear la comanda {ComandaId}", request.MesaId, comanda.Id);
                // Continuar sin bloquear la creación de la comanda
            }
        }

        // 4. Invalidar caché para que el listado refleje la nueva comanda y el estado de mesas
        _cacheService.InvalidatePattern("ObtenerComandasPaginadasQuery_");
        _cacheService.InvalidatePattern("ObtenerComandasPorMesaQuery_");
        _cacheService.InvalidateForEntity("ObtenerComandaPorIdQuery_", comanda.Id);
        _cacheService.InvalidatePattern("ObtenerMesasDisponiblesQuery_");
        _cacheService.InvalidatePattern("ObtenerEstadoMesasQuery_");

        // 5. Mapear a DTO y retornar
        var dto = _mapper.Map<ComandaDto>(comanda);
        return Result.Success(dto);
    }
} 