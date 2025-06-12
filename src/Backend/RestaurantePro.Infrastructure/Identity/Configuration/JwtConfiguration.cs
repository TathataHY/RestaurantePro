using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace RestaurantePro.Infrastructure.Identity.Configuration
{
    public class JwtConfiguration
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ExpirationInMinutes { get; set; }
        public int RefreshTokenExpirationInDays { get; set; }

        public SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Secret));
        }
    }

    public class JwtConfigurationSetup : IConfigureOptions<JwtConfiguration>
    {
        private readonly IConfiguration _configuration;
        private const string SectionName = "JwtSettings";

        public JwtConfigurationSetup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(JwtConfiguration options)
        {
            _configuration.GetSection(SectionName).Bind(options);

            // Validaciones
            if (string.IsNullOrEmpty(options.Secret))
            {
                throw new ArgumentException("JWT Secret must be configured in appsettings.json");
            }

            if (string.IsNullOrEmpty(options.Issuer))
            {
                throw new ArgumentException("JWT Issuer must be configured in appsettings.json");
            }

            if (string.IsNullOrEmpty(options.Audience))
            {
                throw new ArgumentException("JWT Audience must be configured in appsettings.json");
            }

            if (options.ExpirationInMinutes <= 0)
            {
                throw new ArgumentException("JWT ExpirationInMinutes must be a positive number");
            }

            if (options.RefreshTokenExpirationInDays <= 0)
            {
                throw new ArgumentException("JWT RefreshTokenExpirationInDays must be a positive number");
            }
        }
    }
} 