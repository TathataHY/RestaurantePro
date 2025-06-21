namespace RestaurantePro.Application.Core.Usuarios.Commands.EliminarUsuario;

/// <summary>
/// Handler para eliminar un usuario del sistema
/// </summary>
public class EliminarUsuarioHandler : IRequestHandler<EliminarUsuarioCommand, Result<bool>>
{
    private readonly IUsuarioService _usuarioService;
    private readonly ILogger<EliminarUsuarioHandler> _logger;

    public EliminarUsuarioHandler(
        IUsuarioService usuarioService,
        ILogger<EliminarUsuarioHandler> logger)
    {
        _usuarioService = usuarioService ?? throw new ArgumentNullException(nameof(usuarioService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> Handle(
        EliminarUsuarioCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Eliminando usuario con ID: {Id}", request.Id);

            // Verificar que el usuario existe
            var usuario = await _usuarioService.ObtenerPorIdAsync(request.Id, cancellationToken);
            if (usuario == null)
            {
                _logger.LogWarning("Usuario no encontrado para eliminar: {Id}", request.Id);
                return Result.Failure<bool>("Usuario no encontrado");
            }

            // Verificar que no se está eliminando a sí mismo
            if (request.Id == request.UsuarioEliminadorId)
            {
                _logger.LogWarning("Intento de auto-eliminación: {Id}", request.Id);
                return Result.Failure<bool>("No puede eliminarse a sí mismo");
            }

            // Eliminar el usuario (soft delete)
            await _usuarioService.EliminarAsync(request.Id, cancellationToken);

            _logger.LogInformation("Usuario eliminado exitosamente: {Email}", usuario.Email);

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar usuario: {Id}", request.Id);
            return Result.Failure<bool>("Error al eliminar usuario");
        }
    }
} 