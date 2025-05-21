namespace RestaurantePro.Domain.Core.SharedKernel.ValueObjects
{
    /// <summary>
    /// Enumeración que representa las regiones de Chile
    /// </summary>
    public enum RegionChile
    {
        Arica = 1,
        Tarapaca = 2,
        Antofagasta = 3,
        Atacama = 4,
        Coquimbo = 5,
        Valparaiso = 6,
        Metropolitana = 7,
        OHiggins = 8,
        Maule = 9,
        Nuble = 10,
        Biobio = 11,
        Araucania = 12,
        LosRios = 13,
        LosLagos = 14,
        Aysen = 15,
        Magallanes = 16
    }

    /// <summary>
    /// Objeto de valor que representa una dirección
    /// </summary>
    public class Address : ValueObject
    {
        /// <summary>
        /// Calle (línea principal de la dirección)
        /// </summary>
        public string Calle { get; }
        
        /// <summary>
        /// Número (de casa, departamento, etc.)
        /// </summary>
        public string Numero { get; }
        
        /// <summary>
        /// Información adicional (departamento, piso, etc.)
        /// </summary>
        public string? Complemento { get; }
        
        /// <summary>
        /// Comuna
        /// </summary>
        public string Comuna { get; }
        
        /// <summary>
        /// Región de Chile
        /// </summary>
        public RegionChile Region { get; }
        
        /// <summary>
        /// Código postal
        /// </summary>
        public string CodigoPostal { get; }
        
        /// <summary>
        /// País (por defecto Chile)
        /// </summary>
        public string Pais { get; }
        
        /// <summary>
        /// Referencia adicional para facilitar la ubicación
        /// </summary>
        public string? Referencia { get; }

        private Address(string calle, string numero, string? complemento, string comuna, 
                        RegionChile region, string codigoPostal, string pais, string? referencia)
        {
            Calle = calle;
            Numero = numero;
            Complemento = complemento;
            Comuna = comuna;
            Region = region;
            CodigoPostal = codigoPostal;
            Pais = pais;
            Referencia = referencia;
        }

        /// <summary>
        /// Crea una nueva dirección
        /// </summary>
        /// <param name="calle">Calle</param>
        /// <param name="numero">Número</param>
        /// <param name="complemento">Información adicional (opcional)</param>
        /// <param name="comuna">Comuna</param>
        /// <param name="region">Región de Chile</param>
        /// <param name="codigoPostal">Código postal</param>
        /// <param name="pais">País (por defecto "Chile")</param>
        /// <param name="referencia">Referencia adicional (opcional)</param>
        /// <returns>Objeto Address validado</returns>
        /// <exception cref="ArgumentException">Si los datos obligatorios no son válidos</exception>
        public static Address Create(
            string calle, 
            string numero, 
            string? complemento, 
            string comuna, 
            RegionChile region, 
            string codigoPostal,
            string pais = "Chile",
            string? referencia = null)
        {
            if (string.IsNullOrWhiteSpace(calle))
                throw new ArgumentException("La calle no puede estar vacía", nameof(calle));
                
            if (string.IsNullOrWhiteSpace(numero))
                throw new ArgumentException("El número no puede estar vacío", nameof(numero));
                
            if (string.IsNullOrWhiteSpace(comuna))
                throw new ArgumentException("La comuna no puede estar vacía", nameof(comuna));
                
            if (string.IsNullOrWhiteSpace(codigoPostal))
                throw new ArgumentException("El código postal no puede estar vacío", nameof(codigoPostal));
                
            if (string.IsNullOrWhiteSpace(pais))
                throw new ArgumentException("El país no puede estar vacío", nameof(pais));
                
            return new Address(
                calle.Trim(), 
                numero.Trim(), 
                complemento?.Trim(), 
                comuna.Trim(),
                region,
                codigoPostal.Trim(), 
                pais.Trim(),
                referencia?.Trim());
        }
        
        /// <summary>
        /// Obtiene el nombre de la región a partir del enum
        /// </summary>
        public string NombreRegion
        {
            get
            {
                return Region switch
                {
                    RegionChile.Arica => "Arica y Parinacota",
                    RegionChile.Tarapaca => "Tarapacá",
                    RegionChile.Antofagasta => "Antofagasta",
                    RegionChile.Atacama => "Atacama",
                    RegionChile.Coquimbo => "Coquimbo",
                    RegionChile.Valparaiso => "Valparaíso",
                    RegionChile.Metropolitana => "Metropolitana de Santiago",
                    RegionChile.OHiggins => "Libertador General Bernardo O'Higgins",
                    RegionChile.Maule => "Maule",
                    RegionChile.Nuble => "Ñuble",
                    RegionChile.Biobio => "Biobío",
                    RegionChile.Araucania => "La Araucanía",
                    RegionChile.LosRios => "Los Ríos",
                    RegionChile.LosLagos => "Los Lagos",
                    RegionChile.Aysen => "Aysén del General Carlos Ibáñez del Campo",
                    RegionChile.Magallanes => "Magallanes y de la Antártica Chilena",
                    _ => "Desconocida"
                };
            }
        }
        
        /// <summary>
        /// Obtiene el romano de la región
        /// </summary>
        public string NumeroRomanoRegion
        {
            get
            {
                return Region switch
                {
                    RegionChile.Arica => "XV",
                    RegionChile.Tarapaca => "I",
                    RegionChile.Antofagasta => "II",
                    RegionChile.Atacama => "III",
                    RegionChile.Coquimbo => "IV",
                    RegionChile.Valparaiso => "V",
                    RegionChile.Metropolitana => "RM",
                    RegionChile.OHiggins => "VI",
                    RegionChile.Maule => "VII",
                    RegionChile.Nuble => "XVI",
                    RegionChile.Biobio => "VIII",
                    RegionChile.Araucania => "IX",
                    RegionChile.LosRios => "XIV",
                    RegionChile.LosLagos => "X",
                    RegionChile.Aysen => "XI",
                    RegionChile.Magallanes => "XII",
                    _ => string.Empty
                };
            }
        }
        
        /// <summary>
        /// Obtiene una línea formateada de la dirección sin incluir país
        /// </summary>
        public string DireccionSimple => $"{Calle} {Numero}{(string.IsNullOrEmpty(Complemento) ? "" : $", {Complemento}")}, {Comuna}";
        
        /// <summary>
        /// Obtiene una línea formateada de la dirección completa
        /// </summary>
        public string DireccionCompleta => $"{DireccionSimple}, {NombreRegion}, {Pais}, {CodigoPostal}";

        /// <summary>
        /// Modifica el país de la dirección
        /// </summary>
        /// <param name="nuevoPais">Nuevo país</param>
        /// <returns>Nueva instancia de Address con el país modificado</returns>
        /// <exception cref="ArgumentException">Si el nuevo país está vacío</exception>
        public Address CambiarPais(string nuevoPais)
        {
            if (string.IsNullOrWhiteSpace(nuevoPais))
                throw new ArgumentException("El país no puede estar vacío", nameof(nuevoPais));
                
            return new Address(Calle, Numero, Complemento, Comuna, Region, CodigoPostal, nuevoPais.Trim(), Referencia);
        }
        
        /// <summary>
        /// Modifica la referencia de la dirección
        /// </summary>
        /// <param name="nuevaReferencia">Nueva referencia</param>
        /// <returns>Nueva instancia de Address con la referencia modificada</returns>
        public Address CambiarReferencia(string? nuevaReferencia)
        {
            return new Address(Calle, Numero, Complemento, Comuna, Region, CodigoPostal, Pais, nuevaReferencia?.Trim());
        }
        
        /// <summary>
        /// Modifica el complemento de la dirección
        /// </summary>
        /// <param name="nuevoComplemento">Nuevo complemento</param>
        /// <returns>Nueva instancia de Address con el complemento modificado</returns>
        public Address CambiarComplemento(string? nuevoComplemento)
        {
            return new Address(Calle, Numero, nuevoComplemento?.Trim(), Comuna, Region, CodigoPostal, Pais, Referencia);
        }

        /// <summary>
        /// Obtiene la dirección formateada en múltiples líneas
        /// </summary>
        public override string ToString()
        {
            return DireccionCompleta;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Calle.ToUpperInvariant();
            yield return Numero.ToUpperInvariant();
            yield return (Complemento ?? string.Empty).ToUpperInvariant();
            yield return Comuna.ToUpperInvariant();
            yield return Region;
            yield return CodigoPostal.ToUpperInvariant();
            yield return Pais.ToUpperInvariant();
            yield return (Referencia ?? string.Empty).ToUpperInvariant();
        }
    }
} 