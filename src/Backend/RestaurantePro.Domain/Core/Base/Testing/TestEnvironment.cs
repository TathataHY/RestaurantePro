using System;

namespace RestaurantePro.Domain.Core.Base.Testing
{
    /// <summary>
    /// Clase de utilidad para verificar si estamos en un entorno de pruebas
    /// </summary>
    public static class TestEnvironment
    {
        private static bool _isTestEnvironment = false;

        /// <summary>
        /// Establece si estamos en un entorno de pruebas
        /// </summary>
        public static void SetTestEnvironment(bool isTestEnvironment)
        {
            _isTestEnvironment = isTestEnvironment;
        }

        /// <summary>
        /// Verifica si estamos en un entorno de pruebas
        /// </summary>
        public static bool IsTestEnvironment => _isTestEnvironment;
    }
} 