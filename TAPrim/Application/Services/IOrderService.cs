using TAPrim.Application.DTOs.Common;
using TAPrim.Application.DTOs.Order;

namespace TAPrim.Application.Services
{
    public interface IOrderService
	{
		Task<ApiResponseModel<object>> GetOrderByProductAccount(int productAccount);
		Task<ApiResponseModel<object>> UpdateOrderAsync(string transactionCode, UpdateOrderRequestDto orderUpdateRequest);
		Task<ApiResponseModel<object>> GetOrderDetailsByTransactionCode(string transactionCode);
		Task<ApiResponseModel<object>> DeleteOrderById(int orderId);
		Task<ApiResponseModel<object>> DeleteOrderByPaymentId(int paymentId);
	}
}
