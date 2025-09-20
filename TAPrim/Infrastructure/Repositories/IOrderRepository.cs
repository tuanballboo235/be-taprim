using Microsoft.EntityFrameworkCore;
using TAPrim.Application.DTOs.Order;
using TAPrim.Models;

namespace TAPrim.Infrastructure.Repositories
{
	public interface IOrderRepository
	{
		Task AddOrderAsync(Order orders);
		Task<Order?> FindByPaymentTransactionCodeAsync(string transactionCode);
		Task<Order?> FindByProductAccountId(int productAccountId);
		Task<bool> UpdateOrderAsync(Order order);
		Task<OrderResponseDto?> GetOrderDetailsById(int orderId); 
		Task DeleteOrderById(int orderId);
		Task DeleteOrderByPaymentId(int paymentId);
		Task SaveChange();
	}
}
