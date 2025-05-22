namespace RestaurantePro.Domain.Operaciones.Comandas.ValueObjects
{
    /// <summary>
    /// Value Object que representa una personalización aplicada a un ítem de comanda.
    /// Inmutable y representa un concepto del dominio relacionado con la modificación 
    /// de ingredientes en un producto.
    /// </summary>
    public class PersonalizacionItem : ValueObject
    {
        /// <summary>
        /// ID del ingrediente que se está personalizando
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nombre del ingrediente (para mejor visualización)
        /// </summary>
        public string NombreIngrediente { get; }
        
        /// <summary>
        /// Tipo de acción que se aplica sobre el ingrediente
        /// </summary>
        public AccionPersonalizacion Accion { get; }
        
        /// <summary>
        /// Cantidad del ingrediente (cuando aplica)
        /// </summary>
        public decimal Cantidad { get; }
        
        /// <summary>
        /// Precio adicional por esta personalización (cuando aplica)
        /// </summary>
        public decimal PrecioAdicional { get; }
        
        /// <summary>
        /// ID del ingrediente de sustitución (sólo cuando Accion = Sustituir)
        /// </summary>
        public Guid? IngredienteSustitucionId { get; }
        
        /// <summary>
        /// Nombre del ingrediente de sustitución (sólo cuando Accion = Sustituir)
        /// </summary>
        public string? NombreIngredienteSustitucion { get; }

        private PersonalizacionItem(
            Guid ingredienteId, 
            string nombreIngrediente, 
            AccionPersonalizacion accion, 
            decimal cantidad, 
            decimal precioAdicional,
            Guid? ingredienteSustitucionId = null,
            string? nombreIngredienteSustitucion = null)
        {
            IngredienteId = ingredienteId;
            NombreIngrediente = nombreIngrediente;
            Accion = accion;
            Cantidad = cantidad;
            PrecioAdicional = precioAdicional;
            IngredienteSustitucionId = ingredienteSustitucionId;
            NombreIngredienteSustitucion = nombreIngredienteSustitucion;
        }
        
        /// <summary>
        /// Crea una personalización para agregar más cantidad de un ingrediente
        /// </summary>
        public static PersonalizacionItem CrearAgregar(
            Guid ingredienteId, 
            string nombreIngrediente, 
            decimal cantidad, 
            decimal precioAdicional = 0)
        {
            if (ingredienteId == Guid.Empty)
                throw new ArgumentException("El ID del ingrediente no puede estar vacío", nameof(ingredienteId));
                
            if (string.IsNullOrWhiteSpace(nombreIngrediente))
                throw new ArgumentException("El nombre del ingrediente no puede estar vacío", nameof(nombreIngrediente));
                
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(cantidad));
                
            if (precioAdicional < 0)
                throw new ArgumentException("El precio adicional no puede ser negativo", nameof(precioAdicional));
                
            return new PersonalizacionItem(
                ingredienteId, 
                nombreIngrediente, 
                AccionPersonalizacion.Agregar, 
                cantidad, 
                precioAdicional);
        }
        
        /// <summary>
        /// Crea una personalización para quitar un ingrediente
        /// </summary>
        public static PersonalizacionItem CrearQuitar(
            Guid ingredienteId, 
            string nombreIngrediente)
        {
            if (ingredienteId == Guid.Empty)
                throw new ArgumentException("El ID del ingrediente no puede estar vacío", nameof(ingredienteId));
                
            if (string.IsNullOrWhiteSpace(nombreIngrediente))
                throw new ArgumentException("El nombre del ingrediente no puede estar vacío", nameof(nombreIngrediente));
                
            return new PersonalizacionItem(
                ingredienteId, 
                nombreIngrediente, 
                AccionPersonalizacion.Quitar, 
                0, 
                0);
        }
        
        /// <summary>
        /// Crea una personalización para sustituir un ingrediente por otro
        /// </summary>
        public static PersonalizacionItem CrearSustituir(
            Guid ingredienteId, 
            string nombreIngrediente,
            Guid ingredienteSustitucionId,
            string nombreIngredienteSustitucion,
            decimal cantidad = 1,
            decimal precioAdicional = 0)
        {
            if (ingredienteId == Guid.Empty)
                throw new ArgumentException("El ID del ingrediente no puede estar vacío", nameof(ingredienteId));
                
            if (string.IsNullOrWhiteSpace(nombreIngrediente))
                throw new ArgumentException("El nombre del ingrediente no puede estar vacío", nameof(nombreIngrediente));
                
            if (ingredienteSustitucionId == Guid.Empty)
                throw new ArgumentException("El ID del ingrediente de sustitución no puede estar vacío", nameof(ingredienteSustitucionId));
                
            if (string.IsNullOrWhiteSpace(nombreIngredienteSustitucion))
                throw new ArgumentException("El nombre del ingrediente de sustitución no puede estar vacío", nameof(nombreIngredienteSustitucion));
                
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(cantidad));
                
            if (precioAdicional < 0)
                throw new ArgumentException("El precio adicional no puede ser negativo", nameof(precioAdicional));
                
            return new PersonalizacionItem(
                ingredienteId, 
                nombreIngrediente, 
                AccionPersonalizacion.Sustituir, 
                cantidad, 
                precioAdicional,
                ingredienteSustitucionId,
                nombreIngredienteSustitucion);
        }
        
        /// <summary>
        /// Devuelve una descripción textual de la personalización para mostrar en interfaces
        /// </summary>
        public string ObtenerDescripcion()
        {
            return Accion switch
            {
                AccionPersonalizacion.Agregar => $"Extra {NombreIngrediente} ({Cantidad})",
                AccionPersonalizacion.Quitar => $"Sin {NombreIngrediente}",
                AccionPersonalizacion.Sustituir => $"Sustituir {NombreIngrediente} por {NombreIngredienteSustitucion} ({Cantidad})",
                _ => $"Personalización desconocida: {NombreIngrediente}"
            };
        }

        /// <summary>
        /// Determina si esta personalización afecta al precio
        /// </summary>
        public bool AfectaPrecio() => PrecioAdicional > 0;
        
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return IngredienteId;
            yield return NombreIngrediente;
            yield return Accion;
            yield return Cantidad;
            yield return PrecioAdicional;
            
            if (IngredienteSustitucionId.HasValue)
                yield return IngredienteSustitucionId.Value;
                
            if (!string.IsNullOrEmpty(NombreIngredienteSustitucion))
                yield return NombreIngredienteSustitucion;
        }
    }
} 
