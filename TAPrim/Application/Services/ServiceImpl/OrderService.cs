using Azure.Core;
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
				if (result == null)
				{
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



		public async Task<ApiResponseModel<object>> UpdateOrderAsync(string transactionCode, UpdateOrderRequestDto orderUpdateRequest)
		{
			try
			{
				var order = await _orderRepository.FindByPaymentTransactionCodeAsync(transactionCode);
				if (order == null) return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = "Không tìm thấy đơn"
				};

				// Ánh xạ dữ liệu từ DTO vào Entity
				order.ProductAccountId = orderUpdateRequest.ProductAccountId ?? order.ProductAccountId;
				order.Status = orderUpdateRequest.Status ?? order.Status;
				order.RemainGetCode = orderUpdateRequest.RemainCode ?? order.RemainGetCode;
				order.ExpiredAt = orderUpdateRequest.ExpiredAt ?? order.ExpiredAt;
				order.ContactInfo = orderUpdateRequest.ContactInfo ?? order.ContactInfo;
				order.TotalAmount = orderUpdateRequest.TotalAmount?? order.TotalAmount;

				await _orderRepository.UpdateOrderAsync(order);
				var result = new OrderResponseDto
				{
					OrderId = order.OrderId,
					CouponId = order.CouponId,
					ProductId = order.ProductId,
					ProductAccountId = order.ProductAccountId,
					Status = order.Status,
					CreateAt = order.CreateAt,
					UpdateAt = order.UpdateAt,
					RemainGetCode = order.RemainGetCode,
					ExpiredAt = order.ExpiredAt,
					ContactInfo = order.ContactInfo,
					PaymentId = order.PaymentId,
					TotalAmount = order.TotalAmount,
					ClientNote = order.ClientNote
				};
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.SuccessStatus,
					Message = "Cập nhật đơn hàng thành công",
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
	}
}
