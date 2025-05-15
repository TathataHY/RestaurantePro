using System;

namespace RestaurantePro.Domain.Comercial.Clientes.ValueObjects
{
    /// <summary>
    /// Value Object que representa el nombre de un cliente.
    /// Es inmutable y encapsula reglas de validación.
    /// </summary>
    public class ClienteNombre
    {
        public string Nombre { get; }
        public string Apellido { get; }

        private ClienteNombre(string nombre, string apellido)
        {
            Nombre = nombre;
            Apellido = apellido;
        }

        public static ClienteNombre Crear(string nombre, string apellido)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío", nameof(nombre));
            
            if (string.IsNullOrWhiteSpace(apellido))
                throw new ArgumentException("El apellido no puede estar vacío", nameof(apellido));
            
            return new ClienteNombre(nombre, apellido);
        }

        public string NombreCompleto => $"{Nombre} {Apellido}";

        // Value Objects se comparan por valor, no por referencia
        public override bool Equals(object obj)
        {
            if (obj is not ClienteNombre other)
                return false;
            
            return Nombre == other.Nombre && Apellido == other.Apellido;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Nombre, Apellido);
        }
    }
}
