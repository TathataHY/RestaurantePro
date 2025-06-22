using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Infrastructure.Identity.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RestaurantePro.Infrastructure.Identity.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtConfiguration _jwtConfig;

        public JwtTokenService(IOptions<JwtConfiguration> jwtConfig)
        {
            _jwtConfig = jwtConfig.Value;
        }

        public JwtTokenResponse GenerateToken(string userId, string userName, string email, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, userName),
                new Claim("uid", userId)
            };

            // Agregar roles como claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = _jwtConfig.GetSymmetricSecurityKey();
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpirationInMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtConfig.Issuer,
                audience: _jwtConfig.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            var tokenHandler = new JwtSecurityTokenHandler();

            return new JwtTokenResponse
            {
                AccessToken = tokenHandler.WriteToken(token),
                ExpiresIn = (int)TimeSpan.FromMinutes(_jwtConfig.ExpirationInMinutes).TotalSeconds,
                RequiresRefresh = _jwtConfig.RefreshTokenExpirationInDays > 0
            };
        }

        // Método interno para usar con IdentityApplicationUser
        internal JwtTokenResponse GenerateToken(IdentityApplicationUser user, IList<string> roles)
        {
            return GenerateToken(user.Id.ToString(), user.UserName, user.Email, roles);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _jwtConfig.GetSymmetricSecurityKey(),
                ValidIssuer = _jwtConfig.Issuer,
                ValidAudience = _jwtConfig.Audience,
                ValidateLifetime = false // No validamos la expiración
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }
} 