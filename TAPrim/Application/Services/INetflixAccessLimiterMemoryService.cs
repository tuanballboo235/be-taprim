using TAPrim.Application.DTOs.Tempmail;

namespace TAPrim.Application.Services
{
	public interface INetflixAccessLimiterMemoryService
	{
		Task<bool> IsAllowedAsync(string transactionCode);
		Task RegisterRequestAsync(string transactionCode);
		Task CacheEmailsAsync(string transactionCode, List<TempmailEmailItemDto> emails);
		Task<List<TempmailEmailItemDto>> GetCachedEmailsAsync(string transactionCode);
	}
}
