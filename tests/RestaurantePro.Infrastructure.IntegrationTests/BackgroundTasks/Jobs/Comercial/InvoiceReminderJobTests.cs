using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Comercial;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.BackgroundTasks.Jobs.Comercial;

public class InvoiceReminderJobTests : IntegrationTestBase
{
    private readonly Mock<IEmailService> _emailServiceMock;

    public InvoiceReminderJobTests(DatabaseFixture fixture) : base(fixture)
    {
        _emailServiceMock = new Mock<IEmailService>();
    }

    [Fact]
    public async Task ExecuteInternalAsync_ShouldSendReminderForDueInvoices()
    {
        // Arrange
        var nombre = ClienteNombre.Crear("Juan", "Perez");
        var email = Email.Create("juan.perez@test.com");
        var telefono = PhoneNumber.Create("123456789");
        var cliente = Cliente.Crear(Guid.NewGuid(), nombre, email, telefono, new DateTime(1990, 1, 1));
        
        var fechaVencimiento = ServiceProvider.GetRequiredService<IDateTimeService>().UtcNow.AddDays(2);

        var factura = Factura.Crear("F001-001", TipoFactura.Electronica, cliente.Nombre.NombreCompleto, cliente.Id);
        
        // Add a detail to the invoice before emitting
        factura.AgregarDetalle(Guid.NewGuid(), "Producto de prueba", 1, 100, 0.19m);

        // We need to manually set the expiration date for the test scenario
        var fechaEmision = ServiceProvider.GetRequiredService<IDateTimeService>().UtcNow;
        factura.GetType().GetProperty("FechaVencimiento")?.SetValue(factura, fechaVencimiento);
        factura.GetType().GetProperty("FechaEmision")?.SetValue(factura, fechaEmision);
        factura.Emitir();


        DbContext.Add(cliente);
        DbContext.Add(factura);
        await DbContext.SaveChangesAsync();

        var job = new InvoiceReminderJob(
            ServiceProvider.GetRequiredService<ILogger<InvoiceReminderJob>>(),
            ServiceProvider.GetRequiredService<IFacturaRepository>(),
            _emailServiceMock.Object,
            ServiceProvider.GetRequiredService<IUnitOfWork>(),
            Options.Create(new InvoiceReminderOptions { DaysBeforeExpiration = 3 })
        );

        // Act
        // We need to call the protected method for testing purposes
        var methodInfo = typeof(InvoiceReminderJob).GetMethod("ExecuteInternalAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (methodInfo != null)
        {
            await (Task)methodInfo.Invoke(job, new object[] { default(System.Threading.CancellationToken) });
        }


        // Assert
        _emailServiceMock.Verify(
            x => x.SendEmailAsync(
                It.Is<string>(e => e == cliente.Email.Value),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Once);
    }
} 