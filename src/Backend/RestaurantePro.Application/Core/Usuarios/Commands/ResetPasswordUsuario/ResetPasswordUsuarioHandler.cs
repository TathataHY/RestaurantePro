namespace RestaurantePro.Application.Core.Usuarios.Commands.ResetPasswordUsuario;

/// <summary>
/// Handler para resetear la contraseña de un usuario
/// </summary>
public class ResetPasswordUsuarioHandler : IRequestHandler<ResetPasswordUsuarioCommand, Result<bool>>
{
    private readonly IUsuarioService _usuarioService;
    private readonly ILogger<ResetPasswordUsuarioHandler> _logger;

    public ResetPasswordUsuarioHandler(
        IUsuarioService usuarioService,
        ILogger<ResetPasswordUsuarioHandler> logger)
    {
        _usuarioService = usuarioService ?? throw new ArgumentNullException(nameof(usuarioService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> Handle(
        ResetPasswordUsuarioCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Reseteando contraseña de usuario: {Id}", request.Id);

            // Verificar que el usuario existe
            var usuario = await _usuarioService.ObtenerPorIdAsync(request.Id, cancellationToken);
            if (usuario == null)
            {
                _logger.LogWarning("Usuario no encontrado para resetear contraseña: {Id}", request.Id);
                return Result.Failure<bool>("Usuario no encontrado");
            }

            // Generar nueva contraseña temporal si no se proporciona
            var nuevaPassword = request.NuevaPasswordTemporal ?? GenerarPasswordTemporal();

            // Resetear la contraseña del usuario
            await _usuarioService.ResetearPasswordAsync(request.Id, nuevaPassword, cancellationToken);

            _logger.LogInformation("Contraseña de usuario reseteada exitosamente: {Email}", usuario.Email);

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al resetear contraseña de usuario: {Id}", request.Id);
            return Result.Failure<bool>("Error al resetear contraseña de usuario");
        }
    }

    private static string GenerarPasswordTemporal()
    {
        // Generar contraseña temporal de 8 caracteres
        const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(caracteres, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
} 