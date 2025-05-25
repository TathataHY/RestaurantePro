namespace RestaurantePro.Domain.Core.SharedKernel.Services
{
    /// <summary>
    /// Implementación por defecto del servicio de fecha y hora
    /// </summary>
    public class DateTimeService : IDateTimeService
    {
        /// <summary>
        /// Fecha y hora actuales del sistema
        /// </summary>
        public DateTime Now => DateTime.Now;
        
        /// <summary>
        /// Fecha actual del sistema (sin hora)
        /// </summary>
        public DateTime Today => DateTime.Today;
        
        /// <summary>
        /// Fecha y hora actuales del sistema en UTC
        /// </summary>
        public DateTime UtcNow => DateTime.UtcNow;
    }
    
    /// <summary>
    /// Implementación del servicio de fecha y hora para pruebas
    /// Permite establecer una fecha/hora fija para simular diferentes escenarios
    /// </summary>
    public class MockDateTimeService : IDateTimeService
    {
        private DateTime _now;
        
        /// <summary>
        /// Constructor que permite establecer una fecha/hora específica
        /// </summary>
        public MockDateTimeService(DateTime now)
        {
            _now = now;
        }
        
        /// <summary>
        /// Fecha y hora fijas establecidas para pruebas
        /// </summary>
        public DateTime Now => _now;
        
        /// <summary>
        /// Fecha fija establecida para pruebas (sin hora)
        /// </summary>
        public DateTime Today => _now.Date;
        
        /// <summary>
        /// Fecha y hora fijas establecidas para pruebas (en UTC)
        /// </summary>
        public DateTime UtcNow => _now.ToUniversalTime();
        
        /// <summary>
        /// Permite establecer una nueva fecha/hora para las pruebas
        /// </summary>
        public void SetDateTime(DateTime now)
        {
            _now = now;
        }
        
        /// <summary>
        /// Avanza la fecha/hora en la cantidad especificada
        /// </summary>
        public void Advance(TimeSpan timeSpan)
        {
            _now = _now.Add(timeSpan);
        }
    }
} 