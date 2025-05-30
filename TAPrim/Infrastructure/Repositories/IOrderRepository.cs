using TAPrim.Models;

namespace TAPrim.Infrastructure.Repositories
{
	public interface IOrderRepository
	{
		Task AddOrderAsync(Order orders);

	}
}
