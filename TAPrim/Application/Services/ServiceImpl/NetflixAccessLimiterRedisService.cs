using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using TAPrim.Application.DTOs.Tempmail;
using TAPrim.Application.Services;

public class NetflixAccessLimiterRedisService : INetflixAccessLimiterMemoryService
{
	private readonly IDistributedCache _redis;
	private readonly TimeSpan _mailCacheTtl = TimeSpan.FromMinutes(5);
	private const int MaxTotalAccess = 2;

	public NetflixAccessLimiterRedisService(IDistributedCache redis)
	{
		_redis = redis;
	}

	public async Task<bool> IsAllowedAsync(string transactionCode)
	{
		var countKey = $"limit:count:{transactionCode}";
		var countString = await _redis.GetStringAsync(countKey);
		var count = string.IsNullOrEmpty(countString) ? 0 : int.Parse(countString);

		return count < MaxTotalAccess;
	}

	public async Task RegisterRequestAsync(string transactionCode)
	{
		var countKey = $"limit:count:{transactionCode}";
		var countString = await _redis.GetStringAsync(countKey);
		var count = string.IsNullOrEmpty(countString) ? 0 : int.Parse(countString);
		count++;

		var options = new DistributedCacheEntryOptions
		{
			AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7) // giữ trong Redis ít nhất 7 ngày
		};

		await _redis.SetStringAsync(countKey, count.ToString(), options);

		// Lưu thời gian truy cập đầu tiên (chỉ lưu nếu chưa có)
		var timeKey = $"limit:first_access:{transactionCode}";
		if (string.IsNullOrEmpty(await _redis.GetStringAsync(timeKey)))
		{
			await _redis.SetStringAsync(timeKey, DateTime.UtcNow.ToString("O"), options);
		}
	}

	public async Task<DateTime?> GetFirstAccessTimeAsync(string transactionCode)
	{
		var str = await _redis.GetStringAsync($"limit:first_access:{transactionCode}");
		if (DateTime.TryParse(str, out var dt)) return dt;
		return null;
	}

	public async Task CacheEmailsAsync(string transactionCode, List<TempmailEmailItemDto> emails)
	{
		var data = JsonConvert.SerializeObject(emails);
		await _redis.SetStringAsync($"cached_email:{transactionCode}", data, new DistributedCacheEntryOptions
		{
			AbsoluteExpirationRelativeToNow = _mailCacheTtl
		});
	}

	public async Task<List<TempmailEmailItemDto>> GetCachedEmailsAsync(string transactionCode)
	{
		var data = await _redis.GetStringAsync($"cached_email:{transactionCode}");
		if (string.IsNullOrEmpty(data)) return null;

		return JsonConvert.DeserializeObject<List<TempmailEmailItemDto>>(data);
	}
}
