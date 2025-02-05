using Microsoft.Extensions.DependencyInjection;
using MediatR;
using AutoMapper;
using RestaurantePro.Core.Interfaces;
using RestaurantePro.Infrastructure.Services;
using RestaurantePro.Infrastructure.Hubs;
using RestaurantePro.Core.Services;
using Moq;
using System.Security.Claims;

namespace RestaurantePro.Tests
{
    public abstract class TestBase
    {
        protected readonly IServiceCollection Services;
        protected readonly ServiceProvider ServiceProvider;

        protected TestBase()
        {
            Services = new ServiceCollection();
            
            // Registrar mocks
            Services.AddScoped(_ => new Mock<IUnitOfWork>().Object);
            Services.AddScoped(_ => new Mock<IMapper>().Object);
            Services.AddScoped(_ => new Mock<IMediator>().Object);
            
            ServiceProvider = Services.BuildServiceProvider();
        }
    }
    public class MockUserContext : IUserContext
    {
        public string CurrentUser => "TestUser";
        public string CurrentRole => "TestRole";
        public ClaimsPrincipal User => new ClaimsPrincipal();
        public bool IsInRole(string role) => true;
    }
}