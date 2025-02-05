using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FluentAssertions;
using RestaurantePro.Core.Validators;
using RestaurantePro.Core.Interfaces;
using RestaurantePro.Core.Commands;
using RestaurantePro.Core.DTOs.Comanda;
using RestaurantePro.Core.Entities;

namespace RestaurantePro.Tests.Validators
{
    public class ComandaValidatorTests
    {
        private readonly ComandaValidator _validator;
        private readonly Mock<IUnitOfWork> _unitOfWork;

        public ComandaValidatorTests()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _validator = new ComandaValidator(_unitOfWork.Object);
        }

        [Fact]
        public async Task Validate_CuandoMesaNoExiste_DebeRetornarError()
        {
            // Arrange
            var comanda = new Comanda
            {
                MesaId = 999,
                Detalles = new List<ComandaDetalle>
                {
                    new() { PlatoId = 1, Cantidad = 1 }
                }
            };

            _unitOfWork.Setup(x => x.Mesas.GetByIdAsync(999))
                .ReturnsAsync((Mesa)null);
            
            _unitOfWork.Setup(x => x.Platos.GetByIdAsync(1))
                .ReturnsAsync(new Plato { Id = 1, Nombre = "Test Plato" });

            // Act
            var result = await _validator.ValidateAsync(comanda);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => 
                x.PropertyName == nameof(comanda.MesaId));
        }

        [Fact]
        public async Task Validate_CuandoCantidadNegativa_DebeRetornarError()
        {
            // Arrange
            var comanda = new Comanda
            {
                MesaId = 1,
                Detalles = new List<ComandaDetalle>
                {
                    new() { PlatoId = 1, Cantidad = -1 }
                }
            };

            _unitOfWork.Setup(x => x.Mesas.GetByIdAsync(1))
                .ReturnsAsync(new Mesa());
            
            _unitOfWork.Setup(x => x.Platos.GetByIdAsync(1))
                .ReturnsAsync(new Plato { 
                    Id = 1, 
                    Nombre = "Test Plato",
                    Disponible = true,
                    Stock = 10
                });

            // Act
            var result = await _validator.ValidateAsync(comanda);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(error => 
                error.ErrorMessage == "La cantidad debe ser mayor que 0");
        }

        [Fact]
        public async Task Validate_CuandoNoHayDetalles_DebeRetornarError()
        {
            // Arrange
            var comanda = new Comanda
            {
                MesaId = 1,
                Detalles = new List<ComandaDetalle>()
            };

            _unitOfWork.Setup(x => x.Mesas.GetByIdAsync(1))
                .ReturnsAsync(new Mesa());

            // Act
            var result = await _validator.ValidateAsync(comanda);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => 
                x.PropertyName == "Detalles" && 
                x.ErrorMessage.Contains("La comanda debe tener al menos un detalle"));
        }

        [Fact]
        public async Task Validate_CuandoPlatoNoDisponible_DebeRetornarError()
        {
            // Arrange
            var comanda = new Comanda
            {
                MesaId = 1,
                Detalles = new List<ComandaDetalle>
                {
                    new() { PlatoId = 1, Cantidad = 1 }
                }
            };

            _unitOfWork.Setup(x => x.Mesas.GetByIdAsync(1))
                .ReturnsAsync(new Mesa());
            
            _unitOfWork.Setup(x => x.Platos.GetByIdAsync(1))
                .ReturnsAsync(new Plato 
                { 
                    Id = 1, 
                    Nombre = "Test Plato",
                    Disponible = false,
                    Stock = 10
                });

            // Act
            var result = await _validator.ValidateAsync(comanda);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(error => 
                error.ErrorMessage == "El plato no está disponible");
        }

        [Fact]
        public async Task Validate_CuandoStockInsuficiente_DebeRetornarError()
        {
            // Arrange
            var comanda = new Comanda
            {
                MesaId = 1,
                Detalles = new List<ComandaDetalle>
                {
                    new() { PlatoId = 1, Cantidad = 11 }
                }
            };

            _unitOfWork.Setup(x => x.Mesas.GetByIdAsync(1))
                .ReturnsAsync(new Mesa());
            
            _unitOfWork.Setup(x => x.Platos.GetByIdAsync(1))
                .ReturnsAsync(new Plato 
                { 
                    Id = 1, 
                    Nombre = "Test Plato",
                    Disponible = true,
                    Stock = 10
                });

            // Act
            var result = await _validator.ValidateAsync(comanda);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(error => 
                error.ErrorMessage.Contains("Stock insuficiente"));
        }

        [Fact]
        public async Task Validate_CuandoDemasiadosDetalles_DebeRetornarError()
        {
            // Arrange
            var detalles = new List<ComandaDetalle>();
            for (int i = 0; i < 51; i++)
            {
                detalles.Add(new() { PlatoId = 1, Cantidad = 1 });
            }

            var comanda = new Comanda
            {
                MesaId = 1,
                Detalles = detalles
            };

            _unitOfWork.Setup(x => x.Mesas.GetByIdAsync(1))
                .ReturnsAsync(new Mesa());
            
            _unitOfWork.Setup(x => x.Platos.GetByIdAsync(1))
                .ReturnsAsync(new Plato { 
                    Id = 1, 
                    Nombre = "Test Plato",
                    Disponible = true,
                    Stock = 100
                });

            // Act
            var result = await _validator.ValidateAsync(comanda);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => 
                x.PropertyName == "Detalles" && 
                x.ErrorMessage == "La comanda no puede tener más de 50 detalles");
        }
    }
} 