using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Identity.Services
{
    public class JwtTokenServiceTests : IntegrationTestBase
    {
        private IJwtTokenService _jwtTokenService = null!;

        public JwtTokenServiceTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _jwtTokenService = ServiceProvider.GetRequiredService<IJwtTokenService>();
        }

        [Fact]
        public void GenerateToken_DebeCrearUnTokenValidoConClaimsCorrectos()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var userName = "testuser";
            var email = "test@test.com";
            var roles = new List<string> { "Admin", "User" };

            // Act
            var tokenResponse = _jwtTokenService.GenerateToken(userId, userName, email, roles);

            // Assert
            tokenResponse.Should().NotBeNull();
            tokenResponse.AccessToken.Should().NotBeNullOrEmpty();
            tokenResponse.ExpiresIn.Should().BeGreaterThan(0);

            var principal = _jwtTokenService.GetPrincipalFromExpiredToken(tokenResponse.AccessToken);
            principal.Should().NotBeNull();

            var identity = principal.Identity as ClaimsIdentity;
            identity.Should().NotBeNull();

            identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value.Should().Be(userId);
            identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value.Should().Be(email);
            identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value.Should().Be(userName);
            identity.Claims.FirstOrDefault(c => c.Type == "uid")?.Value.Should().Be(userId);

            var roleClaims = identity.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            roleClaims.Should().BeEquivalentTo(roles);
        }

        [Fact]
        public void GenerateRefreshToken_DebeCrearUnTokenDeRefrescoValido()
        {
            // Arrange & Act
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            // Assert
            refreshToken.Should().NotBeNullOrEmpty();

            // Verificar que es un string Base64 válido
            var buffer = new Span<byte>(new byte[refreshToken.Length]);
            var esBase64 = Convert.TryFromBase64String(refreshToken, buffer, out _);
            esBase64.Should().BeTrue();
        }
    }
} 