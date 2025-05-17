namespace RestaurantePro.Domain.UnitTests.Operaciones.Comandas.ValueObjects
{
    using Xunit;
    using FluentAssertions;
    using System;
    using RestaurantePro.Domain.Operaciones.Comandas.ValueObjects;

    public class TotalComandaTests
    {
        [Fact]
        public void Crear_ConValoresPositivos_DebeCrearTotalComanda()
        {
            // Arrange
            decimal subtotal = 100m;
            decimal impuestos = 19m;
            decimal totalEsperado = 119m;

            // Act
            var totalComanda = TotalComanda.Crear(subtotal, impuestos);

            // Assert
            totalComanda.Should().NotBeNull();
            totalComanda.Subtotal.Should().Be(subtotal);
            totalComanda.Impuestos.Should().Be(impuestos);
            totalComanda.Total.Should().Be(totalEsperado);
        }

        [Fact]
        public void Crear_ConValoresCero_DebeCrearTotalComanda()
        {
            // Arrange
            decimal subtotal = 0m;
            decimal impuestos = 0m;
            decimal totalEsperado = 0m;

            // Act
            var totalComanda = TotalComanda.Crear(subtotal, impuestos);

            // Assert
            totalComanda.Should().NotBeNull();
            totalComanda.Subtotal.Should().Be(subtotal);
            totalComanda.Impuestos.Should().Be(impuestos);
            totalComanda.Total.Should().Be(totalEsperado);
        }

        [Fact]
        public void Crear_ConSubtotalNegativo_DebeLanzarArgumentException()
        {
            // Arrange
            decimal subtotal = -10m;
            decimal impuestos = 5m;

            // Act
            Action action = () => TotalComanda.Crear(subtotal, impuestos);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*subtotal*negativo*");
        }

        [Fact]
        public void Crear_ConImpuestosNegativos_DebeLanzarArgumentException()
        {
            // Arrange
            decimal subtotal = 100m;
            decimal impuestos = -10m;

            // Act
            Action action = () => TotalComanda.Crear(subtotal, impuestos);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*impuestos*negativos*");
        }

        [Fact]
        public void Equals_MismosValores_DebeSerIguales()
        {
            // Arrange
            var total1 = TotalComanda.Crear(100m, 19m);
            var total2 = TotalComanda.Crear(100m, 19m);

            // Act & Assert
            total1.Should().Be(total2);
            (total1 == total2).Should().BeTrue();
            (total1 != total2).Should().BeFalse();
            total1.GetHashCode().Should().Be(total2.GetHashCode());
        }

        [Fact]
        public void Equals_DiferentesValores_DebeSerDiferentes()
        {
            // Arrange
            var total1 = TotalComanda.Crear(100m, 19m);
            var total2 = TotalComanda.Crear(200m, 38m);

            // Act & Assert
            total1.Should().NotBe(total2);
            (total1 == total2).Should().BeFalse();
            (total1 != total2).Should().BeTrue();
        }

        [Fact]
        public void ToString_DebeIncluirValores()
        {
            // Arrange
            decimal subtotal = 100m;
            decimal impuestos = 19m;
            var totalComanda = TotalComanda.Crear(subtotal, impuestos);

            // Act
            string resultado = totalComanda.ToString();

            // Assert
            resultado.Should().Contain(subtotal.ToString("C", null));
            resultado.Should().Contain(impuestos.ToString("C", null));
            resultado.Should().Contain(119m.ToString("C", null));
        }
    }
} 
