using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Collections.Generic;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Notificaciones.Interfaces;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Core.Usuarios.Interfaces;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Domain.Proveedores.Interfaces;
using RestaurantePro.Infrastructure.DependencyInjection;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Interceptors;
using Xunit;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;

namespace RestaurantePro.Infrastructure.IntegrationTests.DependencyInjection
{
    public class PersistenceSetupTests
    {
        private readonly IServiceProvider _serviceProvider;

        public PersistenceSetupTests()
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"UseInMemoryDatabase", "true"}
                })
                .Build();

            // Mocks de dependencias
            services.AddSingleton(Substitute.For<IDomainEventDispatcher>());
            services.AddSingleton(Substitute.For<ICurrentUserService>());
            services.AddSingleton(Substitute.For<IDateTimeService>());

            // Configurar logging
            services.AddLogging();

            // Llamar al método de configuración de persistencia (esto registrará todos los DbContexts)
            services.AddPersistenceServices(configuration, isTestEnvironment: true);

            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public void AddPersistenceServices_ShouldRegisterInterceptors()
        {
            // Assert
            Assert.NotNull(_serviceProvider.GetService<AuditableEntityInterceptor>());
            Assert.NotNull(_serviceProvider.GetService<DomainEventInterceptor>());
            Assert.NotNull(_serviceProvider.GetService<SoftDeleteInterceptor>());
        }

        [Fact]
        public void AddPersistenceServices_ShouldRegisterUnitOfWork()
        {
            // Assert
            Assert.NotNull(_serviceProvider.GetService<IUnitOfWork>());
        }

        [Fact]
        public void AddPersistenceServices_ShouldRegisterCoreRepositories()
        {
            // Assert
            Assert.NotNull(_serviceProvider.GetService<IProductoRepository>());
            Assert.NotNull(_serviceProvider.GetService<IProductoCategoriaRepository>());
            Assert.NotNull(_serviceProvider.GetService<IUsuarioRepository>());
            Assert.NotNull(_serviceProvider.GetService<INotificacionRepository>());
        }
        
        [Fact]
        public void AddPersistenceServices_ShouldRegisterComercialRepositories()
        {
            // Assert
            Assert.NotNull(_serviceProvider.GetService<IClienteRepository>());
            Assert.NotNull(_serviceProvider.GetService<IFacturaRepository>());
            Assert.NotNull(_serviceProvider.GetService<ITarjetaFidelizacionRepository>());
        }
        
        [Fact]
        public void AddPersistenceServices_ShouldRegisterOperacionesRepositories()
        {
            // Assert
            Assert.NotNull(_serviceProvider.GetService<IComandaRepository>());
            Assert.NotNull(_serviceProvider.GetService<IReservacionRepository>());
            Assert.NotNull(_serviceProvider.GetService<IMesaRepository>());
            Assert.NotNull(_serviceProvider.GetService<IPreparacionRepository>());
        }
        
        [Fact]
        public void AddPersistenceServices_ShouldRegisterInventarioRepositories()
        {
            // Assert
            Assert.NotNull(_serviceProvider.GetService<IIngredienteRepository>());
            Assert.NotNull(_serviceProvider.GetService<IMovimientoInventarioRepository>());
            Assert.NotNull(_serviceProvider.GetService<IOrdenCompraRepository>());
        }
        
        [Fact]
        public void AddPersistenceServices_ShouldRegisterProveedoresRepositories()
        {
            // Assert
            Assert.NotNull(_serviceProvider.GetService<IProveedorRepository>());
            Assert.NotNull(_serviceProvider.GetService<IContactoProveedorRepository>());
        }
    }
} 