using Microsoft.EntityFrameworkCore;
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

		public async Task SaveChange()
		{
			await _context.SaveChangesAsync();
		}
	}
}
