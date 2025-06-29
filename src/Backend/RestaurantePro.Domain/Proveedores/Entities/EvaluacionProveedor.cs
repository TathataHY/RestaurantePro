namespace RestaurantePro.Domain.Proveedores.Entities
{
    /// <summary>
    /// Entidad que representa una evaluación de un proveedor
    /// </summary>
    public class EvaluacionProveedor : EntityBase
    {
        /// <summary>
        /// ID del proveedor evaluado
        /// </summary>
        public Guid ProveedorId { get; private set; }
        
        /// <summary>
        /// ID del usuario que realizó la evaluación
        /// </summary>
        public Guid EvaluadorId { get; private set; }
        
        /// <summary>
        /// Calificación general del proveedor (1-5)
        /// </summary>
        public int CalificacionGeneral { get; private set; }
        
        /// <summary>
        /// Calificación de calidad de productos (1-5)
        /// </summary>
        public int CalificacionCalidad { get; private set; }
        
        /// <summary>
        /// Calificación de puntualidad en entregas (1-5)
        /// </summary>
        public int CalificacionPuntualidad { get; private set; }
        
        /// <summary>
        /// Calificación de comunicación (1-5)
        /// </summary>
        public int CalificacionComunicacion { get; private set; }
        
        /// <summary>
        /// Calificación de precios (1-5)
        /// </summary>
        public int CalificacionPrecios { get; private set; }
        
        /// <summary>
        /// Comentarios de la evaluación
        /// </summary>
        public string Comentarios { get; private set; }
        
        /// <summary>
        /// Fecha de la evaluación
        /// </summary>
        public DateTime FechaEvaluacion { get; private set; }
        
        /// <summary>
        /// Indica si la evaluación está activa
        /// </summary>
        public bool Activa { get; private set; }
        
        /// <summary>
        /// Fecha de la última actualización
        /// </summary>
        public DateTime FechaActualizacion { get; private set; }
        
        /// <summary>
        /// Promedio ponderado de todas las calificaciones
        /// </summary>
        public decimal PromedioPonderado => CalcularPromedioPonderado();
        
        /// <summary>
        /// Constructor protegido para EF Core
        /// </summary>
        protected EvaluacionProveedor() { }
        
        /// <summary>
        /// Constructor para crear una nueva evaluación
        /// </summary>
        private EvaluacionProveedor(
            Guid proveedorId,
            Guid evaluadorId,
            int calificacionGeneral,
            int calificacionCalidad,
            int calificacionPuntualidad,
            int calificacionComunicacion,
            int calificacionPrecios,
            string comentarios)
        {
            ProveedorId = proveedorId;
            EvaluadorId = evaluadorId;
            CalificacionGeneral = calificacionGeneral;
            CalificacionCalidad = calificacionCalidad;
            CalificacionPuntualidad = calificacionPuntualidad;
            CalificacionComunicacion = calificacionComunicacion;
            CalificacionPrecios = calificacionPrecios;
            Comentarios = comentarios ?? string.Empty;
            FechaEvaluacion = DateTime.Now;
            FechaActualizacion = DateTime.Now;
            Activa = true;
            
            AddDomainEvent(new Events.EvaluacionProveedor.EvaluacionProveedorCreada(
                Id, 
                proveedorId, 
                evaluadorId, 
                CalcularPromedioPonderado()));
            
            ValidarInvariantes();
        }
        
        /// <summary>
        /// Factory method para crear una nueva evaluación de proveedor
        /// </summary>
        public static EvaluacionProveedor Crear(
            Guid proveedorId,
            Guid evaluadorId,
            int calificacionGeneral,
            int calificacionCalidad,
            int calificacionPuntualidad,
            int calificacionComunicacion,
            int calificacionPrecios,
            string comentarios = "")
        {
            // Validaciones
            if (proveedorId == Guid.Empty)
                throw new ArgumentException("El ID del proveedor es obligatorio", nameof(proveedorId));
                
            if (evaluadorId == Guid.Empty)
                throw new ArgumentException("El ID del evaluador es obligatorio", nameof(evaluadorId));
                
            if (calificacionGeneral < 1 || calificacionGeneral > 5)
                throw new ArgumentException("La calificación general debe estar entre 1 y 5", nameof(calificacionGeneral));
                
            if (calificacionCalidad < 1 || calificacionCalidad > 5)
                throw new ArgumentException("La calificación de calidad debe estar entre 1 y 5", nameof(calificacionCalidad));
                
            if (calificacionPuntualidad < 1 || calificacionPuntualidad > 5)
                throw new ArgumentException("La calificación de puntualidad debe estar entre 1 y 5", nameof(calificacionPuntualidad));
                
            if (calificacionComunicacion < 1 || calificacionComunicacion > 5)
                throw new ArgumentException("La calificación de comunicación debe estar entre 1 y 5", nameof(calificacionComunicacion));
                
            if (calificacionPrecios < 1 || calificacionPrecios > 5)
                throw new ArgumentException("La calificación de precios debe estar entre 1 y 5", nameof(calificacionPrecios));
                
            return new EvaluacionProveedor(
                proveedorId,
                evaluadorId,
                calificacionGeneral,
                calificacionCalidad,
                calificacionPuntualidad,
                calificacionComunicacion,
                calificacionPrecios,
                comentarios);
        }
        
        /// <summary>
        /// Actualiza la evaluación existente
        /// </summary>
        public void Actualizar(
            int calificacionGeneral,
            int calificacionCalidad,
            int calificacionPuntualidad,
            int calificacionComunicacion,
            int calificacionPrecios,
            string comentarios)
        {
            if (!Activa)
                throw new InvalidOperationException("No se puede actualizar una evaluación inactiva");
                
            // Validaciones
            if (calificacionGeneral < 1 || calificacionGeneral > 5)
                throw new ArgumentException("La calificación general debe estar entre 1 y 5", nameof(calificacionGeneral));
                
            if (calificacionCalidad < 1 || calificacionCalidad > 5)
                throw new ArgumentException("La calificación de calidad debe estar entre 1 y 5", nameof(calificacionCalidad));
                
            if (calificacionPuntualidad < 1 || calificacionPuntualidad > 5)
                throw new ArgumentException("La calificación de puntualidad debe estar entre 1 y 5", nameof(calificacionPuntualidad));
                
            if (calificacionComunicacion < 1 || calificacionComunicacion > 5)
                throw new ArgumentException("La calificación de comunicación debe estar entre 1 y 5", nameof(calificacionComunicacion));
                
            if (calificacionPrecios < 1 || calificacionPrecios > 5)
                throw new ArgumentException("La calificación de precios debe estar entre 1 y 5", nameof(calificacionPrecios));
                
            var promedioAnterior = CalcularPromedioPonderado();
            
            CalificacionGeneral = calificacionGeneral;
            CalificacionCalidad = calificacionCalidad;
            CalificacionPuntualidad = calificacionPuntualidad;
            CalificacionComunicacion = calificacionComunicacion;
            CalificacionPrecios = calificacionPrecios;
            Comentarios = comentarios ?? string.Empty;
            FechaActualizacion = DateTime.Now;
            
            AddDomainEvent(new Events.EvaluacionProveedor.EvaluacionProveedorActualizada(
                Id,
                ProveedorId,
                EvaluadorId,
                promedioAnterior,
                CalcularPromedioPonderado()));
                
            MarkAsModified();
            ValidarInvariantes();
        }
        
        /// <summary>
        /// Desactiva la evaluación
        /// </summary>
        public void Desactivar()
        {
            if (!Activa)
                return;
                
            Activa = false;
            FechaActualizacion = DateTime.Now;
            
            AddDomainEvent(new Events.EvaluacionProveedor.EvaluacionProveedorDesactivada(
                Id,
                ProveedorId,
                EvaluadorId));
                
            MarkAsModified();
        }
        
        /// <summary>
        /// Reactiva la evaluación
        /// </summary>
        public void Reactivar()
        {
            if (Activa)
                return;
                
            Activa = true;
            FechaActualizacion = DateTime.Now;
            
            AddDomainEvent(new Events.EvaluacionProveedor.EvaluacionProveedorReactivada(
                Id,
                ProveedorId,
                EvaluadorId));
                
            MarkAsModified();
        }
        
        /// <summary>
        /// Calcula el promedio ponderado de todas las calificaciones
        /// </summary>
        private decimal CalcularPromedioPonderado()
        {
            // Ponderación: General (30%), Calidad (25%), Puntualidad (20%), Comunicación (15%), Precios (10%)
            var promedio = (CalificacionGeneral * 0.30m) +
                          (CalificacionCalidad * 0.25m) +
                          (CalificacionPuntualidad * 0.20m) +
                          (CalificacionComunicacion * 0.15m) +
                          (CalificacionPrecios * 0.10m);
                          
            return Math.Round(promedio, 2);
        }
        
        /// <summary>
        /// Valida las invariantes de la entidad
        /// </summary>
        private void ValidarInvariantes()
        {
            if (ProveedorId == Guid.Empty)
                throw new InvalidOperationException("El ID del proveedor no puede estar vacío");
                
            if (EvaluadorId == Guid.Empty)
                throw new InvalidOperationException("El ID del evaluador no puede estar vacío");
                
            if (CalificacionGeneral < 1 || CalificacionGeneral > 5)
                throw new InvalidOperationException("La calificación general debe estar entre 1 y 5");
                
            if (CalificacionCalidad < 1 || CalificacionCalidad > 5)
                throw new InvalidOperationException("La calificación de calidad debe estar entre 1 y 5");
                
            if (CalificacionPuntualidad < 1 || CalificacionPuntualidad > 5)
                throw new InvalidOperationException("La calificación de puntualidad debe estar entre 1 y 5");
                
            if (CalificacionComunicacion < 1 || CalificacionComunicacion > 5)
                throw new InvalidOperationException("La calificación de comunicación debe estar entre 1 y 5");
                
            if (CalificacionPrecios < 1 || CalificacionPrecios > 5)
                throw new InvalidOperationException("La calificación de precios debe estar entre 1 y 5");
        }
    }
} 