using BasketballAcademyManagementSystemAPI.Common.Helpers;
using Microsoft.Extensions.Caching.Memory;
using TAPrim.Application.DTOs.Common;
using TAPrim.Application.DTOs.Order;
using TAPrim.Infrastructure.Repositories;
using TAPrim.Shared.Constants;

namespace TAPrim.Application.Services.ServiceImpl
{
	public class OrderService : IOrderService
	{
		private const string OrderLookupCachePrefix = "order-lookup-code:";
		private readonly IOrderRepository _orderRepository;
		private readonly IMemoryCache _memoryCache;
		private readonly EmailHelper _emailHelper;

		public OrderService(
			IOrderRepository orderRepository,
			IMemoryCache memoryCache,
			EmailHelper emailHelper)
		{
			_orderRepository = orderRepository;
			_memoryCache = memoryCache;
			_emailHelper = emailHelper;
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
				if (order == null)
				{
					return new ApiResponseModel<object>
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Không tìm thấy đơn"
					};
				}

				order.ProductAccountId = orderUpdateRequest.ProductAccountId ?? order.ProductAccountId;
				order.Status = orderUpdateRequest.Status ?? order.Status;
				order.RemainGetCode = orderUpdateRequest.RemainCode ?? order.RemainGetCode;
				order.ExpiredAt = orderUpdateRequest.ExpiredAt ?? order.ExpiredAt;
				order.ContactInfo = orderUpdateRequest.ContactInfo ?? order.ContactInfo;
				order.TotalAmount = orderUpdateRequest.TotalAmount ?? order.TotalAmount;

				await _orderRepository.UpdateOrderAsync(order);

				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.SuccessStatus,
					Message = "Cập nhật đơn hàng thành công"
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

		public async Task<ApiResponseModel<object>> SendOrderLookupVerificationCode(string transactionCode)
		{
			try
			{
				var normalizedTransactionCode = NormalizeTransactionCode(transactionCode);
				if (string.IsNullOrWhiteSpace(normalizedTransactionCode))
				{
					return new ApiResponseModel<object>
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Vui lòng nhập mã giao dịch"
					};
				}

				var existingOrder = await _orderRepository.FindByPaymentTransactionCodeAsync(normalizedTransactionCode);
				if (existingOrder == null)
				{
					return new ApiResponseModel<object>
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Không tìm thấy đơn"
					};
				}

				var email = existingOrder.ContactInfo?.Trim();
				if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
				{
					return new ApiResponseModel<object>
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Đơn hàng chưa có email hợp lệ để gửi mã xác nhận"
					};
				}

				var verificationCode = Random.Shared.Next(100000, 999999).ToString();
				_memoryCache.Set(
					GetOrderLookupCacheKey(normalizedTransactionCode),
					verificationCode,
					TimeSpan.FromMinutes(10));

				var body = $@"
					<div style=""font-family:Arial,sans-serif;color:#0f172a;line-height:1.6"">
						<h2 style=""margin:0 0 12px"">Mã xác nhận tra cứu đơn hàng TAPRIM</h2>
						<p>Mã giao dịch: <strong>{normalizedTransactionCode}</strong></p>
						<p>Mã xác nhận của bạn là:</p>
						<div style=""font-size:28px;font-weight:700;letter-spacing:6px;color:#15803d"">{verificationCode}</div>
						<p>Mã có hiệu lực trong 10 phút. Vui lòng không chia sẻ mã này cho người khác.</p>
					</div>";

