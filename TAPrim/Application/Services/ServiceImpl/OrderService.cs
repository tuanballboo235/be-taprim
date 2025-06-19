using Azure.Core;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using TAPrim.Application.DTOs.Common;
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
					CouponCode = order.Coupon.CouponCode ?? "N/A",
					CouponDiscountPersent = order.Coupon.DiscountPercent,
					ProductId = order.ProductId,
					ProductName = order.Product.ProductName,
					ProductAccountId = order.ProductAccountId,
					ProductAccountData = order.ProductAccount.AccountData ?? "N/A",
					Status = order.Status,
					CreateAt = order.CreateAt,
					RemainGetCode = order.RemainGetCode,
					ExpiredAt = order.ExpiredAt,
					PaymentTransactionCode = order.Payment.TransactionCode,
					ContactInfo = order.ContactInfo,
					PaidAt = order.Payment.PaidDateAt,
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
		public async Task<ApiResponseModel<object>> GetOrderDetailsByTransactionCode(string transactionCode)
		{
			try {
				var order = await _orderRepository.GetOrderDetailsById((await _orderRepository.FindByPaymentTransactionCodeAsync(transactionCode)).OrderId);
				if (order == null) 
				{
					return new ApiResponseModel<object>()
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Không tìm thấy đơn"
					};
				
				}else
				{
					return new ApiResponseModel<object>
					{
						Status = ApiResponseStatusConstant.SuccessStatus,
						Data = order
					};
				}
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
