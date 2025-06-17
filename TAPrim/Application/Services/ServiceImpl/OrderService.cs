using TAPrim.Application.DTOs;
using TAPrim.Application.DTOs.Order;
using TAPrim.Infrastructure.Repositories;
using TAPrim.Shared.Constants;

namespace TAPrim.Application.Services.ServiceImpl
{
	public class OrderService : IOrderService
	{
		private readonly IOrderRepository _orderRepository;
		public OrderService(IOrderRepository orderRepository)
		{
			_orderRepository = orderRepository;
		}
		public async Task<ApiResponseModel<object>> GetOrderByProductAccount(int productAccountId)
		{
			try
			{
				var result = await _orderRepository.FindByProductAccountId(productAccountId);
				if (result == null) {
					return new ApiResponseModel<object>
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Không tìm thấy đơn hàng nào",
						Data = result
					};
				}
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.SuccessStatus,
					Message = "Lấy danh sách đơn hàng thành công",
					Data = result
				};
			}
			catch (Exception ex)
			{
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = ex.Message
				};
			}
		}

		public async Task<ApiResponseModel<object>> UpdateOrder(UpdateOrderRequestDto orderUpdateRequest)
		{
			return null;
		}
	}
}
