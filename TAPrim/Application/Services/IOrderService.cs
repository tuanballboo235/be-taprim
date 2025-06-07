using TAPrim.Application.DTOs;

namespace TAPrim.Application.Services
{
	public interface IOrderService
	{
		Task<ApiResponseModel<object>> GetOrderByProductAccount(int productAccount);
	}
}
