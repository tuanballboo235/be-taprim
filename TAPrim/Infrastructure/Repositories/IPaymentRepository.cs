using TAPrim.Models;

namespace TAPrim.Infrastructure.Repositories
{
    public interface IPaymentRepository
    {
		Task AddPaymentAsync(Payment payment);
		Task<bool> IsExistedTransactionCode(string transactionCode);
		
	}
}
