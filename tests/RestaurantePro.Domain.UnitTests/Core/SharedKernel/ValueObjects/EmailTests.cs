namespace RestaurantePro.Domain.UnitTests.Core.SharedKernel.ValueObjects
{
    public class EmailTests
    {
        [Theory]
        [InlineData("usuario@dominio.com")]
        [InlineData("usuario.apellido@empresa.cl")]
        [InlineData("usuario-apellido@dominio.net")]
        [InlineData("usuario_apellido@dominio.org")]
        [InlineData("usuario+filtro@gmail.com")]
        [InlineData("1234567890@dominio.com")]
        [InlineData("contacto@empresa-nombre.com")]
        [InlineData("admin@servidor.empresa.com")]
        [InlineData("info@dominio.com.cl")]
        public void Create_ConEmailsValidos_DebeCrearObjeto(string email)
        {
            // Act
            var resultado = Email.Create(email);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Value.Should().Be(email);
        }

        [Theory]
        [InlineData("usuario@dominio")] // Sin TLD
        [InlineData("usuario@.com")] // Dominio inválido
        [InlineData("@dominio.com")] // Sin nombre de usuario
        [InlineData("usuario@")] // Sin dominio
        [InlineData("usuario.dominio.com")] // Sin @
        [InlineData("usuario@dominio..com")] // Puntos consecutivos en dominio
        [InlineData("usuario..test@dominio.com")] // Puntos consecutivos en usuario
        [InlineData("usuario@dominio.c")] // TLD demasiado corto
        [InlineData("usuario@dominio.123")] // TLD con números
        [InlineData("<script>@dominio.com")] // Caracteres no permitidos
        [InlineData("usuario@dominio.abcdefghijk")] // TLD demasiado largo
        public void Create_ConEmailsInvalidos_DebeLanzarException(string email)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => Email.Create(email));
        }

        [Theory]
        [InlineData("aaaaaaaaaaaaaa@dominio.com")] // Muchos caracteres repetitivos (más de los permitidos)
        [InlineData("abcabcabcabcabcabcabc@dominio.com")] // Patrón repetitivo extenso
        [InlineData("12341234123412341234@dominio.com")] // Patrón repetitivo numérico extenso
        public void Create_ConPatronesRepetitivos_DebeLanzarException(string email)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => Email.Create(email));
        }

        [Theory]
        [InlineData("testuser@dominio.com")] // Contiene 'test' pero es válido
        [InlineData("adminuser@dominio.com")] // Contiene 'admin' pero es válido
        [InlineData("developer@dominio.com")] // Contiene 'dev' pero es válido
        [InlineData("infocontact@dominio.com")] // Contiene 'info' pero es válido
        [InlineData("supportteam@dominio.com")] // Contiene 'support' pero es válido
        public void Create_ConPatronesPermitidos_DebeCrearObjeto(string email)
        {
            // Act
            var resultado = Email.Create(email);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Value.Should().Be(email);
        }

        [Theory]
        [InlineData("aaaaaaa@dominio.com")] // Caracteres repetitivos
        [InlineData("abcabcabc@dominio.com")] // Patrón repetitivo
        [InlineData("123123123@dominio.com")] // Patrón repetitivo numérico
        [InlineData("test!@dominio.com")] // Caracteres especiales normalmente no permitidos
        [InlineData("usuario@dominio.123")] // TLD con números
        public void CreateForTesting_ConEmailsParaPruebas_DebeCrearObjeto(string email)
        {
            // Act
            var resultado = Email.CreateForTesting(email);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Value.Should().Be(email);
        }

        [Fact]
        public void EsDominioChileno_ConDominioChileno_DebeRetornarTrue()
        {
            // Arrange
            var email = Email.Create("usuario@empresa.cl");

            // Act & Assert
            email.EsDominioChileno.Should().BeTrue();
        }

        [Fact]
        public void EsDominioChileno_ConDominioChilenoEnLista_DebeRetornarTrue()
        {
            // Arrange
            var email = Email.Create("usuario@uc.cl");

            // Act & Assert
            email.EsDominioChileno.Should().BeTrue();
        }

        [Fact]
        public void EsDominioEmpresarial_ConDominioEmpresarial_DebeRetornarTrue()
        {
            // Arrange
            var email = Email.Create("usuario@miempresa.com");

            // Act & Assert
            email.EsDominioEmpresarial.Should().BeTrue();
        }

        [Fact]
        public void EsDominioEmpresarial_ConDominioGratuito_DebeRetornarFalse()
        {
            // Arrange
            var email = Email.Create("usuario@gmail.com");

            // Act & Assert
            email.EsDominioEmpresarial.Should().BeFalse();
        }

        [Fact]
        public void EsDominioEducativo_ConDominioEducativo_DebeRetornarTrue()
        {
            // Arrange
            var email = Email.Create("estudiante@universidad.edu");

            // Act & Assert
            email.EsDominioEducativo.Should().BeTrue();
        }

        [Fact]
        public void EsDominioEducativo_ConDominioEducativoChileno_DebeRetornarTrue()
        {
            // Arrange
            var email = Email.Create("estudiante@uc.cl");

            // Act & Assert
            email.EsDominioEducativo.Should().BeTrue();
        }

        [Fact]
        public void EsDominioGubernamental_ConDominioGubernamental_DebeRetornarTrue()
        {
            // Arrange
            var email = Email.Create("funcionario@institucion.gob.cl");

            // Act & Assert
            email.EsDominioGubernamental.Should().BeTrue();
        }

        [Fact]
        public void GenerarAliasGmail_ConCorreoGmail_DebeGenerarAlias()
        {
            // Arrange
            var email = Email.Create("usuario@gmail.com");

            // Act
            var aliasEmail = email.GenerarAliasGmail("filtro");

            // Assert
            aliasEmail.Should().NotBeNull();
            aliasEmail.Value.Should().Be("usuario+filtro@gmail.com");
        }

        [Fact]
        public void GenerarAliasGmail_ConCorreoNoGmail_DebeRetornarNull()
        {
            // Arrange
            var email = Email.Create("usuario@outlook.com");

            // Act
            var aliasEmail = email.GenerarAliasGmail("filtro");

            // Assert
            aliasEmail.Should().BeNull();
        }

        [Fact]
        public void TryCreate_ConEmailValido_DebeRetornarTrue()
        {
            // Act
            bool resultado = Email.TryCreate("usuario@dominio.com", out var email);

            // Assert
            resultado.Should().BeTrue();
            email.Should().NotBeNull();
        }

        [Fact]
        public void TryCreate_ConEmailInvalido_DebeRetornarFalse()
        {
            // Act
            bool resultado = Email.TryCreate("usuario@dominio", out var email);

            // Assert
            resultado.Should().BeFalse();
            email.Should().BeNull();
        }

        [Fact]
        public void TryCreateForTesting_ConEmailParaPruebas_DebeRetornarTrue()
        {
            // Act
            bool resultado = Email.TryCreateForTesting("aaaaaaa@dominio.com", out var email);

            // Assert
            resultado.Should().BeTrue();
            email.Should().NotBeNull();
        }
    }
} 