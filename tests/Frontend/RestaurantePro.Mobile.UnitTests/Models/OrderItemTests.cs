using FluentAssertions;
using RestaurantePro.Mobile.Core.Models.DTOs;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Models;

/// <summary>
/// Pruebas unitarias para OrderItem
/// </summary>
public class OrderItemTests
{
    #region Constructor y Propiedades Iniciales

    [Fact]
    public void Constructor_DeberiaInicializarPropiedadesCorrectamente()
    {
        // Arrange & Act
        var orderItem = new OrderItem();

        // Assert
        orderItem.Id.Should().Be(0);
        orderItem.OrderNumber.Should().BeEmpty();
        orderItem.TableNumber.Should().BeEmpty();
        orderItem.CustomerName.Should().BeEmpty();
        orderItem.Total.Should().Be(0);
        orderItem.Status.Should().BeEmpty();
        orderItem.OrderTime.Should().Be(default(DateTime));
        orderItem.Items.Should().NotBeNull();
        orderItem.Items.Should().BeEmpty();
    }

    #endregion

    #region Propiedades Básicas

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Id_DeberiaEstablecerCorrectamente(int id)
    {
        // Arrange
        var orderItem = new OrderItem();

        // Act
        orderItem.Id = id;

        // Assert
        orderItem.Id.Should().Be(id);
    }

    [Theory]
    [InlineData("ORD-001")]
    [InlineData("ORD-002")]
    [InlineData("")]
    [InlineData("Orden con espacios   ")]
    public void OrderNumber_DeberiaEstablecerCorrectamente(string orderNumber)
    {
        // Arrange
        var orderItem = new OrderItem();

        // Act
        orderItem.OrderNumber = orderNumber;

        // Assert
        orderItem.OrderNumber.Should().Be(orderNumber);
    }

    [Theory]
    [InlineData("Mesa 1")]
    [InlineData("Mesa 2")]
    [InlineData("")]
    [InlineData("Mesa con espacios   ")]
    public void TableNumber_DeberiaEstablecerCorrectamente(string tableNumber)
    {
        // Arrange
        var orderItem = new OrderItem();

        // Act
        orderItem.TableNumber = tableNumber;

        // Assert
        orderItem.TableNumber.Should().Be(tableNumber);
    }

    [Theory]
    [InlineData("Cliente 1")]
    [InlineData("Cliente 2")]
    [InlineData("")]
    [InlineData("Cliente con espacios   ")]
    public void CustomerName_DeberiaEstablecerCorrectamente(string customerName)
    {
        // Arrange
        var orderItem = new OrderItem();

        // Act
        orderItem.CustomerName = customerName;

        // Assert
        orderItem.CustomerName.Should().Be(customerName);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(10.50)]
    [InlineData(100.99)]
    [InlineData(999.99)]
    [InlineData(-10.50)]
    public void Total_DeberiaEstablecerCorrectamente(decimal total)
    {
        // Arrange
        var orderItem = new OrderItem();

        // Act
        orderItem.Total = total;

        // Assert
        orderItem.Total.Should().Be(total);
    }

    [Theory]
    [InlineData("Pendiente")]
    [InlineData("En Progreso")]
    [InlineData("Completada")]
    [InlineData("Cancelada")]
    [InlineData("")]
    public void Status_DeberiaEstablecerCorrectamente(string status)
    {
        // Arrange
        var orderItem = new OrderItem();

        // Act
        orderItem.Status = status;

        // Assert
        orderItem.Status.Should().Be(status);
    }

    [Fact]
    public void OrderTime_DeberiaEstablecerCorrectamente()
    {
        // Arrange
        var orderItem = new OrderItem();
        var orderTime = DateTime.Now;

        // Act
        orderItem.OrderTime = orderTime;

        // Assert
        orderItem.OrderTime.Should().Be(orderTime);
    }

    #endregion

    #region Items Collection

    [Fact]
    public void Items_DeberiaPermitirAgregarElementos()
    {
        // Arrange
        var orderItem = new OrderItem();
        var items = new List<string> { "Item 1", "Item 2", "Item 3" };

        // Act
        foreach (var item in items)
        {
            orderItem.Items.Add(item);
        }

        // Assert
        orderItem.Items.Should().HaveCount(3);
        orderItem.Items.Should().BeEquivalentTo(items);
    }

