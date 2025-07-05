using RestaurantePro.Domain.Core.Base;
using System;

namespace RestaurantePro.Infrastructure.IntegrationTests.TestBase
{
    public class TestEntity : EntityBase
    {
        public string Nombre { get; set; }
    }

    public class NonAuditableTestEntity
    {
        public Guid Id { get; set; }
        public string Valor { get; set; }
    }
} 