namespace RestaurantePro.Domain.Comercial.Clientes.ValueObjects
{
    /// <summary>
    /// Value Object que representa el nombre de un cliente
    /// </summary>
    public class ClienteNombre : ValueObject
    {
        /// <summary>
        /// Nombre(s) del cliente
        /// </summary>
        public string Nombre { get; }

        /// <summary>
        /// Apellido(s) del cliente
        /// </summary>
        public string Apellido { get; }

        /// <summary>
        /// Nombre completo del cliente (Nombre + Apellido)
        /// </summary>
        public string NombreCompleto => $"{Nombre} {Apellido}".Trim();

        /// <summary>
        /// Constructor privado
        /// </summary>
        private ClienteNombre(string nombre, string apellido)
        {
            Nombre = nombre;
            Apellido = apellido;
        }

        /// <summary>
        /// Crea una nueva instancia de ClienteNombre
        /// </summary>
        /// <param name="nombre">Nombre del cliente</param>
        /// <param name="apellido">Apellido del cliente</param>
        /// <returns>Una nueva instancia de ClienteNombre</returns>
        /// <exception cref="ArgumentException">Si el nombre o apellido no son válidos</exception>
        public static ClienteNombre Crear(string nombre, string apellido)
        {
            Validar(nombre, apellido);
            return new ClienteNombre(nombre?.Trim(), apellido?.Trim());
        }

        /// <summary>
        /// Valida que el nombre y apellido cumplan con las reglas de negocio
        /// </summary>
        private static void Validar(string nombre, string apellido)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del cliente no puede estar vacío", nameof(nombre));

            if (nombre.Length > 50)
                throw new ArgumentException("El nombre del cliente no puede exceder los 50 caracteres", nameof(nombre));

            if (string.IsNullOrWhiteSpace(apellido))
                throw new ArgumentException("El apellido del cliente no puede estar vacío", nameof(apellido));

            if (apellido.Length > 50)
                throw new ArgumentException("El apellido del cliente no puede exceder los 50 caracteres", nameof(apellido));
        }

        /// <summary>
        /// Implementación requerida por la clase base ValueObject
        /// </summary>
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Nombre;
            yield return Apellido;
        }

        public override string ToString()
        {
            return NombreCompleto;
        }
    }
}
