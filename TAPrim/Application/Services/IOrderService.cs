using TAPrim.Application.DTOs;
using TAPrim.Application.DTOs.Order;

namespace TAPrim.Application.Services
{
	public interface IOrderService
	{
		Task<ApiResponseModel<object>> GetOrderByProductAccount(int productAccount);
		Task<ApiResponseModel<object>> UpdateOrderAsync(string transactionCode, UpdateOrderRequestDto orderUpdateRequest);
	}
}
