using TAPrim.Models;

namespace TAPrim.Infrastructure.Repositories
{
	public interface IOrderRepository
	{
		Task AddOrderAsync(Order orders);
		Task<Order?> FindByPaymentTransactionCodeAsync(string transactionCode);
		Task<Order?> FindByProductAccountId(int productAccountId);
		Task SaveChange();
	}
}
