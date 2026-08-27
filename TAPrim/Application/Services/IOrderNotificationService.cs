using TAPrim.Models;

namespace TAPrim.Application.Services
{
	public interface IOrderNotificationService
	{
		Task<bool> SendOrderUpdateNotificationAsync(Order order, string? customMessage);
		Task<bool> SendProductAccountUpdateNotificationAsync(Order order, ProductAccount productAccount, string? customMessage);
	}
}