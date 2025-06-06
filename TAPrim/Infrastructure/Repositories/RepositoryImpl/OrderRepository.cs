using Microsoft.EntityFrameworkCore;
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

		//hàm tìm kiếm Order theo Transaction Code 
		public async Task<Order?> FindByPaymentTransactionCodeAsync(string transactionCode)
		{
			return await _context.Payments
								 .Where(p => p.TransactionCode == transactionCode)
								 .Select(p => p.Order)
								 .FirstOrDefaultAsync();
		}

		public async Task SaveChange()
		{
			await _context.SaveChangesAsync();
		}
	}
}
