using TAPrim.Models;

namespace TAPrim.Infrastructure.Repositories.RepositoryImpl
{
	public class OrderRepository:IOrderRepository
	{
		private readonly TaprimContext _context;
		public OrderRepository(TaprimContext context) { 
		_context = context;
		}

		public async Task AddOrderAsync(Order orders)
		{
			_context.Orders.Add(orders);
			await _context.SaveChangesAsync();
		}
	}
}