    [Fact]
    public void Items_DeberiaPermitirLimpiarElementos()
    {
        // Arrange
        var orderItem = new OrderItem();
        orderItem.Items.Add("Item 1");
        orderItem.Items.Add("Item 2");

        // Act
        orderItem.Items.Clear();

        // Assert
        orderItem.Items.Should().BeEmpty();
    }

    [Fact]
    public void Items_DeberiaPermitirAsignarListaCompleta()
    {
        // Arrange
        var orderItem = new OrderItem();
        var items = new List<string> { "Item 1", "Item 2", "Item 3" };

        // Act
        orderItem.Items = items;

        // Assert
        orderItem.Items.Should().BeEquivalentTo(items);
    }

    #endregion

    #region TimeAgo Property

    [Fact]
    public void TimeAgo_ConTiempoActual_DeberiaRetornarHaceUnMomento()
    {
        // Arrange
        var orderItem = new OrderItem();
        orderItem.OrderTime = DateTime.Now;

        // Act
        var timeAgo = orderItem.TimeAgo;

        // Assert
        timeAgo.Should().Be("Hace un momento");
    }

    [Fact]
    public void TimeAgo_ConTiempoHace30Minutos_DeberiaRetornarMinutos()
    {
        // Arrange
        var orderItem = new OrderItem();
        orderItem.OrderTime = DateTime.Now.AddMinutes(-30);

        // Act
        var timeAgo = orderItem.TimeAgo;

        // Assert
        timeAgo.Should().Be("Hace 30 min");
    }

    [Fact]
    public void TimeAgo_ConTiempoHace2Horas_DeberiaRetornarHorasYMinutos()
    {
        // Arrange
        var orderItem = new OrderItem();
        orderItem.OrderTime = DateTime.Now.AddHours(-2).AddMinutes(-30);

        // Act
        var timeAgo = orderItem.TimeAgo;

        // Assert
        timeAgo.Should().Be("Hace 2h 30m");
    }

    [Fact]
    public void TimeAgo_ConTiempoHace3Dias_DeberiaRetornarDias()
    {
        // Arrange
        var orderItem = new OrderItem();
        orderItem.OrderTime = DateTime.Now.AddDays(-3);

        // Act
        var timeAgo = orderItem.TimeAgo;

        // Assert
        timeAgo.Should().Be("Hace 3 días");
    }

    [Fact]
    public void TimeAgo_ConTiempoFuturo_DeberiaManejarCorrectamente()
    {
        // Arrange
        var orderItem = new OrderItem();
        orderItem.OrderTime = DateTime.Now.AddMinutes(30);

        // Act
        var timeAgo = orderItem.TimeAgo;

        // Assert
        timeAgo.Should().Be("Hace un momento"); // Debería manejar tiempos negativos
    }

    #endregion

    #region StatusColor Property

    [Theory]
    [InlineData("Pendiente", "#FF6B35")]
    [InlineData("pendiente", "#FF6B35")]
    [InlineData("PENDIENTE", "#FF6B35")]
    [InlineData("En Progreso", "#4ECDC4")]
    [InlineData("en progreso", "#4ECDC4")]
    [InlineData("EN PROGRESO", "#4ECDC4")]
    [InlineData("Completada", "#45B7D1")]
    [InlineData("completada", "#45B7D1")]
    [InlineData("COMPLETADA", "#45B7D1")]
    [InlineData("Cancelada", "#FF4757")]
    [InlineData("cancelada", "#FF4757")]
    [InlineData("CANCELADA", "#FF4757")]
    [InlineData("Estado Desconocido", "#95A5A6")]
    [InlineData("", "#95A5A6")]
    [InlineData("   ", "#95A5A6")]
    public void StatusColor_DeberiaRetornarColorCorrecto(string status, string colorEsperado)
    {
        // Arrange
        var orderItem = new OrderItem();

        // Act
        orderItem.Status = status;

        // Assert
        orderItem.StatusColor.Should().NotBeNull();
        orderItem.StatusColor.ToArgbHex().Should().Be(colorEsperado);
    }

