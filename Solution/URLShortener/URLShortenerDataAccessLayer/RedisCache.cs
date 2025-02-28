﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrlShortenerDomainLayer.Interfaces;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UrlShortenerDataAccessLayer
{
    public class RedisCache : ICache
    {
        private ConnectionMultiplexer ConnectionMultiplexer { get; }
        public RedisCache(ConnectionMultiplexer connectionMultiplexer)
        {
            ConnectionMultiplexer = connectionMultiplexer 
                ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
        }

        public T Get<T>(string key)
        {
            var redisValue = ConnectionMultiplexer.GetDatabase().StringGet(key);
            if (redisValue.HasValue)
            {
                var unpacked = redisValue.ToString();
                var deserialized = JsonSerializer.Deserialize<T>(unpacked);
                return deserialized;
            }
            else
            {
                throw new ArgumentNullException("Cache doesn't contain a value with given key");
            }
        }

        public bool Has(string key)
        {
            var redisValue = ConnectionMultiplexer.GetDatabase().StringGet(key);
            return redisValue.HasValue;
        }

        public void Set<T>(string key, T value)
        {
            var serialized = JsonSerializer.Serialize<T>(value);
            bool success = ConnectionMultiplexer.GetDatabase().StringSet(key, serialized, TimeSpan.FromMinutes(5));
            if (!success)
            {
                throw new ArgumentException("Cache error while trying to add value");
            }
        }
    }
}
