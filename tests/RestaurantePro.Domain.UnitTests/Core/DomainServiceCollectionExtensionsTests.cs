namespace RestaurantePro.Domain.UnitTests.Core
{
    public class DomainServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDomainServices_DebeRegistrarTodasLasPoliticas()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Act
            services.AddDomainServices();
            var serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetRequiredService<IDateTimeService>().Should().NotBeNull();
            serviceProvider.GetRequiredService<IClientesFrecuentesPolicy>().Should().NotBeNull();
            serviceProvider.GetRequiredService<IStockBajoPolicy>().Should().NotBeNull();
            serviceProvider.GetRequiredService<IProductoRecomendadoPolicy>().Should().NotBeNull();
            serviceProvider.GetRequiredService<IVisibilidadCategoriasPolicy>().Should().NotBeNull();
        }
        
        [Fact]
        public void AddDomainServicesForTests_DebeRegistrarMockDateTimeService()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Act
            services.AddDomainServicesForTests();
            var serviceProvider = services.BuildServiceProvider();
            
            // Assert
            var dateTimeService = serviceProvider.GetRequiredService<IDateTimeService>();
            dateTimeService.Should().BeOfType<MockDateTimeService>();
            
            serviceProvider.GetRequiredService<IProductoRecomendadoPolicy>().Should().NotBeNull();
        }
    }
} 