    #endregion

    #region Casos Edge

    [Fact]
    public void TimeAgo_ConTiempoExactamente1Hora_DeberiaRetornarHorasYMinutos()
    {
        // Arrange
        var orderItem = new OrderItem();
        orderItem.OrderTime = DateTime.Now.AddHours(-1);

        // Act
        var timeAgo = orderItem.TimeAgo;

        // Assert
        timeAgo.Should().Be("Hace 1h 0m");
    }

    [Fact]
    public void TimeAgo_ConTiempoExactamente1Dia_DeberiaRetornarDias()
    {
        // Arrange
        var orderItem = new OrderItem();
        orderItem.OrderTime = DateTime.Now.AddDays(-1);

        // Act
        var timeAgo = orderItem.TimeAgo;

        // Assert
        timeAgo.Should().Be("Hace 1 días");
    }

    [Fact]
    public void TimeAgo_ConTiempoExactamente59Minutos_DeberiaRetornarMinutos()
    {
        // Arrange
        var orderItem = new OrderItem();
        orderItem.OrderTime = DateTime.Now.AddMinutes(-59);

        // Act
        var timeAgo = orderItem.TimeAgo;

        // Assert
        timeAgo.Should().Be("Hace 59 min");
    }

    [Fact]
    public void TimeAgo_ConTiempoExactamente23Horas_DeberiaRetornarHorasYMinutos()
    {
        // Arrange
        var orderItem = new OrderItem();
        orderItem.OrderTime = DateTime.Now.AddHours(-23);

        // Act
        var timeAgo = orderItem.TimeAgo;

        // Assert
        timeAgo.Should().Be("Hace 23h 0m");
    }

    [Fact]
    public void StatusColor_ConStatusConEspacios_DeberiaManejarCorrectamente()
    {
        // Arrange
        var orderItem = new OrderItem();
        orderItem.Status = "En Progreso con espacios";

        // Act
        var statusColor = orderItem.StatusColor;

        // Assert
        statusColor.Should().NotBeNull();
        statusColor.ToArgbHex().Should().Be("#95A5A6"); // Color por defecto
    }

    [Fact]
    public void StatusColor_ConStatusConCaracteresEspeciales_DeberiaManejarCorrectamente()
    {
        // Arrange
        var orderItem = new OrderItem();
        orderItem.Status = "Pendiente con áéíóú";

        // Act
        var statusColor = orderItem.StatusColor;

        // Assert
        statusColor.Should().NotBeNull();
        // El código actual no normaliza caracteres especiales, usa color por defecto
        statusColor.ToArgbHex().Should().Be("#95A5A6"); // Color por defecto
    }

    #endregion

    #region Propiedades Complejas

    [Fact]
    public void OrderItem_ConTodasLasPropiedades_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var orderItem = new OrderItem();
        var orderTime = DateTime.Now.AddMinutes(-30);
        var items = new List<string> { "Pizza Margherita", "Coca Cola", "Ensalada César" };

        // Act
        orderItem.Id = 123;
        orderItem.OrderNumber = "ORD-001";
        orderItem.TableNumber = "Mesa 5";
        orderItem.CustomerName = "Juan Pérez";
        orderItem.Total = 45.50m;
        orderItem.Status = "En Progreso";
        orderItem.OrderTime = orderTime;
        orderItem.Items = items;

        // Assert
        orderItem.Id.Should().Be(123);
        orderItem.OrderNumber.Should().Be("ORD-001");
        orderItem.TableNumber.Should().Be("Mesa 5");
        orderItem.CustomerName.Should().Be("Juan Pérez");
        orderItem.Total.Should().Be(45.50m);
        orderItem.Status.Should().Be("En Progreso");
        orderItem.OrderTime.Should().Be(orderTime);
        orderItem.Items.Should().BeEquivalentTo(items);
        orderItem.TimeAgo.Should().Be("Hace 30 min");
        orderItem.StatusColor.Should().NotBeNull();
        orderItem.StatusColor.ToArgbHex().Should().Be("#4ECDC4");
    }

    #endregion
}

