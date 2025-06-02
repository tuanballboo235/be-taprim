using System.Net.Http;
using System.Text.Json;
using System.Text;
using TAPrim.Shared.Constants;
using TAPrim.Application.DTOs.Payment;
using TAPrim.Application.DTOs;

namespace TAPrim.Application.Services
{
	public interface IPaymentService
	{
		Task<ApiResponseModel<object>> GenerateQrAsync(CreateOrderRequest createOrderRequest);
		Task<ApiResponseModel<object>> SetProductAccountForPaymentByTransactionCode(SePayWebhookDto response);
	}
}
