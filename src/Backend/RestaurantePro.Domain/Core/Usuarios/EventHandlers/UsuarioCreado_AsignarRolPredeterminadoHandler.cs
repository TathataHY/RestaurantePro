namespace RestaurantePro.Domain.Core.Usuarios.EventHandlers
{
    /// <summary>
    /// Manejador de eventos que asigna roles predeterminados a un usuario recién creado.
    /// </summary>
    public class UsuarioCreado_AsignarRolPredeterminadoHandler : IEventHandler<UsuarioCreado>
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IRolRepository _rolRepository;
        private readonly IDomainEventRegistry _eventRegistry;
        private readonly IDateTimeService _dateTimeService;

        public UsuarioCreado_AsignarRolPredeterminadoHandler(
            IUsuarioRepository usuarioRepository,
            IRolRepository rolRepository,
            IDomainEventRegistry eventRegistry,
            IDateTimeService dateTimeService)
        {
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _rolRepository = rolRepository ?? throw new ArgumentNullException(nameof(rolRepository));
            _eventRegistry = eventRegistry ?? throw new ArgumentNullException(nameof(eventRegistry));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }

        public async Task Handle(UsuarioCreado evento, CancellationToken cancellationToken)
        {
            // Registrar evento para historial
            await _eventRegistry.RegisterAsync(evento, cancellationToken);

            // Obtener el usuario recién creado
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(evento.UsuarioId, cancellationToken);
            if (usuario == null)
            {
                // Usuario no encontrado, no podemos hacer nada
                // En un caso real, deberíamos loggear este error
                return;
            }

            // Determinar el rol a asignar según el tipo de usuario
            Rol rolParaAsignar;

            // Si el evento o usuario indica que es un administrador, asignar rol de administrador
            if (evento.TipoUsuario == Enums.TipoUsuario.Administrador || 
                usuario.TipoUsuario == Enums.TipoUsuario.Administrador)
            {
                rolParaAsignar = await _rolRepository.ObtenerPorTipoUsuarioAsync(
                    Enums.TipoUsuario.Administrador, cancellationToken);
            }
            else
            {
                // De lo contrario, obtener el rol predeterminado (empleado común)
                rolParaAsignar = await _rolRepository.ObtenerPredeterminadoAsync(cancellationToken);
            }

            if (rolParaAsignar != null)
            {
                // Asignar el rol al usuario
                usuario.AsignarRol(rolParaAsignar.Id);

                // Guardar los cambios
                await _usuarioRepository.GuardarAsync(usuario, cancellationToken);
            }
        }
    }
} 