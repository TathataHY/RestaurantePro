using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using RestaurantePro.Core.DTOs.Comanda;
using RestaurantePro.Core.Enums;
using RestaurantePro.Core.Services;
using Moq;
using AutoMapper;
using MediatR;
using RestaurantePro.Infrastructure.Data;
using RestaurantePro.Core.Interfaces.Services;
using RestaurantePro.Core.Interfaces;
using Microsoft.Extensions.Logging;
using RestaurantePro.Infrastructure.Repositories;
using RestaurantePro.Tests.Mapping;
using RestaurantePro.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Core.Exceptions;
using RestaurantePro.Core.Validators;

namespace RestaurantePro.Tests.Integration
{
    public class ComandaFlowTests : IClassFixture<TestDatabaseFixture>
    {
        private readonly TestDatabaseFixture _fixture;
        private readonly ComandaService _comandaService;
        private readonly ComandaValidator _validator;

        public ComandaFlowTests(TestDatabaseFixture fixture)
        {
            _fixture = fixture;
            
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var unitOfWorkLogger = loggerFactory.CreateLogger<UnitOfWork>();
            var comandaRepoLogger = loggerFactory.CreateLogger<ComandaRepository>();
            var mesaRepoLogger = loggerFactory.CreateLogger<MesaRepository>();
            var platoRepoLogger = loggerFactory.CreateLogger<PlatoRepository>();

            var unitOfWork = new UnitOfWork(
                _fixture.Context, 
                unitOfWorkLogger,
                comandaRepoLogger,
                mesaRepoLogger,
                platoRepoLogger
            );

            _validator = new ComandaValidator(unitOfWork);

            var mapper = new MapperConfiguration(cfg => 
            {
                cfg.AddProfile<ComandaMappingProfile>();
            }).CreateMapper();
            
            var notificationService = new Mock<INotificationService>().Object;
            var mediator = new Mock<IMediator>().Object;
            var userContext = new Mock<IUserContext>().Object;
            var stateManager = new ComandaStateManager(unitOfWork);

            _comandaService = new ComandaService(
                unitOfWork,
                mapper,
                notificationService,
                stateManager,
                mediator,
                userContext,
                _validator
            );
        }

        [Fact]
        public async Task FlujoComandasCompleto_DebeCompletarseSinErrores()
        {
            // Arrange
            var mesa = await _fixture.Context.Mesas.FirstAsync();
            var plato = await _fixture.Context.Platos.FirstAsync();
            
            var createDto = new ComandaCreateDto 
            { 
                MesaId = mesa.Id,
                MeseroId = "mesero-test-1",
                Detalles = new List<ComandaDetalleCreateDto>
                {
                    new() { PlatoId = plato.Id, Cantidad = 2, Observaciones = "Sin observaciones" }
                },
                Observaciones = "Sin observaciones"
            };

            // Act & Assert
            var comanda = await _comandaService.CreateAsync(createDto);
            comanda.Should().NotBeNull();
            comanda.Estado.Should().Be(EstadoComanda.Pendiente);

            var mesaActualizada = await _fixture.Context.Mesas.FindAsync(mesa.Id);
            mesaActualizada.Estado.Should().Be(EstadoMesa.Ocupada);
        }

        [Fact]
        public async Task CrearComanda_ConCantidadExtrema_DeberiaManejarCorrectamente()
        {
            // Arrange
            var mesa = await _fixture.Context.Mesas.FirstAsync();
            var plato = await _fixture.Context.Platos.FirstAsync();
            
            // Asegurarse de que el plato tenga stock suficiente
            plato.Stock = int.MaxValue;
            plato.Disponible = true;
            _fixture.Context.Update(plato);
            await _fixture.Context.SaveChangesAsync();
            
            // Limpiar el contexto
            _fixture.Context.ChangeTracker.Clear();
            
            var createDto = new ComandaCreateDto 
            { 
                MesaId = mesa.Id,
                MeseroId = "mesero-test-1",
                Detalles = new List<ComandaDetalleCreateDto>
                {
                    new() { 
                        PlatoId = plato.Id, 
                        Cantidad = int.MaxValue, 
                        Observaciones = "Prueba cantidad máxima" 
                    }
                },
                Observaciones = "Test cantidad máxima"
            };

            // Act & Assert
            var comanda = await _comandaService.CreateAsync(createDto);
            comanda.Should().NotBeNull();
            comanda.Detalles.First().Cantidad.Should().Be(int.MaxValue);
        }

        [Fact]
        public async Task CrearComanda_SinDetalles_DeberiaRetornarError()
        {
            // Arrange
            var mesa = await _fixture.Context.Mesas.FirstAsync();
            
            var createDto = new ComandaCreateDto 
            { 
                MesaId = mesa.Id,
                MeseroId = "mesero-test-1",
                Detalles = new List<ComandaDetalleCreateDto>(),
                Observaciones = "Sin observaciones"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                async () => await _comandaService.CreateAsync(createDto)
            );

            exception.Message.Should().Contain("La comanda debe tener al menos un detalle");
        }

