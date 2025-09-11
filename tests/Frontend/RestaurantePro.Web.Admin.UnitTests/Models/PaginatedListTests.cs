using FluentAssertions;
using RestaurantePro.Web.Admin.Models;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Models;

public class PaginatedListTests
{
    // ===== PRUEBAS BÁSICAS =====

    [Fact]
    public void Constructor_ConParametrosValidos_DeberiaInicializarCorrectamente()
    {
        // Arrange
        var items = new List<string> { "item1", "item2", "item3" };
        var pageNumber = 1;
        var pageSize = 10;
        var totalCount = 25;

        // Act
        var paginatedList = new PaginatedList<string>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = 3
        };

        // Assert
        paginatedList.Items.Should().BeEquivalentTo(items);
        paginatedList.PageNumber.Should().Be(pageNumber);
        paginatedList.PageSize.Should().Be(pageSize);
        paginatedList.TotalCount.Should().Be(totalCount);
        paginatedList.TotalPages.Should().Be(3);
    }

    [Fact]
    public void Constructor_ConListaVacia_DeberiaInicializarCorrectamente()
    {
        // Arrange & Act
        var paginatedList = new PaginatedList<string>();

        // Assert
        paginatedList.Items.Should().NotBeNull();
        paginatedList.Items.Should().BeEmpty();
        paginatedList.PageNumber.Should().Be(0);
        paginatedList.PageSize.Should().Be(0);
        paginatedList.TotalCount.Should().Be(0);
        paginatedList.TotalPages.Should().Be(0);
    }

    // ===== PRUEBAS DE PROPIEDADES COMPUTADAS =====

    [Fact]
    public void HasPreviousPage_ConPrimeraPagina_DeberiaRetornarFalse()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            PageNumber = 1,
            TotalPages = 5
        };

        // Act & Assert
        paginatedList.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_ConSegundaPagina_DeberiaRetornarTrue()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            PageNumber = 2,
            TotalPages = 5
        };

        // Act & Assert
        paginatedList.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void HasPreviousPage_ConUltimaPagina_DeberiaRetornarTrue()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            PageNumber = 5,
            TotalPages = 5
        };

        // Act & Assert
        paginatedList.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_ConPrimeraPagina_DeberiaRetornarTrue()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            PageNumber = 1,
            TotalPages = 5
        };

        // Act & Assert
        paginatedList.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_ConUltimaPagina_DeberiaRetornarFalse()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            PageNumber = 5,
            TotalPages = 5
        };

        // Act & Assert
        paginatedList.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasNextPage_ConPaginaIntermedia_DeberiaRetornarTrue()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            PageNumber = 3,
            TotalPages = 5
        };

        // Act & Assert
        paginatedList.HasNextPage.Should().BeTrue();
    }

    // ===== PRUEBAS ROBUSTAS - CASOS EDGE =====

    [Fact]
    public void HasPreviousPage_ConPaginaCero_DeberiaRetornarFalse()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            PageNumber = 0,
            TotalPages = 5
        };

        // Act & Assert
        paginatedList.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void HasNextPage_ConPaginaCero_DeberiaRetornarTrue()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            PageNumber = 0,
            TotalPages = 5
        };

        // Act & Assert
        paginatedList.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void HasPreviousPage_ConPaginaNegativa_DeberiaRetornarFalse()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            PageNumber = -1,
            TotalPages = 5
        };

        // Act & Assert
        paginatedList.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void HasNextPage_ConPaginaNegativa_DeberiaRetornarTrue()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            PageNumber = -1,
            TotalPages = 5
        };

        // Act & Assert
        paginatedList.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void HasPreviousPage_ConTotalPagesCero_DeberiaRetornarFalse()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            PageNumber = 1,
            TotalPages = 0
        };

        // Act & Assert
        paginatedList.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void HasNextPage_ConTotalPagesCero_DeberiaRetornarFalse()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            PageNumber = 1,
            TotalPages = 0
        };

        // Act & Assert
        paginatedList.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_ConTotalPagesNegativo_DeberiaRetornarFalse()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            PageNumber = 1,
            TotalPages = -1
        };

        // Act & Assert
        paginatedList.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void HasNextPage_ConTotalPagesNegativo_DeberiaRetornarFalse()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            PageNumber = 1,
            TotalPages = -1
        };

        // Act & Assert
        paginatedList.HasNextPage.Should().BeFalse();
    }

    // ===== PRUEBAS ROBUSTAS - VALORES EXTREMOS =====

    [Fact]
    public void Constructor_ConValoresMaximos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var items = Enumerable.Range(1, 1000).Select(i => $"item{i}").ToList();
        var pageNumber = int.MaxValue;
        var pageSize = int.MaxValue;
        var totalCount = int.MaxValue;

        // Act
        var paginatedList = new PaginatedList<string>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = int.MaxValue
        };

        // Assert
        paginatedList.Items.Should().HaveCount(1000);
        paginatedList.PageNumber.Should().Be(int.MaxValue);
        paginatedList.PageSize.Should().Be(int.MaxValue);
        paginatedList.TotalCount.Should().Be(int.MaxValue);
        paginatedList.TotalPages.Should().Be(int.MaxValue);
    }

    [Fact]
    public void Constructor_ConValoresMinimos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var items = new List<string>();
        var pageNumber = int.MinValue;
        var pageSize = int.MinValue;
        var totalCount = int.MinValue;

        // Act
        var paginatedList = new PaginatedList<string>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = int.MinValue
        };

        // Assert
        paginatedList.Items.Should().BeEmpty();
        paginatedList.PageNumber.Should().Be(int.MinValue);
        paginatedList.PageSize.Should().Be(int.MinValue);
        paginatedList.TotalCount.Should().Be(int.MinValue);
        paginatedList.TotalPages.Should().Be(int.MinValue);
    }

    // ===== PRUEBAS ROBUSTAS - TIPOS DIFERENTES =====

    [Fact]
    public void Constructor_ConTipoInt_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var paginatedList = new PaginatedList<int>
        {
            Items = items,
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 5,
            TotalPages = 1
        };

        // Assert
        paginatedList.Items.Should().BeEquivalentTo(items);
        paginatedList.HasPreviousPage.Should().BeFalse();
        paginatedList.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void Constructor_ConTipoObject_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var items = new List<object> { "string", 123, DateTime.Now, true };

        // Act
        var paginatedList = new PaginatedList<object>
        {
            Items = items,
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 4,
            TotalPages = 1
        };

        // Assert
        paginatedList.Items.Should().HaveCount(4);
        paginatedList.HasPreviousPage.Should().BeFalse();
        paginatedList.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void Constructor_ConTipoNullable_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var items = new List<int?> { 1, null, 3, null, 5 };

        // Act
        var paginatedList = new PaginatedList<int?>
        {
            Items = items,
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 5,
            TotalPages = 1
        };

        // Assert
        paginatedList.Items.Should().HaveCount(5);
        paginatedList.Items.Should().Contain((int?)null);
        paginatedList.HasPreviousPage.Should().BeFalse();
        paginatedList.HasNextPage.Should().BeFalse();
    }

    // ===== PRUEBAS ROBUSTAS - CASOS ESPECIALES =====

    [Fact]
    public void HasPreviousPage_ConPaginaIgualATotalPages_DeberiaRetornarTrue()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            PageNumber = 5,
            TotalPages = 5
        };

        // Act & Assert
        paginatedList.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_ConPaginaIgualATotalPages_DeberiaRetornarFalse()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            PageNumber = 5,
            TotalPages = 5
        };

        // Act & Assert
        paginatedList.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_ConPaginaMayorATotalPages_DeberiaRetornarTrue()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            PageNumber = 10,
            TotalPages = 5
        };

        // Act & Assert
        paginatedList.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_ConPaginaMayorATotalPages_DeberiaRetornarFalse()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            PageNumber = 10,
            TotalPages = 5
        };

        // Act & Assert
        paginatedList.HasNextPage.Should().BeFalse();
    }

    // ===== PRUEBAS ROBUSTAS - RENDIMIENTO =====

    [Fact]
    public void Constructor_ConListaGrande_DeberiaManejarCorrectamente()
    {
        // Arrange
        var items = Enumerable.Range(1, 100000).Select(i => $"item{i}").ToList();

        // Act
        var paginatedList = new PaginatedList<string>
        {
            Items = items,
            PageNumber = 1,
            PageSize = 1000,
            TotalCount = 100000,
            TotalPages = 100
        };

        // Assert
        paginatedList.Items.Should().HaveCount(100000);
        paginatedList.HasPreviousPage.Should().BeFalse();
        paginatedList.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void PropiedadesComputadas_ConAccesoMultiple_DeberiaSerConsistente()
    {
        // Arrange
        var paginatedList = new PaginatedList<string>
        {
            PageNumber = 3,
            TotalPages = 10
        };

        // Act & Assert
        for (int i = 0; i < 100; i++)
        {
            paginatedList.HasPreviousPage.Should().BeTrue();
            paginatedList.HasNextPage.Should().BeTrue();
        }
    }
}
