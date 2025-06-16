using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RestaurantePro.Infrastructure.Caching.Configuration;
using RestaurantePro.Infrastructure.Caching.Services;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Text.Json;
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
                KeyPrefix = "TestPrefix",
                Redis = new RedisConfiguration { Database = 0 }
            };
            _optionsMock = new Mock<IOptions<CacheConfiguration>>();
            _optionsMock.Setup(opt => opt.Value).Returns(_cacheConfig);

            _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(_databaseMock.Object);
        }

        private Expression<Func<RedisKey, bool>> KeyMatcher(string expectedKey) => k => k.ToString() == expectedKey;
        private Expression<Func<RedisValue, bool>> ValueMatcher(string expectedValue) => v => v.ToString() == expectedValue;

        [Fact]
        public void Get_WhenKeyExists_ShouldReturnValue()
        {
            // Arrange
            var key = "testKey";
            var expectedValue = "testValue";
            var serializedValue = JsonSerializer.Serialize(expectedValue);
            var fullKey = $"{_cacheConfig.KeyPrefix}:{key}";

            _databaseMock.Setup(db => db.StringGet(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .Returns(serializedValue);

            var cacheService = new RedisCacheService(_redisMock.Object, _loggerMock.Object, _optionsMock.Object);

            // Act
            var result = cacheService.Get<string>(key);

            // Assert
            _databaseMock.Verify(db => db.StringGet(It.Is(KeyMatcher(fullKey)), CommandFlags.None), Times.Once);
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void Set_ShouldStoreValueInCache()
        {
            // Arrange
            var key = "testKey";
            var value = "testValue";
            var fullKey = $"{_cacheConfig.KeyPrefix}:{key}";
            var serializedValue = JsonSerializer.Serialize(value);

            var cacheService = new RedisCacheService(_redisMock.Object, _loggerMock.Object, _optionsMock.Object);

            // Act
            cacheService.Set(key, value);

            // Assert
            _databaseMock.Verify(db => db.StringSet(
                It.Is(KeyMatcher(fullKey)),
                It.Is(ValueMatcher(serializedValue)),
                TimeSpan.FromHours(1),
                false,
                When.Always,
                CommandFlags.None), Times.Once);
        }

        [Fact]
        public void Remove_ShouldRemoveValueFromCache()
        {
            // Arrange
            var key = "testKey";
            var fullKey = $"{_cacheConfig.KeyPrefix}:{key}";
            var cacheService = new RedisCacheService(_redisMock.Object, _loggerMock.Object, _optionsMock.Object);

            // Act
            cacheService.Remove(key);

            // Assert
            _databaseMock.Verify(db => db.KeyDelete(It.Is(KeyMatcher(fullKey)), It.IsAny<CommandFlags>()), Times.Once);
        }

        [Fact]
        public void Exists_WhenKeyExists_ShouldReturnTrue()
        {
            // Arrange
            var key = "testKey";
            var fullKey = $"{_cacheConfig.KeyPrefix}:{key}";
            _databaseMock.Setup(db => db.KeyExists(It.Is(KeyMatcher(fullKey)), CommandFlags.None)).Returns(true);
            var cacheService = new RedisCacheService(_redisMock.Object, _loggerMock.Object, _optionsMock.Object);

            // Act
            var result = cacheService.Exists(key);

            // Assert
            Assert.True(result);
            _databaseMock.Verify(db => db.KeyExists(It.Is(KeyMatcher(fullKey)), CommandFlags.None), Times.Once);
        }

        [Fact]
        public void GetOrCreate_WhenKeyDoesNotExist_ShouldCreateAndStoreNewValue()
        {
            // Arrange
            var key = "testKey";
            var expectedValue = "testValue";
            var fullKey = $"{_cacheConfig.KeyPrefix}:{key}";
            var serializedValue = JsonSerializer.Serialize(expectedValue);

            _databaseMock.Setup(db => db.KeyExists(It.Is(KeyMatcher(fullKey)), It.IsAny<CommandFlags>())).Returns(false);
            var cacheService = new RedisCacheService(_redisMock.Object, _loggerMock.Object, _optionsMock.Object);

            // Act
            var result = cacheService.GetOrCreate(key, () => expectedValue);

            // Assert
            Assert.Equal(expectedValue, result);
            _databaseMock.Verify(db => db.KeyExists(It.Is(KeyMatcher(fullKey)), It.IsAny<CommandFlags>()), Times.Once);
            _databaseMock.Verify(db => db.StringSet(
                It.Is(KeyMatcher(fullKey)),
                It.Is(ValueMatcher(serializedValue)),
                TimeSpan.FromHours(1),
                false,
                When.Always,
                CommandFlags.None), Times.Once);
        }

        [Fact]
        public async Task GetOrAddAsync_WhenKeyDoesNotExist_ShouldCreateAndStoreNewValue()
        {
            // Arrange
            var key = "testKey";
            var expectedValue = "testValue";
            var fullKey = $"{_cacheConfig.KeyPrefix}:{key}";
            var serializedValue = JsonSerializer.Serialize(expectedValue);

            _databaseMock.Setup(db => db.KeyExistsAsync(It.Is(KeyMatcher(fullKey)), It.IsAny<CommandFlags>())).ReturnsAsync(false);
            var cacheService = new RedisCacheService(_redisMock.Object, _loggerMock.Object, _optionsMock.Object);

            // Act
            var result = await cacheService.GetOrAddAsync(key, _ => Task.FromResult(expectedValue));

            // Assert
            Assert.Equal(expectedValue, result);
            _databaseMock.Verify(db => db.KeyExistsAsync(It.Is(KeyMatcher(fullKey)), It.IsAny<CommandFlags>()), Times.Once);
            _databaseMock.Verify(db => db.StringSetAsync(
                It.Is(KeyMatcher(fullKey)),
                It.Is(ValueMatcher(serializedValue)),
                TimeSpan.FromMinutes(10),
                false,
                When.Always,
                CommandFlags.None), Times.Once);
        }

        [Fact]
        public void InvalidatePattern_ShouldDeleteMatchingKeys()
        {
            // Arrange
            var pattern = "test*";
            var fullPattern = $"{_cacheConfig.KeyPrefix}:{pattern}";
            
            var serverMock = new Mock<IServer>();
            var key1 = (RedisKey)$"{_cacheConfig.KeyPrefix}:test1";
            var key2 = (RedisKey)$"{_cacheConfig.KeyPrefix}:test2";
            
            var keys = new List<RedisKey> { key1, key2 };
            var endpoints = new[] { new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6379) };

            _redisMock.Setup(r => r.GetEndPoints(false)).Returns(endpoints);
            _redisMock.Setup(r => r.GetServer(It.IsAny<EndPoint>(), null)).Returns(serverMock.Object);

            serverMock.Setup(s => s.Keys(
                    -1,
                    It.Is(ValueMatcher(fullPattern)),
                    It.IsAny<int>(),
                    It.IsAny<long>(),
                    0,
                    It.IsAny<CommandFlags>()))
                .Returns(keys);

            var cacheService = new RedisCacheService(_redisMock.Object, _loggerMock.Object, _optionsMock.Object);

            // Act
            cacheService.InvalidatePattern(pattern);

            // Assert
            serverMock.Verify(s => s.Keys(-1, It.Is(ValueMatcher(fullPattern)), 250, 0, 0, CommandFlags.None), Times.Once);
            _databaseMock.Verify(db => db.KeyDelete((RedisKey)key1, CommandFlags.None), Times.Once);
            _databaseMock.Verify(db => db.KeyDelete((RedisKey)key2, CommandFlags.None), Times.Once);
        }
    }
} 