				await _emailHelper.SendEmailAsync(email, "Mã xác nhận tra cứu đơn hàng TAPRIM", body);

				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.SuccessStatus,
					Message = "Đã gửi mã xác nhận về email của đơn hàng",
					Data = new
					{
						Email = MaskEmail(email),
						ExpiresInMinutes = 10
					}
				};
			}
			catch (Exception ex)
			{
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = "Không thể gửi mã xác nhận",
					Errors = new Dictionary<string, string> { { "Exception", ex.Message } }
				};
			}
		}

		public async Task<ApiResponseModel<object>> GetOrderDetailsByTransactionCode(string transactionCode, string verificationCode)
		{
			try
			{
				var normalizedTransactionCode = NormalizeTransactionCode(transactionCode);
				if (string.IsNullOrWhiteSpace(normalizedTransactionCode))
				{
					return new ApiResponseModel<object>
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Vui lòng nhập mã giao dịch"
					};
				}

				if (string.IsNullOrWhiteSpace(verificationCode))
				{
					return new ApiResponseModel<object>
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Vui lòng nhập mã xác nhận"
					};
				}

				if (!_memoryCache.TryGetValue(GetOrderLookupCacheKey(normalizedTransactionCode), out string? cachedCode))
				{
					return new ApiResponseModel<object>
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Mã xác nhận đã hết hạn hoặc chưa được gửi"
					};
				}

				if (!string.Equals(cachedCode, verificationCode.Trim(), StringComparison.Ordinal))
				{
					return new ApiResponseModel<object>
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Mã xác nhận không đúng"
					};
				}

				var existingOrder = await _orderRepository.FindByPaymentTransactionCodeAsync(normalizedTransactionCode);
				if (existingOrder == null)
				{
					return new ApiResponseModel<object>
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Không tìm thấy đơn"
					};
				}

				var order = await _orderRepository.GetOrderDetailsById(existingOrder.OrderId);
				if (order == null)
				{
					return new ApiResponseModel<object>
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Không tìm thấy đơn"
					};
				}

				_memoryCache.Remove(GetOrderLookupCacheKey(normalizedTransactionCode));

				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.SuccessStatus,
					Data = order
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

		public async Task<ApiResponseModel<object>> GetAdminProductOrdersAsync(AdminProductOrderFilterDto filter)
		{
			try
			{
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.SuccessStatus,
					Message = "Lấy danh sách đơn hàng sản phẩm thành công",
					Data = await _orderRepository.GetAdminProductOrdersAsync(filter)
				};
			}
			catch (Exception ex)
			{
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = "Không thể lấy danh sách đơn hàng sản phẩm",
					Errors = new Dictionary<string, string> { { "Exception", ex.Message } }
				};
			}
		}
		public async Task<ApiResponseModel<object>> GetUserProductOrdersAsync(int userId)
		{
			try
			{
				if (userId <= 0)
				{
					return new ApiResponseModel<object>
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Phiên đăng nhập không hợp lệ"
					};
				}

				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.SuccessStatus,
					Message = "Lấy danh sách đơn hàng của bạn thành công",
					Data = await _orderRepository.GetUserProductOrdersAsync(userId)
				};
			}
			catch (Exception ex)
			{
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = "Không thể lấy danh sách đơn hàng của bạn",
					Errors = new Dictionary<string, string> { { "Exception", ex.Message } }
				};
			}
		}

		public async Task<ApiResponseModel<object>> DeleteOrderById(int orderId)
		{
			try
			{
				await _orderRepository.DeleteOrderById(orderId);
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.SuccessStatus
				};
			}
			catch
			{
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.FailedStatus
				};
			}
		}

		public async Task<ApiResponseModel<object>> DeleteOrderByPaymentId(int paymentId)
		{
			try
			{
				await _orderRepository.DeleteOrderByPaymentId(paymentId);
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.SuccessStatus
				};
			}
			catch
			{
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.FailedStatus
				};
			}
		}

		private static string NormalizeTransactionCode(string transactionCode)
		{
			return transactionCode?.Trim().ToUpperInvariant() ?? string.Empty;
		}

		private static string GetOrderLookupCacheKey(string transactionCode)
		{
			return $"{OrderLookupCachePrefix}{NormalizeTransactionCode(transactionCode)}";
		}

		private static string MaskEmail(string email)
		{
			var parts = email.Split('@', 2);
			if (parts.Length != 2) return email;

			var name = parts[0];
			var domain = parts[1];
			var visibleName = name.Length <= 2 ? name[..1] : name[..Math.Min(3, name.Length)];
			return $"{visibleName}***@{domain}";
		}
	}
}
