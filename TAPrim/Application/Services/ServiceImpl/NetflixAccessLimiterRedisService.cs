using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using TAPrim.Application.DTOs.Tempmail;
using TAPrim.Application.Services;

public class NetflixAccessLimiterRedisService : INetflixAccessLimiterMemoryService
{
	private readonly IDistributedCache _redis;
	private readonly TimeSpan _mailCacheTtl = TimeSpan.FromMinutes(5);
	private const int MaxSessionAccess = 5;

	public NetflixAccessLimiterRedisService(IDistributedCache redis)
	{
		_redis = redis;
	}

	public async Task<bool> IsAllowedAsync(string transactionCode)
	{
		var sessionKey = $"limit:session_count:{transactionCode}";
		var countString = await _redis.GetStringAsync(sessionKey);
		var count = string.IsNullOrEmpty(countString) ? 0 : int.Parse(countString);

		// Chỉ cho phép tối đa 2 lần truy cập cho mỗi lần cache
		return count < MaxSessionAccess;
	}

	public async Task RegisterRequestAsync(string transactionCode)
	{
		var sessionKey = $"limit:session_count:{transactionCode}";
		var countString = await _redis.GetStringAsync(sessionKey); 
		var count = string.IsNullOrEmpty(countString) ? 0 : int.Parse(countString);
		count++;

		// Thời gian sống cho lượt truy cập này giống TTL của cache
		var options = new DistributedCacheEntryOptions
		{
			AbsoluteExpirationRelativeToNow = _mailCacheTtl
		};

		await _redis.SetStringAsync(sessionKey, count.ToString(), options);
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
		var options = new DistributedCacheEntryOptions
		{
			AbsoluteExpirationRelativeToNow = _mailCacheTtl
		};

		// Lưu nội dung email
		await _redis.SetStringAsync($"cached_email:{transactionCode}", data, options);

		// Lưu thời gian bắt đầu cache
		await _redis.SetStringAsync($"cached_email_time:{transactionCode}", DateTime.UtcNow.ToString("O"), options);

		// Reset lại số lượt truy cập của phiên cache này
		await _redis.RemoveAsync($"limit:session_count:{transactionCode}");
	}

	public async Task<List<TempmailEmailItemDto>> GetCachedEmailsAsync(string transactionCode)
	{
		// Kiểm tra thời gian cache còn hợp lệ không
		var timeStr = await _redis.GetStringAsync($"cached_email_time:{transactionCode}");
		if (!DateTime.TryParse(timeStr, out var cachedAt)) return null;

		if ((DateTime.UtcNow - cachedAt) > TimeSpan.FromMinutes(2))
			return null;

		var data = await _redis.GetStringAsync($"cached_email:{transactionCode}");
		if (string.IsNullOrEmpty(data)) return null;

		return JsonConvert.DeserializeObject<List<TempmailEmailItemDto>>(data);
	}
}
