namespace RestaurantePro.Domain.Core.SharedKernel.ValueObjects
{
    /// <summary>
    /// Objeto de valor que representa un nombre completo de persona
    /// </summary>
    public class NombreCompleto : ValueObject
    {
        /// <summary>
        /// Nombre(s) de la persona
        /// </summary>
        public string Nombres { get; }
        
        /// <summary>
        /// Apellido paterno
        /// </summary>
        public string ApellidoPaterno { get; }
        
        /// <summary>
        /// Apellido materno
        /// </summary>
        public string ApellidoMaterno { get; }
        
        /// <summary>
        /// Nombre completo formateado (Nombres + ApellidoPaterno + ApellidoMaterno)
        /// </summary>
        public string NombreFormateado => $"{Nombres} {ApellidoPaterno} {(string.IsNullOrEmpty(ApellidoMaterno) ? "" : ApellidoMaterno)}".Trim();
        
        /// <summary>
        /// Iniciales del nombre (primera letra de cada nombre y apellidos)
        /// </summary>
        public string Iniciales
        {
            get
            {
                string iniciales = string.Empty;
                
                if (!string.IsNullOrEmpty(Nombres))
                {
                    var nombresParts = Nombres.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var nombre in nombresParts)
                    {
                        if (!string.IsNullOrEmpty(nombre))
                            iniciales += nombre[0];
                    }
                }
                
                if (!string.IsNullOrEmpty(ApellidoPaterno) && ApellidoPaterno.Length > 0)
                    iniciales += ApellidoPaterno[0];
                    
                if (!string.IsNullOrEmpty(ApellidoMaterno) && ApellidoMaterno.Length > 0)
                    iniciales += ApellidoMaterno[0];
                    
                return iniciales.ToUpper();
            }
        }

        private NombreCompleto(string nombres, string apellidoPaterno, string apellidoMaterno)
        {
            Nombres = nombres;
            ApellidoPaterno = apellidoPaterno;
            ApellidoMaterno = apellidoMaterno;
        }

        /// <summary>
        /// Crea un nuevo nombre completo
        /// </summary>
        /// <param name="nombres">Nombre(s) de la persona</param>
        /// <param name="apellidoPaterno">Apellido paterno</param>
        /// <param name="apellidoMaterno">Apellido materno (opcional)</param>
        /// <returns>Objeto NombreCompleto validado</returns>
        /// <exception cref="ArgumentException">Si los datos requeridos no son válidos</exception>
        public static NombreCompleto Create(string nombres, string apellidoPaterno, string apellidoMaterno = "")
        {
            if (string.IsNullOrWhiteSpace(nombres))
                throw new ArgumentException("El nombre no puede estar vacío", nameof(nombres));
                
            if (string.IsNullOrWhiteSpace(apellidoPaterno))
                throw new ArgumentException("El apellido paterno no puede estar vacío", nameof(apellidoPaterno));
                
            return new NombreCompleto(
                nombres.Trim(), 
                apellidoPaterno.Trim(), 
                apellidoMaterno?.Trim() ?? string.Empty
            );
        }
        
        /// <summary>
        /// Crea un nombre completo a partir de una cadena de texto
        /// </summary>
        /// <param name="nombreCompleto">Nombre completo en formato "Nombres ApellidoPaterno ApellidoMaterno"</param>
        /// <returns>Objeto NombreCompleto</returns>
        /// <exception cref="ArgumentException">Si el formato no es válido</exception>
        public static NombreCompleto Parse(string nombreCompleto)
        {
            if (string.IsNullOrWhiteSpace(nombreCompleto))
                throw new ArgumentException("El nombre no puede estar vacío", nameof(nombreCompleto));
                
            string[] parts = nombreCompleto.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length < 2)
                throw new ArgumentException("El nombre debe incluir al menos nombres y apellido paterno", nameof(nombreCompleto));
                
            string nombres;
            string apellidoPaterno;
            string apellidoMaterno = string.Empty;
            
            // Si hay al menos 3 partes, asumimos que hay nombre, apellido paterno y materno
            if (parts.Length >= 3)
            {
                // El último elemento es el apellido materno
                apellidoMaterno = parts[parts.Length - 1];
                
                // El penúltimo elemento es el apellido paterno
                apellidoPaterno = parts[parts.Length - 2];
                
                // Todo lo demás son nombres
                nombres = string.Join(" ", parts, 0, parts.Length - 2);
            }
            else // Solo hay dos partes
            {
                nombres = parts[0];
                apellidoPaterno = parts[1];
            }
            
            return new NombreCompleto(nombres, apellidoPaterno, apellidoMaterno);
        }
        
        /// <summary>
        /// Intenta crear un nombre completo a partir de una cadena de texto
        /// </summary>
        /// <param name="nombreCompleto">Nombre completo en formato "Nombres ApellidoPaterno ApellidoMaterno"</param>
        /// <param name="result">Objeto NombreCompleto resultante si es válido</param>
        /// <returns>True si se pudo crear, False en caso contrario</returns>
        public static bool TryParse(string nombreCompleto, out NombreCompleto result)
        {
            result = null;
            
            if (string.IsNullOrWhiteSpace(nombreCompleto))
                return false;
                
            try
            {
                result = Parse(nombreCompleto);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Devuelve el nombre completo formateado
        /// </summary>
        public override string ToString()
        {
            return NombreFormateado;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Nombres;
            yield return ApellidoPaterno;
            yield return ApellidoMaterno;
        }
    }
} 