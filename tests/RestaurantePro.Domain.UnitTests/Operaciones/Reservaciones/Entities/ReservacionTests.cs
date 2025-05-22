namespace RestaurantePro.Domain.UnitTests.Operaciones.Reservaciones.Entities
{
    public class ReservacionTests
    {
        [Fact]
        public void CrearReservacion_ConParametrosValidos_DebeCrearReservacionPendiente()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            var fecha = DateTime.Now.AddDays(1);
            var hora = new TimeSpan(20, 0, 0); // 8:00 PM
            var cantidadPersonas = 4;
            var observaciones = "Mesa junto a la ventana";

            // Act
            var reservacion = Reservacion.Crear(clienteId, mesaId, fecha, hora, cantidadPersonas, "612345678", "cliente@example.com", observaciones);

            // Assert
            reservacion.Should().NotBeNull();
            reservacion.Id.Should().NotBe(Guid.Empty);
            reservacion.ClienteId.Should().Be(clienteId);
            reservacion.MesaId.Should().Be(mesaId);
            reservacion.Fecha.Should().Be(fecha.Date);
            reservacion.Hora.Should().Be(hora);
            reservacion.CantidadPersonas.Should().Be(cantidadPersonas);
            reservacion.Observaciones.Should().Be(observaciones);
            reservacion.Estado.Should().Be(EstadoReservacion.Pendiente);

            // Verificamos que se generó el evento de dominio
            reservacion.DomainEvents.Should().ContainSingle(e => e is ReservacionCreada);
            var evento = reservacion.DomainEvents.OfType<ReservacionCreada>().First();
            evento.ReservacionId.Should().Be(reservacion.Id);
            evento.ClienteId.Should().Be(clienteId);
            evento.MesaId.Should().Be(mesaId);
        }

        [Fact]
        public void ConfirmarReservacion_CuandoEstaPendiente_DebeCambiarEstadoAConfirmada()
        {
            // Arrange
            var reservacion = Reservacion.Crear(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTime.Now.AddDays(1),
                new TimeSpan(20, 0, 0),
                4,
                "612345678", // Teléfono
                "cliente@example.com", // Email
                "Observación");

            // Act
            reservacion.Confirmar();

            // Assert
            reservacion.Estado.Should().Be(EstadoReservacion.Confirmada);

            // Verificar evento de dominio
            reservacion.DomainEvents.Should().Contain(e => e is ReservacionConfirmada);
            var evento = reservacion.DomainEvents.OfType<ReservacionConfirmada>().Last();
            evento.ReservacionId.Should().Be(reservacion.Id);
        }

        [Fact]
        public void CancelarReservacion_CuandoEstaConfirmada_DebeCambiarEstadoACancelada()
        {
            // Arrange
            var reservacion = Reservacion.Crear(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTime.Now.AddDays(1),
                new TimeSpan(20, 0, 0),
                4,
                "612345678", // Teléfono
                "cliente@example.com", // Email
                "Observación");
            reservacion.Confirmar();
            var motivo = "El cliente no puede asistir";

            // Act
            reservacion.Cancelar(motivo);

            // Assert
            reservacion.Estado.Should().Be(EstadoReservacion.Cancelada);
            reservacion.MotivoCancelacion.Should().Be(motivo);

            // Verificar evento de dominio
            reservacion.DomainEvents.Should().Contain(e => e is ReservacionCancelada);
            var evento = reservacion.DomainEvents.OfType<ReservacionCancelada>().Last();
            evento.ReservacionId.Should().Be(reservacion.Id);
            evento.Motivo.Should().Be(motivo);
        }

        [Fact]
        public void CompletarReservacion_CuandoEstaConfirmada_DebeCambiarEstadoACompletada()
        {
            // Arrange
            var reservacion = Reservacion.Crear(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTime.Now.AddDays(1),
                new TimeSpan(20, 0, 0),
                4,
                "612345678", // Teléfono
                "cliente@example.com", // Email
                "Observación");
            reservacion.Confirmar();

            // Act
            reservacion.Completar();

            // Assert
            reservacion.Estado.Should().Be(EstadoReservacion.Completada);

            // Verificar evento de dominio
            reservacion.DomainEvents.Should().Contain(e => e is ReservacionCompletada);
            var evento = reservacion.DomainEvents.OfType<ReservacionCompletada>().Last();
            evento.ReservacionId.Should().Be(reservacion.Id);
        }

        [Fact]
        public void CancelarReservacion_ConReservacionYaCompletada_DebeLanzarExcepcion()
        {
            // Arrange
            var reservacion = Reservacion.Crear(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTime.Now.AddDays(1),
                new TimeSpan(20, 0, 0),
                4,
                "612345678", // Teléfono
                "cliente@example.com", // Email
                "Observación");
            reservacion.Confirmar();
            reservacion.Completar();

            // Act & Assert
            Action action = () => reservacion.Cancelar("Motivo");
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*no puede cancelarse*");
        }

        [Fact]
        public void CrearReservacion_ConFechaEnPasado_DebeLanzarExcepcion()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            var fechaPasada = DateTime.Now.AddDays(-1); // Fecha en el pasado
            var hora = new TimeSpan(20, 0, 0);
            var cantidadPersonas = 4;

            // Act & Assert
            Action action = () => Reservacion.Crear(clienteId, mesaId, fechaPasada, hora, cantidadPersonas, "612345678", "cliente@example.com", "Obs");
            action.Should().Throw<ArgumentException>()
                .WithMessage("*fecha de reservación debe ser futura*");
        }

        [Fact]
        public void CrearReservacion_ConCantidadPersonasInvalida_DebeLanzarExcepcion()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            var fecha = DateTime.Now.AddDays(1);
            var hora = new TimeSpan(20, 0, 0);
            var cantidadPersonasInvalida = 0; // Cantidad inválida

            // Act & Assert
            Action action = () => Reservacion.Crear(clienteId, mesaId, fecha, hora, cantidadPersonasInvalida, "612345678", "cliente@example.com", "Obs");
            action.Should().Throw<ArgumentException>()
                .WithMessage("*cantidad de personas debe ser mayor que cero*");
        }

        [Fact]
        public void Crear_ConParametrosValidos_DebeCrearReservacion()
        {
            // Arrange
            var mesaId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var fecha = DateTime.Now.AddDays(1);
            var duracion = TimeSpan.FromMinutes(90);
            var cantidadPersonas = 4;
            var telefono = "612345678";  // Añadir teléfono
            var email = "cliente@example.com";  // Añadir email
            var observaciones = "Reservación de prueba";
            
            // Act
            var reservacion = Reservacion.Crear(
                mesaId, 
                clienteId, 
                fecha, 
                duracion, 
                cantidadPersonas,
                telefono,  // Añadir teléfono
                email,  // Añadir email
                observaciones);
            
            // Assert
            Assert.Equal(mesaId, reservacion.MesaId);
            Assert.Equal(clienteId, reservacion.ClienteId);
            Assert.Equal(fecha.Date, reservacion.Fecha);
            Assert.Equal(fecha.TimeOfDay, reservacion.Hora);
            Assert.Equal(duracion, reservacion.DuracionEstimada);
            Assert.Equal(cantidadPersonas, reservacion.CantidadPersonas);
            Assert.Equal(observaciones, reservacion.Observaciones);
            Assert.Equal(EstadoReservacion.Pendiente, reservacion.Estado);
            Assert.NotEqual(Guid.Empty, reservacion.Id);
            
            // Verificar que el evento de creación fue registrado
            var eventoCreacion = reservacion.DomainEvents
                .OfType<ReservacionCreada>()
                .SingleOrDefault();
            
            Assert.NotNull(eventoCreacion);
            Assert.Equal(reservacion.Id, eventoCreacion.ReservacionId);
            Assert.Equal(clienteId, eventoCreacion.ClienteId);
            Assert.Equal(mesaId, eventoCreacion.MesaId);
        }

        [Fact]
        public void Crear_ConFechaPasada_DebeLanzarArgumentException()
        {
            // Arrange
            var mesaId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var fechaPasada = DateTime.Now.AddDays(-1);
            var duracion = TimeSpan.FromMinutes(90);
            var cantidadPersonas = 4;
            var telefono = "612345678";  // Añadir teléfono
            var email = "cliente@example.com";  // Añadir email
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => Reservacion.Crear(
                mesaId, 
                clienteId, 
                fechaPasada, 
                duracion, 
                cantidadPersonas,
                telefono,  // Añadir teléfono
                email  // Añadir email
            ));
        }

        [Fact]
        public void Crear_ConCantidadPersonasCero_DebeLanzarArgumentException()
        {
            // Arrange
            var mesaId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var fecha = DateTime.Now.AddDays(1);
            var duracion = TimeSpan.FromMinutes(90);
            var cantidadPersonas = 0;
            var telefono = "612345678";  // Añadir teléfono
            var email = "cliente@example.com";  // Añadir email
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => Reservacion.Crear(
                mesaId, 
                clienteId, 
                fecha, 
                duracion, 
                cantidadPersonas,
                telefono,  // Añadir teléfono
                email  // Añadir email
            ));
        }

        [Fact]
        public void Crear_ConDuracionDemasiadoCorta_DebeLanzarArgumentException()
        {
            // Arrange
            var mesaId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var fecha = DateTime.Now.AddDays(1);
            var duracion = TimeSpan.FromMinutes(5); // Menos de 15 minutos
            var cantidadPersonas = 4;
            var telefono = "612345678";  // Añadir teléfono
            var email = "cliente@example.com";  // Añadir email
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => Reservacion.Crear(
                mesaId, 
                clienteId, 
                fecha, 
                duracion, 
                cantidadPersonas,
                telefono,  // Añadir teléfono
                email  // Añadir email
            ));
        }

        [Fact]
        public void Confirmar_ReservacionPendiente_DebeCambiarEstadoAConfirmada()
        {
            // Arrange
            var mesaId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var fecha = DateTime.Now.AddDays(1);
            var duracion = TimeSpan.FromMinutes(90);
            var cantidadPersonas = 4;
            var telefono = "612345678";  // Añadir teléfono
            var email = "cliente@example.com";  // Añadir email
            
            var reservacion = Reservacion.Crear(
                mesaId, 
                clienteId, 
                fecha, 
                duracion, 
                cantidadPersonas,
                telefono,  // Añadir teléfono
                email  // Añadir email
            );
            
            // Act
            reservacion.Confirmar();
            
            // Assert
            Assert.Equal(EstadoReservacion.Confirmada, reservacion.Estado);
            
            // Verificar que el evento de confirmación fue registrado
            var eventoConfirmacion = reservacion.DomainEvents
                .OfType<ReservacionConfirmada>()
                .SingleOrDefault();
                
            Assert.NotNull(eventoConfirmacion);
            Assert.Equal(reservacion.Id, eventoConfirmacion.ReservacionId);
        }

        [Fact]
        public void Cancelar_ReservacionPendienteOConfirmada_DebeCambiarEstadoACancelada()
        {
            // Arrange
            var mesaId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var fecha = DateTime.Now.AddDays(1);
            var duracion = TimeSpan.FromMinutes(90);
            var cantidadPersonas = 4;
            var telefono = "612345678";  // Añadir teléfono
            var email = "cliente@example.com";  // Añadir email
            var motivo = "Prueba de cancelación";
            
            var reservacion = Reservacion.Crear(
                mesaId, 
                clienteId, 
                fecha, 
                duracion, 
                cantidadPersonas,
                telefono,  // Añadir teléfono
                email  // Añadir email
            );
            
            // Act
            reservacion.Cancelar(motivo);
            
            // Assert
            Assert.Equal(EstadoReservacion.Cancelada, reservacion.Estado);
            Assert.Equal(motivo, reservacion.MotivoCancelacion);
            
            // Verificar que el evento de cancelación fue registrado
            var eventoCancelacion = reservacion.DomainEvents
                .OfType<ReservacionCancelada>()
                .SingleOrDefault();
                
            Assert.NotNull(eventoCancelacion);
            Assert.Equal(reservacion.Id, eventoCancelacion.ReservacionId);
        }
    }
}

