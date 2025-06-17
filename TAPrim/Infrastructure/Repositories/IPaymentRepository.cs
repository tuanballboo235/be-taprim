using TAPrim.Application.DTOs.Payment;
using TAPrim.Models;

namespace TAPrim.Infrastructure.Repositories
{
    public interface IPaymentRepository
    {
		Task AddPaymentAsync(Payment payment);
		Task<Payment?> GetPaymentByTransactionCode(string transactionCode);
		Task<bool> IsExistedTransactionCode(string transactionCode);
		Task<List<PaymentDetailsDto>> GetPaymentsAsync(PaymentFilterDto filter);

		Task SaveChange();
	}
}
