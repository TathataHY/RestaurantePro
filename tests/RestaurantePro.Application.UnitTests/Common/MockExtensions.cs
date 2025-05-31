using Microsoft.EntityFrameworkCore;
using Moq;

namespace RestaurantePro.Application.UnitTests.Common;

/// <summary>
/// Extensiones para configurar mocks de DbSet que funcionen correctamente con Entity Framework
/// </summary>
public static class MockExtensions
{
    /// <summary>
    /// Configura un mock de DbSet para trabajar correctamente con FirstOrDefaultAsync, Where, etc.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad</typeparam>
    /// <param name="mockDbSet">Mock del DbSet</param>
    /// <param name="data">Datos de prueba</param>
    public static void SetupMockDbSet<T>(this Mock<DbSet<T>> mockDbSet, IEnumerable<T> data) where T : class
    {
        var queryableData = data.AsQueryable();
        
        // Configurar IQueryable básico
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryableData.Provider);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableData.Expression);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryableData.GetEnumerator());
    }
    
    /// <summary>
    /// Configura un mock de DbSet vacío
    /// </summary>
    /// <typeparam name="T">Tipo de entidad</typeparam>
    /// <param name="mockDbSet">Mock del DbSet</param>
    public static void SetupEmptyMockDbSet<T>(this Mock<DbSet<T>> mockDbSet) where T : class
    {
        mockDbSet.SetupMockDbSet(Enumerable.Empty<T>());
    }
} 