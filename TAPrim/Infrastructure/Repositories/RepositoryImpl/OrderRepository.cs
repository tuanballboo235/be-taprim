using Microsoft.EntityFrameworkCore;
using TAPrim.Application.DTOs.Order;
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
			return await _context.Orders.Include(p => p.Payment)
								 .Where(p => p.Payment.TransactionCode == transactionCode)
								 .FirstOrDefaultAsync();
			
		}

		public async Task<Order?> FindByProductAccountId(int productAccountId)
		{
			return await _context.Orders
								 .Where(o => o.ProductAccountId == productAccountId)
								 .FirstOrDefaultAsync();
		}

		public async Task<OrderResponseDto?> GetOrderDetailsById(int orderId)
		{
			return await _context.Orders
				.Include(x=>x.Payment)
				.Include(x=>x.Coupon)
				.Include(x=>x.ProductOption).ThenInclude(x=>x.Product)
				.Include(x=>x.ProductAccount)
				.Where(x=>x.OrderId == orderId)
				.Select(x=> new OrderResponseDto
				{
					OrderId = x.OrderId,
					CouponId = x.CouponId,
					CouponCode = x.Coupon.CouponCode ??"N/A",
					CouponDiscountPersent = x.Coupon.DiscountPercent,
					ProductId = x.ProductOption.ProductId,
					ProductName = x.ProductOption.Product.ProductName,
					ProductOptionLabel = x.ProductOption.Label,
					ProductAccountId = x.ProductAccountId,
					ProductAccountData = x.ProductAccount.AccountData ?? "N/A",
					Status = x.Status,	
					CreateAt = x.CreateAt,
					RemainGetCode = x.RemainGetCode,
					ExpiredAt = x.ExpiredAt,
					PaymentTransactionCode = x.Payment.TransactionCode,
					ContactInfo = x.ContactInfo,
					PaidAt = x.Payment.PaidDateAt,
					TotalAmount = x.TotalAmount,
					ClientNote = x.ClientNote
				})
				.FirstOrDefaultAsync();
		}

		public async Task SaveChange()
		{
			await _context.SaveChangesAsync();
		}
		public async Task DeleteOrderById(int orderId)
		{
			var order = await _context.Orders.FindAsync(orderId);
			if (order != null)
			{
				_context.Orders.Remove(order);
				await _context.SaveChangesAsync();
			}
		}

		public async Task DeleteOrderByPaymentId(int paymentId)
		{
			var order = await _context.Orders
				.FirstOrDefaultAsync(o => o.PaymentId == paymentId);

			if (order != null)
			{
				_context.Orders.Remove(order);
				await _context.SaveChangesAsync();
			}
		}


		public async Task<bool> UpdateOrderAsync(Order order)
		{
			_context.Orders.Update(order);
			return await _context.SaveChangesAsync() > 0;
		}
	}
}
