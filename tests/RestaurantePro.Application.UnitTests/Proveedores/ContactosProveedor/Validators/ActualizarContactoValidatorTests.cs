namespace RestaurantePro.Application.UnitTests.Proveedores.ContactosProveedor.Validators;

using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using RestaurantePro.Application.Proveedores.ContactosProveedor.Commands.ActualizarContacto;
using Xunit;

/// <summary>
/// Tests para ActualizarContactoValidator
/// </summary>
public class ActualizarContactoValidatorTests
{
    private readonly ActualizarContactoValidator _validator;

    public ActualizarContactoValidatorTests()
    {
        _validator = new ActualizarContactoValidator();
    }

    private ActualizarContactoCommand CrearCommandValido()
    {
        return new ActualizarContactoCommand
        {
            Id = Guid.NewGuid(),
            ProveedorId = Guid.NewGuid(),
            Nombre = "Juan",
            Apellidos = "Perez",
            Cargo = "Gerente de Compras",
            Email = "juan.perez@proveedor.com",
            Telefono = "555-123-4567",
            EmailSecundario = "juanp@gmail.com",
            TelefonoMovil = "555-987-6543",
            Extension = "123",
            Departamento = "Compras",
            HorarioContacto = "9:00 - 18:00",
            Notas = "Contacto principal para compras regulares",
            EsPrincipal = true,
            PuedeAutorizarPedidos = true,
            LimiteAutorizacion = 50000,
            MotivoActualizacion = "Actualización de datos"
        };
    }

    [Theory]
    [InlineData("email-invalido")]
    [InlineData("@dominio.com")]
    [InlineData("usuario@")]
    [InlineData("usuario@dominio.")]
    [InlineData("usuario@.dominio.com")]
    [InlineData("usuario..punto@dominio.com")]
    public async Task Validate_ConEmailFormatoInvalido_DeberiaRetornarError(string emailInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Email = emailInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.Email) &&
            e.ErrorMessage.Contains("El email debe tener un formato válido"));
    }
} 