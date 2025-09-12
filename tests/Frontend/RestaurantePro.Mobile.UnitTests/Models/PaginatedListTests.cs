using FluentAssertions;
using RestaurantePro.Mobile.Core.Models.Common;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Models;

/// <summary>
/// Pruebas unitarias para PaginatedList
/// </summary>
public class PaginatedListTests
{
    #region Constructor y Propiedades Iniciales

    [Fact]
    public void Constructor_DeberiaInicializarPropiedadesCorrectamente()
    {
        // Arrange & Act
        var paginatedList = new PaginatedList<string>();

        // Assert
        paginatedList.Items.Should().NotBeNull();
        paginatedList.Items.Should().BeEmpty();
        paginatedList.TotalCount.Should().Be(0);
        paginatedList.PageNumber.Should().Be(0);
        paginatedList.PageSize.Should().Be(0);
    }

    #endregion

    #region Propiedades Básicas

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(-1)]
    public void TotalCount_DeberiaEstablecerCorrectamente(int totalCount)
    {
        // Arrange
        var paginatedList = new PaginatedList<string>();

        // Act
        paginatedList.TotalCount = totalCount;

        // Assert
        paginatedList.TotalCount.Should().Be(totalCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(-1)]
    public void PageNumber_DeberiaEstablecerCorrectamente(int pageNumber)
    {
        // Arrange
        var paginatedList = new PaginatedList<string>();

        // Act
        paginatedList.PageNumber = pageNumber;

        // Assert
        paginatedList.PageNumber.Should().Be(pageNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(-1)]
    public void PageSize_DeberiaEstablecerCorrectamente(int pageSize)
    {
        // Arrange
        var paginatedList = new PaginatedList<string>();

        // Act
        paginatedList.PageSize = pageSize;

        // Assert
        paginatedList.PageSize.Should().Be(pageSize);
    }

    #endregion

    #region Items Collection

    [Fact]
    public void Items_DeberiaPermitirAgregarElementos()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>();
        var items = new List<string> { "Item 1", "Item 2", "Item 3" };

        // Act
        foreach (var item in items)
        {
            paginatedList.Items.Add(item);
        }

        // Assert
        paginatedList.Items.Should().HaveCount(3);
        paginatedList.Items.Should().BeEquivalentTo(items);
    }

    [Fact]
    public void Items_DeberiaPermitirAsignarListaCompleta()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>();
        var items = new List<string> { "Item 1", "Item 2", "Item 3" };

        // Act
        paginatedList.Items = items;

        // Assert
        paginatedList.Items.Should().BeEquivalentTo(items);
    }

    [Fact]
    public void Items_DeberiaPermitirLimpiarElementos()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>();
        paginatedList.Items.Add("Item 1");
        paginatedList.Items.Add("Item 2");

        // Act
        paginatedList.Items.Clear();

        // Assert
        paginatedList.Items.Should().BeEmpty();
    }

    #endregion

    #region TotalPages Property

    [Theory]
    [InlineData(0, 10, 0)] // 0 elementos, 10 por página = 0 páginas
    [InlineData(10, 10, 1)] // 10 elementos, 10 por página = 1 página
    [InlineData(11, 10, 2)] // 11 elementos, 10 por página = 2 páginas
    [InlineData(20, 10, 2)] // 20 elementos, 10 por página = 2 páginas
    [InlineData(21, 10, 3)] // 21 elementos, 10 por página = 3 páginas
    [InlineData(100, 25, 4)] // 100 elementos, 25 por página = 4 páginas
    [InlineData(1, 1, 1)] // 1 elemento, 1 por página = 1 página
    [InlineData(0, 0, 0)] // 0 elementos, 0 por página = 0 páginas (división por cero)
    public void TotalPages_DeberiaCalcularCorrectamente(int totalCount, int pageSize, int expectedTotalPages)
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            TotalCount = totalCount,
            PageSize = pageSize
        };

        // Act
        var totalPages = paginatedList.TotalPages;

        // Assert
        totalPages.Should().Be(expectedTotalPages);
    }

    [Fact]
    public void TotalPages_ConDivisionPorCero_DeberiaManejarCorrectamente()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            TotalCount = 10,
            PageSize = 0
        };

        // Act & Assert
        // Math.Ceiling(10 / 0) lanza excepción, pero el código actual no la maneja
        // En .NET, la división por cero de double no lanza excepción, retorna Infinity
        var act = () => paginatedList.TotalPages;
        act.Should().NotThrow(); // El código actual no lanza excepción
    }

    #endregion

    #region HasPreviousPage Property

    [Theory]
    [InlineData(1, false)] // Primera página
    [InlineData(2, true)]  // Segunda página
    [InlineData(5, true)]  // Quinta página
    [InlineData(0, false)] // Página 0
    [InlineData(-1, false)] // Página negativa
    public void HasPreviousPage_DeberiaEvaluarCorrectamente(int pageNumber, bool expectedHasPrevious)
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            PageNumber = pageNumber
        };

        // Act
        var hasPrevious = paginatedList.HasPreviousPage;

        // Assert
        hasPrevious.Should().Be(expectedHasPrevious);
    }

    #endregion

    #region HasNextPage Property

    [Theory]
    [InlineData(1, 1, 1, false)] // Página 1 de 1
    [InlineData(1, 2, 1, true)]  // Página 1 de 2
    [InlineData(2, 2, 1, false)] // Página 2 de 2
    [InlineData(1, 3, 1, true)]  // Página 1 de 3
    [InlineData(2, 3, 1, true)]  // Página 2 de 3
    [InlineData(3, 3, 1, false)] // Página 3 de 3
    [InlineData(0, 0, 0, false)] // Página 0 de 0
    public void HasNextPage_DeberiaEvaluarCorrectamente(int pageNumber, int totalCount, int pageSize, bool expectedHasNext)
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            PageNumber = pageNumber,
            TotalCount = totalCount,
            PageSize = pageSize
        };

        // Act
        var hasNext = paginatedList.HasNextPage;

        // Assert
        hasNext.Should().Be(expectedHasNext);
    }

    #endregion

    #region Casos Edge

    [Fact]
    public void TotalPages_ConValoresGrandes_DeberiaManejarCorrectamente()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            TotalCount = int.MaxValue,
            PageSize = 1
        };

        // Act
        var totalPages = paginatedList.TotalPages;

        // Assert
        totalPages.Should().Be(int.MaxValue);
    }

    [Fact]
    public void HasNextPage_ConValoresGrandes_DeberiaManejarCorrectamente()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            PageNumber = 1,
            TotalCount = int.MaxValue,
            PageSize = 1
        };

        // Act
        var hasNext = paginatedList.HasNextPage;

        // Assert
        hasNext.Should().BeTrue();
    }

    [Fact]
    public void HasPreviousPage_ConValoresGrandes_DeberiaManejarCorrectamente()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            PageNumber = int.MaxValue,
            TotalCount = int.MaxValue,
            PageSize = 1
        };

        // Act
        var hasPrevious = paginatedList.HasPreviousPage;

        // Assert
        hasPrevious.Should().BeTrue();
    }

    #endregion

    #region Tipos Genéricos

    [Fact]
    public void PaginatedList_ConTipoInt_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var paginatedList = new PaginatedList<int>();
        var items = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        paginatedList.Items = items;
        paginatedList.TotalCount = 100;
        paginatedList.PageNumber = 1;
        paginatedList.PageSize = 10;

        // Assert
        paginatedList.Items.Should().BeEquivalentTo(items);
        paginatedList.TotalCount.Should().Be(100);
        paginatedList.PageNumber.Should().Be(1);
        paginatedList.PageSize.Should().Be(10);
        paginatedList.TotalPages.Should().Be(10);
        paginatedList.HasPreviousPage.Should().BeFalse();
        paginatedList.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void PaginatedList_ConTipoObjeto_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var paginatedList = new PaginatedList<object>();
        var items = new List<object> { new { Id = 1 }, new { Id = 2 } };

        // Act
        paginatedList.Items = items;
        paginatedList.TotalCount = 50;
        paginatedList.PageNumber = 2;
        paginatedList.PageSize = 25;

        // Assert
        paginatedList.Items.Should().BeEquivalentTo(items);
        paginatedList.TotalCount.Should().Be(50);
        paginatedList.PageNumber.Should().Be(2);
        paginatedList.PageSize.Should().Be(25);
        paginatedList.TotalPages.Should().Be(2);
        paginatedList.HasPreviousPage.Should().BeTrue();
        paginatedList.HasNextPage.Should().BeFalse();
    }

    #endregion

    #region Escenarios Reales

    [Fact]
    public void PaginatedList_EscenarioRealCompleto_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>();
        var items = new List<string> { "Producto 1", "Producto 2", "Producto 3" };

        // Act
        paginatedList.Items = items;
        paginatedList.TotalCount = 25;
        paginatedList.PageNumber = 2;
        paginatedList.PageSize = 10;

        // Assert
        paginatedList.Items.Should().HaveCount(3);
        paginatedList.Items.Should().BeEquivalentTo(items);
        paginatedList.TotalCount.Should().Be(25);
        paginatedList.PageNumber.Should().Be(2);
        paginatedList.PageSize.Should().Be(10);
        paginatedList.TotalPages.Should().Be(3); // 25 / 10 = 2.5, redondeado a 3
        paginatedList.HasPreviousPage.Should().BeTrue(); // Página 2 > 1
        paginatedList.HasNextPage.Should().BeTrue(); // Página 2 < 3
    }

    [Fact]
    public void PaginatedList_EscenarioUltimaPagina_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>();
        var items = new List<string> { "Producto 21", "Producto 22", "Producto 23" };

        // Act
        paginatedList.Items = items;
        paginatedList.TotalCount = 23;
        paginatedList.PageNumber = 3;
        paginatedList.PageSize = 10;

        // Assert
        paginatedList.Items.Should().HaveCount(3);
        paginatedList.TotalCount.Should().Be(23);
        paginatedList.PageNumber.Should().Be(3);
        paginatedList.PageSize.Should().Be(10);
        paginatedList.TotalPages.Should().Be(3); // 23 / 10 = 2.3, redondeado a 3
        paginatedList.HasPreviousPage.Should().BeTrue(); // Página 3 > 1
        paginatedList.HasNextPage.Should().BeFalse(); // Página 3 = 3 (última)
    }

    [Fact]
    public void PaginatedList_EscenarioPrimeraPagina_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>();
        var items = new List<string> { "Producto 1", "Producto 2", "Producto 3" };

        // Act
        paginatedList.Items = items;
        paginatedList.TotalCount = 15;
        paginatedList.PageNumber = 1;
        paginatedList.PageSize = 10;

        // Assert
        paginatedList.Items.Should().HaveCount(3);
        paginatedList.TotalCount.Should().Be(15);
        paginatedList.PageNumber.Should().Be(1);
        paginatedList.PageSize.Should().Be(10);
        paginatedList.TotalPages.Should().Be(2); // 15 / 10 = 1.5, redondeado a 2
        paginatedList.HasPreviousPage.Should().BeFalse(); // Página 1 = 1 (primera)
        paginatedList.HasNextPage.Should().BeTrue(); // Página 1 < 2
    }

    #endregion
}
