namespace RestaurantePro.Domain.Core.SharedKernel.ValueObjects
{
    /// <summary>
    /// Objeto de valor que representa una dirección postal
    /// </summary>
    public class Address : ValueObject
    {
        /// <summary>
        /// Calle y número
        /// </summary>
        public string Calle { get; }
        
        /// <summary>
        /// Colonia/Barrio
        /// </summary>
        public string Colonia { get; }
        
        /// <summary>
        /// Ciudad
        /// </summary>
        public string Ciudad { get; }
        
        /// <summary>
        /// Estado/Provincia
        /// </summary>
        public string Estado { get; }
        
        /// <summary>
        /// Código postal
        /// </summary>
        public string CodigoPostal { get; }
        
        /// <summary>
        /// País
        /// </summary>
        public string Pais { get; }
        
        /// <summary>
        /// Referencias adicionales para facilitar la ubicación
        /// </summary>
        public string Referencias { get; }

        private Address(
            string calle, 
            string colonia, 
            string ciudad, 
            string estado, 
            string codigoPostal, 
            string pais,
            string referencias)
        {
            Calle = calle;
            Colonia = colonia;
            Ciudad = ciudad;
            Estado = estado;
            CodigoPostal = codigoPostal;
            Pais = pais;
            Referencias = referencias;
        }

        /// <summary>
        /// Crea una nueva instancia de Address
        /// </summary>
        /// <param name="calle">Calle y número</param>
        /// <param name="colonia">Colonia/Barrio</param>
        /// <param name="ciudad">Ciudad</param>
        /// <param name="estado">Estado/Provincia</param>
        /// <param name="codigoPostal">Código postal</param>
        /// <param name="pais">País</param>
        /// <param name="referencias">Referencias adicionales</param>
        /// <returns>Objeto Address validado</returns>
        /// <exception cref="ArgumentException">Si alguno de los datos requeridos no es válido</exception>
        public static Address Create(
            string calle,
            string colonia,
            string ciudad,
            string estado,
            string codigoPostal,
            string pais,
            string referencias = null)
        {
            if (string.IsNullOrWhiteSpace(calle))
                throw new ArgumentException("La calle no puede estar vacía", nameof(calle));
                
            if (string.IsNullOrWhiteSpace(ciudad))
                throw new ArgumentException("La ciudad no puede estar vacía", nameof(ciudad));
                
            if (string.IsNullOrWhiteSpace(estado))
                throw new ArgumentException("El estado no puede estar vacío", nameof(estado));
                
            if (string.IsNullOrWhiteSpace(pais))
                throw new ArgumentException("El país no puede estar vacío", nameof(pais));

            return new Address(
                calle.Trim(),
                string.IsNullOrWhiteSpace(colonia) ? string.Empty : colonia.Trim(),
                ciudad.Trim(),
                estado.Trim(),
                string.IsNullOrWhiteSpace(codigoPostal) ? string.Empty : codigoPostal.Trim(),
                pais.Trim(),
                string.IsNullOrWhiteSpace(referencias) ? string.Empty : referencias.Trim()
            );
        }
        
        /// <summary>
        /// Crea una dirección a partir de un texto libre
        /// </summary>
        /// <param name="direccionCompleta">Texto completo de la dirección</param>
        /// <returns>Objeto Address con todos los campos combinados en calle</returns>
        public static Address CreateFromFreeText(string direccionCompleta)
        {
            if (string.IsNullOrWhiteSpace(direccionCompleta))
                throw new ArgumentException("La dirección no puede estar vacía", nameof(direccionCompleta));
                
            return new Address(
                direccionCompleta.Trim(),
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty
            );
        }

        /// <summary>
        /// Devuelve la dirección formateada como una cadena de texto
        /// </summary>
        public override string ToString()
        {
            var parts = new List<string>();
            
            if (!string.IsNullOrEmpty(Calle)) parts.Add(Calle);
            if (!string.IsNullOrEmpty(Colonia)) parts.Add(Colonia);
            if (!string.IsNullOrEmpty(Ciudad)) parts.Add(Ciudad);
            if (!string.IsNullOrEmpty(Estado)) parts.Add(Estado);
            if (!string.IsNullOrEmpty(CodigoPostal)) parts.Add($"C.P. {CodigoPostal}");
            if (!string.IsNullOrEmpty(Pais)) parts.Add(Pais);
            
            return string.Join(", ", parts);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Calle;
            yield return Colonia;
            yield return Ciudad;
            yield return Estado;
            yield return CodigoPostal;
            yield return Pais;
            yield return Referencias;
        }
    }
} 