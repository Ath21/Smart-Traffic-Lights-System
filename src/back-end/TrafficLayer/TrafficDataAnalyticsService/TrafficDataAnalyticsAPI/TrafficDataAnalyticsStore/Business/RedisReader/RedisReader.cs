using Microsoft.Extensions.Options;
using StackExchange.Redis;
using TrafficDataAnalyticsData.Redis;
using TrafficDataAnalyticsStore.Models;

namespace TrafficDataAnalyticsStore.Business.RedisReader
{
    public class RedisReader : IRedisReader
    {
        private readonly ILogger<RedisReader> _logger;
        private readonly RedisDbContext _context;
        private readonly RedisDbSettings _settings;
        private readonly IDatabase _db;

        public RedisReader(
            ILogger<RedisReader> logger,
            RedisDbContext context,
            IOptions<RedisDbSettings> settings)
        {
            _logger = logger;
            _context = context;
            _db = context.Database;
            _settings = settings.Value;
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
                var result = await _db.ExecuteAsync("TS.RANGE", key, start, end);

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
                AvgWaitTime = 0,
                PeakHours = hourly
                    .OrderByDescending(p => p.Value)
                    .Take(3)
                    .ToDictionary(p => p.Key, p => p.Value),
                TotalVehicleCount = total
            };
        }

        public async Task<List<string>> GetVehicleKeysForIntersection(string intersectionId)
        {
            var server = _context.GetServer(_settings); // το παίρνεις έτοιμο από context

            var keys = server
                .Keys(pattern: $"vehicle:{intersectionId}:*")
                .Select(k => k.ToString())
                .ToList();

            return await Task.FromResult(keys);
        }
    }
}
