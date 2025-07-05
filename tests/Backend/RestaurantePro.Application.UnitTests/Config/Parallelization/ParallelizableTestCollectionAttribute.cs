using Xunit;

namespace RestaurantePro.Application.UnitTests.Config.Parallelization
{
    /// <summary>
    /// Esta colección permite que las clases de prueba que la usan se ejecuten en paralelo con otras colecciones
    /// Añade este atributo a las clases de prueba para mejorar el rendimiento
    /// </summary>
    [CollectionDefinition("Parallelizable Tests", DisableParallelization = false)]
    public class ParallelizableTestCollection { }
} 