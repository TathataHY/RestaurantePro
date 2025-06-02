using Microsoft.EntityFrameworkCore;
using Moq;

namespace RestaurantePro.Application.UnitTests.Common;

/// <summary>
/// Helper para crear DbSet mocks en los tests unitarios
/// </summary>
public static class MockDbSetHelper
{
    /// <summary>
    /// Crea un mock de DbSet con datos específicos
    /// </summary>
    /// <typeparam name="T">Tipo de entidad</typeparam>
    /// <param name="data">Datos a incluir en el DbSet mockeado</param>
    /// <returns>Mock de DbSet configurado</returns>
    public static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        
        return mockSet;
    }

    /// <summary>
    /// Crea un mock de DbSet vacío
    /// </summary>
    /// <typeparam name="T">Tipo de entidad</typeparam>
    /// <returns>Mock de DbSet vacío</returns>
    public static Mock<DbSet<T>> CreateEmptyMockDbSet<T>() where T : class
    {
        return CreateMockDbSet(new List<T>().AsQueryable());
    }
} 