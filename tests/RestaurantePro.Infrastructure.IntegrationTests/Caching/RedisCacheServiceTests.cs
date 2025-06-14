using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RestaurantePro.Infrastructure.Caching.Configuration;
using RestaurantePro.Infrastructure.Caching.Services;
using StackExchange.Redis;
using System;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Caching
{
    public class RedisCacheServiceTests
    {
        private readonly Mock<IConnectionMultiplexer> _redisMock;
        private readonly Mock<IDatabase> _databaseMock;
        private readonly Mock<ILogger<RedisCacheService>> _loggerMock;
        private readonly Mock<IOptions<CacheConfiguration>> _optionsMock;
        private readonly CacheConfiguration _cacheConfig;

        public RedisCacheServiceTests()
        {
            _redisMock = new Mock<IConnectionMultiplexer>();
            _databaseMock = new Mock<IDatabase>();
            _loggerMock = new Mock<ILogger<RedisCacheService>>();
            _cacheConfig = new CacheConfiguration
            {
                KeyPrefix = "TestPrefix:",
                Redis = new RedisConfiguration
                {
                    ConnectionString = "localhost:6379",
                    Database = 0
                }
            };
            _optionsMock = new Mock<IOptions<CacheConfiguration>>();
            _optionsMock.Setup(opt => opt.Value).Returns(_cacheConfig);

            _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(_databaseMock.Object);
        }

        [Fact]
        public void Get_WhenKeyExists_ShouldReturnValue()
        {
            // Arrange
            var key = "testKey";
            var expectedValue = "testValue";
            var serializedValue = JsonSerializer.Serialize(expectedValue);
            var fullKey = $"{_cacheConfig.KeyPrefix}{key}";
            
            _databaseMock.Setup(db => db.StringGet(fullKey, CommandFlags.None))
                .Returns(serializedValue);

            var cacheService = new RedisCacheService(_redisMock.Object, _loggerMock.Object, _optionsMock.Object);

            // Act
            var result = cacheService.Get<string>(key);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void Get_WhenKeyDoesNotExist_ShouldReturnDefault()
        {
            // Arrange
            var key = "nonExistentKey";
            var fullKey = $"{_cacheConfig.KeyPrefix}{key}";
            
            _databaseMock.Setup(db => db.StringGet(fullKey, CommandFlags.None))
                .Returns(RedisValue.Null);

            var cacheService = new RedisCacheService(_redisMock.Object, _loggerMock.Object, _optionsMock.Object);

            // Act
            var result = cacheService.Get<string>(key);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Set_ShouldStoreValueInCache()
        {
            // Arrange
            var key = "testKey";
            var value = "testValue";
            var fullKey = $"{_cacheConfig.KeyPrefix}{key}";
            
            _databaseMock.Setup(db => db.StringSet(
                    It.IsAny<RedisKey>(), 
                    It.IsAny<RedisValue>(), 
                    It.IsAny<TimeSpan?>(), 
                    It.IsAny<bool>(), 
                    It.IsAny<When>(), 
                    It.IsAny<CommandFlags>()))
                .Returns(true);

            var cacheService = new RedisCacheService(_redisMock.Object, _loggerMock.Object, _optionsMock.Object);

            // Act
            cacheService.Set(key, value);

            // Assert
            _databaseMock.Verify(db => db.StringSet(
                It.Is<RedisKey>(k => k == fullKey),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()), Times.Once);
        }

        [Fact]
        public void Remove_ShouldRemoveValueFromCache()
        {
            // Arrange
            var key = "testKey";
            var fullKey = $"{_cacheConfig.KeyPrefix}{key}";
            
            _databaseMock.Setup(db => db.KeyDelete(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .Returns(true);

            var cacheService = new RedisCacheService(_redisMock.Object, _loggerMock.Object, _optionsMock.Object);

            // Act
            cacheService.Remove(key);

            // Assert
            _databaseMock.Verify(db => db.KeyDelete(
                It.Is<RedisKey>(k => k == fullKey), 
                It.IsAny<CommandFlags>()), Times.Once);
        }

        [Fact]
        public void Exists_WhenKeyExists_ShouldReturnTrue()
        {
            // Arrange
            var key = "testKey";
            var fullKey = $"{_cacheConfig.KeyPrefix}{key}";
            
            _databaseMock.Setup(db => db.KeyExists(fullKey, CommandFlags.None))
                .Returns(true);

            var cacheService = new RedisCacheService(_redisMock.Object, _loggerMock.Object, _optionsMock.Object);

            // Act
            var result = cacheService.Exists(key);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetOrCreate_WhenKeyDoesNotExist_ShouldCreateAndStoreNewValue()
        {
            // Arrange
            var key = "testKey";
            var expectedValue = "testValue";
            var fullKey = $"{_cacheConfig.KeyPrefix}{key}";

            _databaseMock.Setup(db => db.KeyExists(fullKey, CommandFlags.None))
                .Returns(false);
                
            _databaseMock.Setup(db => db.StringSet(
                    It.IsAny<RedisKey>(), 
                    It.IsAny<RedisValue>(), 
                    It.IsAny<TimeSpan?>(), 
                    It.IsAny<bool>(), 
                    It.IsAny<When>(), 
                    It.IsAny<CommandFlags>()))
                .Returns(true);

            var cacheService = new RedisCacheService(_redisMock.Object, _loggerMock.Object, _optionsMock.Object);

            // Act
            var result = cacheService.GetOrCreate(key, () => expectedValue);

            // Assert
            Assert.Equal(expectedValue, result);
            _databaseMock.Verify(db => db.KeyExists(
                It.Is<RedisKey>(k => k == fullKey), 
                It.IsAny<CommandFlags>()), Times.Once);
            _databaseMock.Verify(db => db.StringSet(
                It.Is<RedisKey>(k => k == fullKey),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()), Times.Once);
        }

        [Fact]
        public async Task GetOrAddAsync_WhenKeyDoesNotExist_ShouldCreateAndStoreNewValue()
        {
            // Arrange
            var key = "testKey";
            var expectedValue = "testValue";
            var fullKey = $"{_cacheConfig.KeyPrefix}{key}";

            _databaseMock.Setup(db => db.KeyExists(fullKey, CommandFlags.None))
                .Returns(false);
                
            _databaseMock.Setup(db => db.StringSet(
                    It.IsAny<RedisKey>(), 
                    It.IsAny<RedisValue>(), 
                    It.IsAny<TimeSpan?>(), 
                    It.IsAny<bool>(), 
                    It.IsAny<When>(), 
                    It.IsAny<CommandFlags>()))
                .Returns(true);

            var cacheService = new RedisCacheService(_redisMock.Object, _loggerMock.Object, _optionsMock.Object);

            // Act
            var result = await cacheService.GetOrAddAsync(key, _ => Task.FromResult(expectedValue));

            // Assert
            Assert.Equal(expectedValue, result);
            _databaseMock.Verify(db => db.KeyExists(
                It.Is<RedisKey>(k => k == fullKey), 
                It.IsAny<CommandFlags>()), Times.Once);
            _databaseMock.Verify(db => db.StringSet(
                It.Is<RedisKey>(k => k == fullKey),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()), Times.Once);
        }

        [Fact]
        public void InvalidatePattern_ShouldDeleteMatchingKeys()
        {
            // Arrange
            var pattern = "test*";
            var fullPattern = $"{_cacheConfig.KeyPrefix}{pattern}";
            var server = new Mock<IServer>();
            var endpoints = new[] { new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6379) };
            
            var keys = new[] 
            {
                new RedisKey($"{_cacheConfig.KeyPrefix}test1"),
                new RedisKey($"{_cacheConfig.KeyPrefix}test2"),
                new RedisKey($"{_cacheConfig.KeyPrefix}test3")
            };

            _redisMock.Setup(r => r.GetEndPoints(It.IsAny<bool>()))
                .Returns(endpoints);
                
            _redisMock.Setup(r => r.GetServer(It.IsAny<EndPoint>(), It.IsAny<object>()))
                .Returns(server.Object);
                
            server.Setup(s => s.Keys(
                    -1, 
                    $"{fullPattern}*", 
                    250, 
                    0, 
                    0, 
                    CommandFlags.None))
                .Returns(keys);
            
            // Setup para cada clave individual
            foreach (var key in keys)
            {
                _databaseMock.Setup(db => db.KeyDelete(key, CommandFlags.None))
                    .Returns(true);
            }

            var cacheService = new RedisCacheService(_redisMock.Object, _loggerMock.Object, _optionsMock.Object);

            // Act
            cacheService.InvalidatePattern(pattern);

            // Assert - Verificamos que se obtuvo el servidor y se realizó la búsqueda de claves
            _redisMock.Verify(r => r.GetServer(It.IsAny<EndPoint>(), It.IsAny<object>()), Times.Once);
            server.Verify(s => s.Keys(
                    -1,
                    $"{fullPattern}*",
                    250,
                    0,
                    0,
                    CommandFlags.None), Times.Once);
        }
    }
} 