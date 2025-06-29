using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using StackExchange.Redis;
using TrafficDataAnalyticsData.Collections;
using TrafficDataAnalyticsStore.Models;

namespace TrafficDataAnalyticsStore.Business.Redis
{
    public class RedisReader : IRedisReader
    {
        private readonly ILogger<RedisReader> _logger;
        private readonly IDatabase _database;

        public RedisReader(
            ILogger<RedisReader> logger,
            IConnectionMultiplexer connectionMultiplexer)
        {
            _logger = logger;
            _database = connectionMultiplexer.GetDatabase();
        }

        public async Task<DailySummaryDto?> ComputeDailySummaryAsync(string intersectionId)
        {
            var now = DateTime.UtcNow;
            var start = new DateTimeOffset(now.Date).ToUnixTimeMilliseconds();
            var end = new DateTimeOffset(now).ToUnixTimeMilliseconds();

            var keys = await GetVehicleKeysForIntersection(intersectionId);

            int total = 0;
            Dictionary<string, int> hourly = new();

            foreach (var key in keys)
            {
                var result = await _database.ExecuteAsync("TS.RANGE", key, start, end);

                // Αν δεν είναι πίνακας, αγνόησε
                if (result.Type != ResultType.MultiBulk) continue;

                var entries = (RedisResult[])result!;

                foreach (var entry in entries)
                {
                    if (entry.Type != ResultType.MultiBulk) continue;

                    var values = (RedisResult[])entry!;
                    if (values.Length < 2) continue;

                    if (!long.TryParse(values[0].ToString(), out var ts)) continue;
                    if (!int.TryParse(values[1].ToString(), out var val)) continue;

                    total += val;
                    var hour = DateTimeOffset.FromUnixTimeMilliseconds(ts).Hour;
                    var hourKey = $"{hour}:00";

                    if (!hourly.ContainsKey(hourKey))
                        hourly[hourKey] = 0;

                    hourly[hourKey] += val;
                }
            }


            if (total == 0) return null;

            return new DailySummaryDto
            {
                IntersectionId = intersectionId,
                Date = now.Date,
                AvgWaitTime = 0, // TODO: υπολογισμός όταν υπάρχουν timestamps φάσης
                PeakHours = hourly
                    .OrderByDescending(p => p.Value)
                    .Take(3)
                    .ToDictionary(p => p.Key, p => p.Value),
                TotalVehicleCount = total
            };
        }

        public async Task<List<string>> GetVehicleKeysForIntersection(string intersectionId)
        {
            var server = _database.Multiplexer
                .GetServer("redis", 6379); // Εναλλακτικά: μέσω config
            var keys = server
                .Keys(pattern: $"vehicle:{intersectionId}:*")
                .Select(k => k.ToString())
                .ToList();

            return await Task.FromResult(keys);
        }
    }
}