        [Fact]
        public async Task CrearComanda_ConCantidadNegativa_DeberiaRetornarError()
        {
            // Arrange
            var mesa = await _fixture.Context.Mesas.FirstAsync();
            var plato = await _fixture.Context.Platos.FirstAsync();
            
            // Asegurarse de que el plato esté disponible
            plato.Disponible = true;
            _fixture.Context.Update(plato);
            await _fixture.Context.SaveChangesAsync();
            
            // Limpiar el contexto para asegurar que se recargue el estado
            _fixture.Context.ChangeTracker.Clear();
            
            var createDto = new ComandaCreateDto 
            { 
                MesaId = mesa.Id,
                MeseroId = "mesero-test-1",
                Detalles = new List<ComandaDetalleCreateDto>
                {
                    new() { PlatoId = plato.Id, Cantidad = -1, Observaciones = "Cantidad negativa" }
                },
                Observaciones = "Test cantidad negativa"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                async () => await _comandaService.CreateAsync(createDto)
            );

            exception.Message.Should().Contain("cantidad debe ser mayor");
        }

        [Fact]
        public async Task CrearComanda_ConPlatoInexistente_DeberiaRetornarError()
        {
            // Arrange
            var mesa = await _fixture.Context.Mesas.FirstAsync();
            
            var createDto = new ComandaCreateDto 
            { 
                MesaId = mesa.Id,
                MeseroId = "mesero-test-1",
                Detalles = new List<ComandaDetalleCreateDto>
                {
                    new() { PlatoId = 99999, Cantidad = 1, Observaciones = "Plato inexistente" }
                },
                Observaciones = "Test plato inexistente"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                async () => await _comandaService.CreateAsync(createDto)
            );

            exception.Message.Should().Contain("El plato no existe");
        }

        [Fact]
        public async Task CrearComanda_ConStockInsuficiente_DeberiaRetornarError()
        {
            // Arrange
            var mesa = await _fixture.Context.Mesas.FirstAsync();
            var plato = await _fixture.Context.Platos.FirstAsync();
            
            // Asegurarse de que el plato tenga stock limitado y esté disponible
            plato.Stock = 5;
            plato.Disponible = true;
            _fixture.Context.Update(plato);
            await _fixture.Context.SaveChangesAsync();
            
            // Limpiar el contexto para asegurar que se recargue el estado
            _fixture.Context.ChangeTracker.Clear();
            
            var createDto = new ComandaCreateDto 
            { 
                MesaId = mesa.Id,
                MeseroId = "mesero-test-1",
                Detalles = new List<ComandaDetalleCreateDto>
                {
                    new() { PlatoId = plato.Id, Cantidad = 6, Observaciones = "Cantidad mayor al stock" }
                },
                Observaciones = "Test stock insuficiente"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                async () => await _comandaService.CreateAsync(createDto)
            );

            exception.Message.Should().Contain("Stock insuficiente");
        }

        [Fact]
        public async Task CrearComanda_ConPlatoNoDisponible_DeberiaRetornarError()
        {
            // Arrange
            var mesa = await _fixture.Context.Mesas.FirstAsync();
            var plato = await _fixture.Context.Platos.FirstAsync();
            
            // Modificar el plato para que no esté disponible
            plato.Disponible = false;
            _fixture.Context.Update(plato);
            await _fixture.Context.SaveChangesAsync();
            
            // Limpiar el contexto para asegurar que se recargue el estado
            _fixture.Context.ChangeTracker.Clear();
            
            var createDto = new ComandaCreateDto 
            { 
                MesaId = mesa.Id,
                MeseroId = "mesero-test-1",
                Detalles = new List<ComandaDetalleCreateDto>
                {
                    new() { PlatoId = plato.Id, Cantidad = 1, Observaciones = "Plato no disponible" }
                },
                Observaciones = "Test plato no disponible"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                async () => await _comandaService.CreateAsync(createDto)
            );

            exception.Message.Should().Contain("El plato no está disponible");
        }

        [Fact]
        public async Task CrearComanda_ConDemasiadosDetalles_DeberiaRetornarError()
        {
            // Arrange
            var mesa = await _fixture.Context.Mesas.FirstAsync();
            var plato = await _fixture.Context.Platos.FirstAsync();
            
            var detalles = new List<ComandaDetalleCreateDto>();
            // Usamos un límite más realista de 50 detalles
            for (int i = 0; i < 51; i++) 
            {
                detalles.Add(new() 
                { 
                    PlatoId = plato.Id, 
                    Cantidad = 1, 
                    Observaciones = $"Detalle {i}" 
                });
            }
            
            var createDto = new ComandaCreateDto 
            { 
                MesaId = mesa.Id,
                MeseroId = "mesero-test-1",
                Detalles = detalles,
                Observaciones = "Test demasiados detalles"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                async () => await _comandaService.CreateAsync(createDto)
            );

            exception.Message.Should().Contain("La comanda no puede tener más de 50 detalles");
        }
    }
} 