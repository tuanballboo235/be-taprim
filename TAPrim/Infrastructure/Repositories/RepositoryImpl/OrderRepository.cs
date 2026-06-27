using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TAPrim.Application.DTOs.Order;
using TAPrim.Application.DTOs.Payment;
using TAPrim.Models;
using TAPrim.Shared.Constants;

namespace TAPrim.Infrastructure.Repositories.RepositoryImpl
{
	public class OrderRepository : IOrderRepository
	{
		private readonly TaprimContext _context;

		public OrderRepository(TaprimContext context)
		{
			_context = context;
		}

		public async Task AddOrderAsync(Order orders)
		{
			_context.Orders.Add(orders);
			await _context.SaveChangesAsync();
		}

		public async Task<Order?> FindByPaymentTransactionCodeAsync(string transactionCode)
		{
			return await _context.Orders
				.Include(p => p.Payment)
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
			var order = await _context.Orders
				.Include(x => x.Payment)
				.Include(x => x.Coupon)
				.Include(x => x.ProductOption).ThenInclude(x => x.Product)
				.Include(x => x.ProductAccount)
				.Where(x => x.OrderId == orderId)
				.Select(x => new OrderResponseDto
				{
					OrderId = x.OrderId,
					CouponId = x.CouponId,
					CouponCode = x.Coupon != null ? x.Coupon.CouponCode ?? "N/A" : "N/A",
					CouponDiscountPersent = x.Coupon != null ? x.Coupon.DiscountPercent : null,
					ProductOptionId = x.ProductOptionId,
					ProductId = x.ProductOption.ProductId,
					ProductName = x.ProductOption.Product.ProductName,
					ProductOptionLabel = x.ProductOption.Label,
					ProductAccountId = x.ProductAccountId,
					ProductAccountData = x.ProductAccount != null ? x.ProductAccount.AccountData ?? "N/A" : "N/A",
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

			if (order == null)
			{
				return null;
			}

			ApplyPricingSnapshot(order);
			return order;
		}

		public async Task<AdminProductOrderResponseDto> GetAdminProductOrdersAsync(AdminProductOrderFilterDto filter)
		{
			var page = filter.Page <= 0 ? 1 : filter.Page;
			var pageSize = filter.PageSize <= 0 ? 20 : Math.Min(filter.PageSize, 100);
			var fromDate = filter.FromDate?.Date;
			var toDateExclusive = filter.ToDate?.Date.AddDays(1);
			var keyword = filter.Keyword?.Trim();

			var query = _context.Orders
				.AsNoTracking()
				.AsQueryable();

			if (fromDate.HasValue)
			{
				query = query.Where(x => (x.Payment.PaidDateAt ?? x.CreateAt) >= fromDate.Value);
			}

			if (toDateExclusive.HasValue)
			{
				query = query.Where(x => (x.Payment.PaidDateAt ?? x.CreateAt) < toDateExclusive.Value);
			}

			if (filter.OrderStatus.HasValue)
			{
				query = query.Where(x => x.Status == filter.OrderStatus);
			}

			if (filter.PaymentStatus.HasValue)
			{
				query = query.Where(x => x.Payment.Status == filter.PaymentStatus);
			}

			if (!string.IsNullOrWhiteSpace(keyword))
			{
				query = query.Where(x =>
					x.Payment.TransactionCode.Contains(keyword) ||
					(x.ContactInfo != null && x.ContactInfo.Contains(keyword)) ||
					x.ProductOption.Product.ProductName.Contains(keyword) ||
					(x.ProductOption.Label != null && x.ProductOption.Label.Contains(keyword)));
			}

			var rows = await query
				.OrderByDescending(x => x.Payment.PaidDateAt ?? x.CreateAt)
				.ThenByDescending(x => x.OrderId)
				.Select(x => new AdminProductOrderRawDto
				{
					OrderId = x.OrderId,
					PaymentId = x.PaymentId,
					PaymentTransactionCode = x.Payment.TransactionCode,
					ProductId = x.ProductOption.ProductId,
					ProductName = x.ProductOption.Product.ProductName,
					ProductOptionId = x.ProductOptionId,
					ProductOptionLabel = x.ProductOption.Label,
					ProductAccountId = x.ProductAccountId,
					ProductAccountData = x.ProductAccount != null ? x.ProductAccount.AccountData : null,
					Quantity = 1,
					ContactInfo = x.ContactInfo,
					OrderStatus = x.Status,
					PaymentStatus = x.Payment.Status,
					PaymentMethod = x.Payment.PaymentMethod,
					CreateAt = x.CreateAt,
					PaidAt = x.Payment.PaidDateAt,
					ExpiredAt = x.ExpiredAt,
					TotalAmount = x.TotalAmount,
					CouponCode = x.Coupon != null ? x.Coupon.CouponCode : null,
					CouponDiscountPercent = x.Coupon != null ? x.Coupon.DiscountPercent : null,
					ClientNote = x.ClientNote
				})
				.ToListAsync();

			var items = rows
				.Select(x => x.ToItemDto(ReadReservationMetadata(x.ClientNote)))
				.ToList();
			var paidItems = items
				.Where(x => x.PaymentStatus == PaymentConstatnt.Paid)
				.ToList();
			var totalRevenue = paidItems.Sum(x => x.TotalAmount ?? 0);
			var totalRecords = items.Count;

			return new AdminProductOrderResponseDto
			{
				Items = items
					.Skip((page - 1) * pageSize)
					.Take(pageSize)
					.ToList(),
				Summary = new AdminProductOrderSummaryDto
				{
					TotalOrders = totalRecords,
					PaidOrders = paidItems.Count,
					PendingOrders = items.Count(x => x.PaymentStatus == PaymentConstatnt.Pending),
					TotalQuantity = paidItems.Sum(x => x.Quantity),
					TotalRevenue = totalRevenue,
					AverageOrderValue = paidItems.Count == 0 ? 0 : totalRevenue / paidItems.Count,
					FromDate = fromDate,
					ToDate = filter.ToDate?.Date
				},
				TotalRecords = totalRecords,
				TotalPages = totalRecords == 0 ? 0 : (int)Math.Ceiling(totalRecords / (double)pageSize),
				CurrentPage = page,
				PageSize = pageSize
			};
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

		private static PaymentReservationMetadata? ReadReservationMetadata(string? clientNote)
		{
			if (string.IsNullOrWhiteSpace(clientNote))
			{
				return null;
			}

			try
			{
				return JsonSerializer.Deserialize<PaymentReservationMetadata>(clientNote);
			}
			catch (JsonException)
			{
				return null;
			}
		}

		private static int GetQuantity(PaymentReservationMetadata? metadata)
		{
			if (metadata == null)
			{
				return 1;
			}

			if (metadata.Quantity > 0)
			{
				return metadata.Quantity;
			}

			var itemQuantity = metadata.Items?.Sum(x => x.Quantity) ?? 0;
			return itemQuantity > 0 ? itemQuantity : 1;
		}

		private static void ApplyPricingSnapshot(OrderResponseDto order)
		{
			var metadata = ReadReservationMetadata(order.ClientNote);
			order.Quantity = GetQuantity(metadata);

			if (metadata == null)
			{
				order.OriginalAmount = order.TotalAmount;
				order.TransactionFee = 0;
				order.DiscountAmount = 0;
				return;
			}

			order.OriginalAmount = metadata.OriginalAmount > 0
				? metadata.OriginalAmount
				: order.TotalAmount;
			order.TransactionFee = metadata.TransactionFee;
			order.DiscountAmount = metadata.DiscountAmount;
			order.CouponId = metadata.CouponId ?? order.CouponId;
			order.CouponCode = !string.IsNullOrWhiteSpace(metadata.CouponCode)
				? metadata.CouponCode
				: order.CouponCode;
			order.CouponDiscountPersent = metadata.CouponDiscountPercent ?? order.CouponDiscountPersent;
		}

		private class AdminProductOrderRawDto : AdminProductOrderItemDto
		{
			public string? ClientNote { get; set; }

			public AdminProductOrderItemDto ToItemDto(PaymentReservationMetadata? metadata)
			{
				var quantity = GetQuantity(metadata);
				return new AdminProductOrderItemDto
				{
					OrderId = OrderId,
					PaymentId = PaymentId,
					PaymentTransactionCode = PaymentTransactionCode,
					ProductId = ProductId,
					ProductName = ProductName,
					ProductOptionId = ProductOptionId,
					ProductOptionLabel = ProductOptionLabel,
					ProductAccountId = ProductAccountId,
					ProductAccountData = ProductAccountData,
					Quantity = quantity,
					ContactInfo = ContactInfo,
					OrderStatus = OrderStatus,
					PaymentStatus = PaymentStatus,
					PaymentMethod = PaymentMethod,
					CreateAt = CreateAt,
					PaidAt = PaidAt,
					ExpiredAt = ExpiredAt,
					OriginalAmount = metadata?.OriginalAmount > 0 ? metadata.OriginalAmount : TotalAmount,
					TransactionFee = metadata?.TransactionFee ?? 0,
					DiscountAmount = metadata?.DiscountAmount ?? 0,
					TotalAmount = TotalAmount,
					CouponCode = !string.IsNullOrWhiteSpace(metadata?.CouponCode) ? metadata.CouponCode : CouponCode,
					CouponDiscountPercent = metadata?.CouponDiscountPercent ?? CouponDiscountPercent
				};
			}
		}
	}
}
