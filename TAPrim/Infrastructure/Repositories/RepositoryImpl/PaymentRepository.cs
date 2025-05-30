using TAPrim.Models;

namespace TAPrim.Infrastructure.Repositories.RepositoryImpl
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly TaprimContext _context;
        public PaymentRepository(TaprimContext context) { 
            _context = context;
        
        }

		public async Task AddPaymentAsync(Payment payments)
		{
			_context.Payments.Add(payments);
			await _context.SaveChangesAsync();
		}
	}
}
