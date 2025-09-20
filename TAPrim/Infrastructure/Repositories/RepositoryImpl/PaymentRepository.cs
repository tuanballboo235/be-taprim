using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using TAPrim.Application.DTOs.Payment;
using TAPrim.Models;

namespace TAPrim.Infrastructure.Repositories.RepositoryImpl
{
	public class PaymentRepository : IPaymentRepository
	{
		private readonly TaprimContext _context;
		public PaymentRepository(TaprimContext context)
		{
			_context = context;
		}

		public async Task AddPaymentAsync(Payment payments)
		{
			_context.Payments.Add(payments);
			await _context.SaveChangesAsync();
		}

		public async Task<Payment?> GetPaymentByTransactionCode(string transactionCode)
		{
			return await _context.Payments.FirstOrDefaultAsync(x => x.TransactionCode == transactionCode);

		}
		public async Task<bool> IsExistedTransactionCode(string transactionCode)
		{
			return await _context.Payments.AnyAsync(x => x.TransactionCode == transactionCode);
		}
		public async Task<List<PaymentDetailsDto>> GetPaymentsAsync(PaymentFilterDto filter)
		{
			var query = _context.Payments.AsQueryable();

			if (!string.IsNullOrEmpty(filter.TransactionCode))
				query = query.Where(p => p.TransactionCode.Contains(filter.TransactionCode));

			if (filter.UserId.HasValue)
				query = query.Where(p => p.UserId == filter.UserId);

			if (filter.PaymentMethod.HasValue)
				query = query.Where(p => p.PaymentMethod == filter.PaymentMethod);

			if (filter.Status.HasValue)
				query = query.Where(p => p.Status == filter.Status);

			if (filter.PaidAt.HasValue)
				query = query.Where(p => p.PaidDateAt >= filter.PaidAt);

			if (filter.Amount.HasValue)
				query = query.Where(p => p.Amount <= filter.Amount);

			return await query.Select(p => new PaymentDetailsDto
			{
				PaymentId = p.PaymentId,
				TransactionCode = p.TransactionCode,
				PaymentMethod = p.PaymentMethod,
				PaidDateAt = p.PaidDateAt,
				CreateAt = p.CreateAt,
				UserId = p.UserId,
				Amount = p.Amount,
				Note = p.Note,
				Status = p.Status
			}).ToListAsync();
		}
		public async Task SaveChange()
		{
			await _context.SaveChangesAsync();
		}

		public async Task DeletePaymentById(int paymentId)
		{
			var payments = await _context.Payments
						.FirstOrDefaultAsync(o => o.PaymentId == paymentId);

			if (payments != null)
			{
				_context.Payments.Remove(payments);
				await _context.SaveChangesAsync();
			}
		}
	}
}
