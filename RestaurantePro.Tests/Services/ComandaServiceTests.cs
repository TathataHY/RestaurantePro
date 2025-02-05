using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Services;
using RestaurantePro.Core.Interfaces;
using RestaurantePro.Core.Events;
using RestaurantePro.Core.Interfaces.Services;
using AutoMapper;
using MediatR;
using RestaurantePro.Core.Enums;
using RestaurantePro.Core.DTOs.Comanda;
using RestaurantePro.Core.Exceptions;
using RestaurantePro.Core.Validators;
using FluentValidation.Results;
using System.Collections.Generic;

namespace RestaurantePro.Tests.Services
{
    public class ComandaServiceTests : TestBase
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<INotificationService> _notificationService;
        private readonly Mock<IUserContext> _userContext;
        private readonly ComandaStateManager _stateManager;
        private readonly ComandaService _sut;

        public ComandaServiceTests()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mapper = new Mock<IMapper>();
            _mediator = new Mock<IMediator>();
            _notificationService = new Mock<INotificationService>();
            _userContext = new Mock<IUserContext>();
            _stateManager = new ComandaStateManager(_unitOfWork.Object);
            
            // Crear una instancia real del validador
            var validator = new ComandaValidator(_unitOfWork.Object);

            _sut = new ComandaService(
                _unitOfWork.Object,
                _mapper.Object,
                _notificationService.Object,
                _stateManager,
                _mediator.Object,
                _userContext.Object,
                validator
            );
        }

        [Fact]
        public async Task UpdateEstado_DebePublicarEvento_YNotificarCambio()
        {
            // Arrange
            var comandaId = 1;
            var estadoInicial = EstadoComanda.Pendiente;
            var nuevoEstado = EstadoComanda.EnPreparacion;
            var usuario = "TestUser";

            var comanda = new Comanda { Id = comandaId, Estado = estadoInicial };
            _unitOfWork.Setup(x => x.Comandas.GetByIdAsync(comandaId))
                .ReturnsAsync(comanda);
            _userContext.Setup(x => x.CurrentUser).Returns(usuario);

            // Act
            await _sut.UpdateEstadoAsync(comandaId, nuevoEstado);

            // Assert
            _mediator.Verify(x => x.Publish(
                It.Is<ComandaEstadoCambiadoEvent>(e =>
                    e.ComandaId == comandaId &&
                    e.EstadoAnterior == estadoInicial &&
                    e.NuevoEstado == nuevoEstado &&
                    e.Usuario == usuario),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_DebeActualizarEstadoDeMesa()
        {
            // Arrange
            var mesaId = 1;
            var comandaDto = new ComandaCreateDto 
            { 
                MesaId = mesaId,
                MeseroId = "mesero-1",
                Detalles = new List<ComandaDetalleCreateDto>
                {
                    new() { PlatoId = 1, Cantidad = 1 }
                }
            };
            var mesa = new Mesa { Id = mesaId, Estado = EstadoMesa.Disponible };
            var comanda = new Comanda 
            { 
                MesaId = mesaId,
                Estado = EstadoComanda.Pendiente,
                FechaHora = DateTime.UtcNow,
                Detalles = new List<ComandaDetalle>
                {
                    new() { PlatoId = 1, Cantidad = 1 }
                }
            };
            
            // Configurar el UnitOfWork para el validador
            _unitOfWork.Setup(x => x.Mesas.GetByIdAsync(mesaId))
                .ReturnsAsync(mesa);
            _unitOfWork.Setup(x => x.Platos.GetByIdAsync(1))
                .ReturnsAsync(new Plato { 
                    Id = 1, 
                    Nombre = "Test", 
                    Precio = 10,
                    Disponible = true,
                    Stock = 100
                });

            _mapper.Setup(x => x.Map<Comanda>(It.IsAny<ComandaCreateDto>()))
                .Returns(comanda);
            _mapper.Setup(x => x.Map<ComandaDto>(It.IsAny<Comanda>()))
                .Returns(new ComandaDto { Id = 1 });

            _unitOfWork.Setup(x => x.Comandas.AddAsync(It.IsAny<Comanda>()))
                .Returns(Task.FromResult(comanda));
            _unitOfWork.Setup(x => x.CompleteAsync())
                .Returns(Task.FromResult(1));

            // Act
            await _sut.CreateAsync(comandaDto);

            // Assert
            _unitOfWork.Verify(x => x.Mesas.UpdateAsync(
                It.Is<Mesa>(m => 
                    m.Id == mesaId && 
                    m.Estado == EstadoMesa.Ocupada)
            ), Times.Once);
        }

        [Fact]
        public async Task UpdateEstadoAsync_CuandoComandaNoExiste_DebeLanzarNotFoundException()
        {
            // Arrange
            var comandaId = 999;
            _unitOfWork.Setup(x => x.Comandas.GetByIdAsync(comandaId))
                .ReturnsAsync((Comanda)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _sut.UpdateEstadoAsync(comandaId, EstadoComanda.EnPreparacion));
        }
    }
} 