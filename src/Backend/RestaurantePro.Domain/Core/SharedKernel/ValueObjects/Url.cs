namespace RestaurantePro.Domain.Core.SharedKernel.ValueObjects
{
    /// <summary>
    /// Objeto de valor que representa una URL válida
    /// </summary>
    public class Url : ValueObject
    {
        private static readonly Regex UrlRegex = new Regex(
            @"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// La URL en formato de texto
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Indica si la URL utiliza el protocolo HTTPS
        /// </summary>
        public bool IsSecure { get; }

        /// <summary>
        /// Dominio de la URL (sin protocolo)
        /// </summary>
        public string Domain { get; }

        /// <summary>
        /// Ruta de la URL (sin dominio ni protocolo)
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Indica si la URL incluye protocolo (http:// o https://)
        /// </summary>
        public bool HasProtocol { get; }

        private Url(string value, bool isSecure, string domain, string path, bool hasProtocol)
        {
            Value = value;
            IsSecure = isSecure;
            Domain = domain;
            Path = path;
            HasProtocol = hasProtocol;
        }

        /// <summary>
        /// Crea un objeto Url a partir de una cadena de texto
        /// </summary>
        /// <param name="url">URL a validar</param>
        /// <param name="requireSecure">Si es true, requiere que la URL use HTTPS</param>
        /// <returns>Objeto Url validado</returns>
        /// <exception cref="ArgumentException">Si el formato no es válido o no cumple el requisito de seguridad</exception>
        public static Url Create(string url, bool requireSecure = false)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("La URL no puede estar vacía", nameof(url));

            url = url.Trim();

            // Si es una ruta relativa, considerarla válida
            if (url.StartsWith("/") || url.StartsWith("./") || url.StartsWith("../"))
            {
                return new Url(url, false, string.Empty, url, false);
            }

            // Asegurarse de que tenga protocolo para validación
            string urlForValidation = url;
            bool hasProtocol = url.StartsWith("http://") || url.StartsWith("https://");
            
            if (!hasProtocol)
            {
                urlForValidation = "http://" + url;
            }

            // Intentar parsear la URL
            if (!Uri.TryCreate(urlForValidation, UriKind.Absolute, out Uri uri))
                throw new ArgumentException("El formato de la URL no es válido", nameof(url));

            bool isSecure = uri.Scheme == "https";
            
            if (requireSecure && !isSecure)
                throw new ArgumentException("Se requiere una URL segura (HTTPS)", nameof(url));

            string domain = uri.Host;
            string path = uri.AbsolutePath;

            // Si no tenía protocolo, mantener la URL original
            string finalUrl = hasProtocol ? url : url;

            return new Url(finalUrl, isSecure, domain, path, hasProtocol);
        }

        /// <summary>
        /// Intenta crear un objeto Url sin lanzar excepciones
        /// </summary>
        /// <param name="url">URL a validar</param>
        /// <param name="result">Objeto Url resultante si es válido</param>
        /// <param name="requireSecure">Si es true, requiere que la URL use HTTPS</param>
        /// <returns>True si se creó correctamente, False en caso contrario</returns>
        public static bool TryCreate(string url, out Url result, bool requireSecure = false)
        {
            result = null;

            try
            {
                result = Create(url, requireSecure);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Asegura que la URL tenga protocolo
        /// </summary>
        /// <param name="secure">Si es true, usa HTTPS; si es false, usa HTTP</param>
        /// <returns>URL con protocolo</returns>
        public Url EnsureProtocol(bool secure = true)
        {
            if (HasProtocol)
            {
                if ((secure && IsSecure) || (!secure && !IsSecure))
                    return this;

                // Cambiar el protocolo
                if (secure)
                    return new Url(Value.Replace("http://", "https://"), true, Domain, Path, true);
                else
                    return new Url(Value.Replace("https://", "http://"), false, Domain, Path, true);
            }

            // Agregar el protocolo
            string protocol = secure ? "https://" : "http://";
            return new Url(protocol + Value, secure, Domain, Path, true);
        }

        /// <summary>
        /// Conversión implícita de Url a string
        /// </summary>
        public static implicit operator string(Url url) => url?.Value;

        /// <summary>
        /// Convierte la URL a su representación en texto
        /// </summary>
        public override string ToString() => Value;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
} 