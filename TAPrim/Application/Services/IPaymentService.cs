using System.Net.Http;
using System.Text.Json;
using System.Text;
using TAPrim.Shared.Constants;
using TAPrim.Application.DTOs.Payment;
using TAPrim.Application.DTOs.Products;
using TAPrim.Application.DTOs.Common;

namespace TAPrim.Application.Services
{
    public interface IPaymentService
	{
		Task<ApiResponseModel<object>> GenerateQrAsync(CreatePaymentRequest createOrderRequest);
		Task<ApiResponseModel<object>> SetProductAccountForPaymentByTransactionCode(SePayWebhookDto data);
		Task<ApiResponseModel<object>> GetPaymentsAsync(PaymentFilterDto filter);
		Task<ApiResponseModel<object>> TestEmail();
		Task<ApiResponseModel<object>> ClearOrderAndPaymentTempByPaymentId(int paymentId);
	}
}
