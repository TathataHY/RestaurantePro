namespace RestaurantePro.Application.Core.Usuarios.Queries.ObtenerUsuariosPaginados;

/// <summary>
/// Handler para obtener usuarios con paginación y filtros
/// </summary>
public class ObtenerUsuariosPaginadosHandler : IRequestHandler<ObtenerUsuariosPaginadosQuery, Result<List<UsuarioDto>>>
{
    private readonly IUsuarioService _usuarioService;
    private readonly ILogger<ObtenerUsuariosPaginadosHandler> _logger;

    public ObtenerUsuariosPaginadosHandler(
        IUsuarioService usuarioService,
        ILogger<ObtenerUsuariosPaginadosHandler> logger)
    {
        _usuarioService = usuarioService ?? throw new ArgumentNullException(nameof(usuarioService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<UsuarioDto>>> Handle(
        ObtenerUsuariosPaginadosQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Obteniendo usuarios paginados - Página: {PageNumber}, Tamaño: {PageSize}", 
                request.PageNumber, request.PageSize);

            // Obtener todos los usuarios
            var usuarios = await _usuarioService.ObtenerTodosAsync(request.SoloActivos, cancellationToken);

            // Aplicar filtro si se especifica
            if (!string.IsNullOrWhiteSpace(request.Filtro))
            {
                usuarios = usuarios.Where(u => 
                    u.NombreCompleto.Contains(request.Filtro, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(request.Filtro, StringComparison.OrdinalIgnoreCase) ||
                    u.NombreUsuario.Contains(request.Filtro, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Aplicar ordenamiento
            usuarios = AplicarOrdenamiento(usuarios, request.OrderBy, request.OrderDirection);

            // Aplicar paginación
            var usuariosPaginados = usuarios
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Convertir a DTOs
            var usuariosDto = usuariosPaginados.Select(u => new UsuarioDto
            {
                Id = u.Id,
                NombreCompleto = u.NombreCompleto,
                Email = u.Email,
                NombreUsuario = u.NombreUsuario,
                Estado = u.Estado,
                TipoUsuario = u.TipoUsuario,
                Rol = u.Rol,
                NivelAcceso = u.NivelAcceso,
                Permisos = u.Permisos.ToList(),
                SupervisorId = u.SupervisorId,
                Departamento = u.Departamento,
                Posicion = u.Posicion,
                Identificacion = u.Identificacion,
                UltimoAcceso = u.UltimoAcceso,
                MotivoBloqueo = u.MotivoBloqueo,
                EsAdministrador = u.EsAdministrador,
                Roles = u.Roles.Select(r => r.ToString()).ToList()
            }).ToList();

            _logger.LogInformation("Usuarios obtenidos exitosamente - Total: {Total}, Página: {PageNumber}", 
                usuariosDto.Count, request.PageNumber);

            return Result<List<UsuarioDto>>.Success(usuariosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios paginados");
            return Result.Failure<List<UsuarioDto>>("Error al obtener usuarios");
        }
    }

    private static IEnumerable<Usuario> AplicarOrdenamiento(
        IEnumerable<Usuario> usuarios, 
        string orderBy, 
        string orderDirection)
    {
        var query = orderBy.ToLower() switch
        {
            "nombrecompleto" => usuarios.OrderBy(u => u.NombreCompleto),
            "email" => usuarios.OrderBy(u => u.Email),
            "nombreusuario" => usuarios.OrderBy(u => u.NombreUsuario),
            "estado" => usuarios.OrderBy(u => u.Estado),
            "fechacreacion" => usuarios.OrderBy(u => u.FechaCreacion),
            _ => usuarios.OrderBy(u => u.NombreCompleto)
        };

        return orderDirection.ToLower() == "desc" ? query.Reverse() : query;
    }
} 