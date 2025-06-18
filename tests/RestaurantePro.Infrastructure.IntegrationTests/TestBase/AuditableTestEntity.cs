using RestaurantePro.Domain.Core.Base;

namespace RestaurantePro.Infrastructure.IntegrationTests.TestBase
{
    // Entidad de prueba para los tests de interceptores
    public class AuditableTestEntity : EntityBase
    {
        public string Nombre { get; private set; }

        // Exponer la propiedad Activo para poder verificarla en los tests
        public bool IsActive => !EstaEliminado;

        private AuditableTestEntity(string nombre)
        {
            Nombre = nombre;
        }

        public static AuditableTestEntity Crear(string nombre)
        {
            return new AuditableTestEntity(nombre);
        }

        public void ActualizarNombre(string nuevoNombre)
        {
            Nombre = nuevoNombre;
            MarkAsModified();
        }

        public void RegistrarEventoPrueba()
        {
            AddDomainEvent(new TestDomainEvent());
        }
    }
} 