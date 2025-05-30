using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;
using TAPrim.Application.DTOs;
using TAPrim.Infrastructure.Repositories;
using TAPrim.Shared.Constants;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using TAPrim.Models;
using TAPrim.Application.DTOs.Payment;

namespace TAPrim.Application.Services.ServiceImpl
{
	public class PaymentService : IPaymentService
	{
		private readonly HttpClient _httpClient;
		private readonly VietQrDto _options;
		private readonly IPaymentRepository _paymentRepository;
		private readonly IOrderRepository _orderRepository;

		public PaymentService(HttpClient httpClient, IOptions<VietQrDto> options, IPaymentRepository paymentRepository, IOrderRepository orderRepository)
		{
			_httpClient = httpClient;
			_options = options.Value;
			_paymentRepository = paymentRepository;
			_orderRepository = orderRepository;
		}

		public async Task<ApiResponseModel<object>> GenerateQrAsync(CreateOrderRequest createOrderRequest)
		{
			var errors = new Dictionary<string, string>();

			try
			{
				try
				{
					// Tạo order
					var order = new Order
					{
						UserId = createOrderRequest.UserId,
						ProductId = createOrderRequest.ProductId,
						CouponId = createOrderRequest.CouponId,
						TotalAmount = createOrderRequest.TotalAmount,
						Status = 0, // Pending
						CreateAt = DateTime.UtcNow,
						UpdateAt = DateTime.UtcNow,
						RemainGetCode = 3
					};

					await _orderRepository.AddOrderAsync(order);

					// Tạo payment
					var transactionCode = GenerateTransactionCode();
					var payment = new Payment
					{
						OrderId = order.OrderId,
						TransactionCode = transactionCode,
						PaymentMethod = 1, // QR Code
						CreateAt = DateTime.UtcNow,
						UserId = request.UserId,
						Amount = request.TotalAmount,
						Status = 0 // Pending
					};
					_paymentRepository.AddPaymentAsync(payment);


					var payload = new
					{
						accountNo = "0344665098",
						accountName = "TPBank",
						acqId = ,
						addInfo = payment.PaymentId,
						amount = payment.PaymentItems.Sum(x => x.Amount),
						template = _options.DefaultTemplate
					};

					var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.vietqr.io/v2/generate")
					{
						Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
					};

					httpRequest.Headers.Add("x-client-id", _options.ClientId);
					httpRequest.Headers.Add("x-api-key", _options.ApiKey);

					var response = await _httpClient.SendAsync(httpRequest);

					if (!response.IsSuccessStatusCode)
					{
						var errorContent = await response.Content.ReadAsStringAsync();
						return new ApiResponseModel<object>
						{
							Status = ApiResponseStatusConstant.FailedStatus,
							Message = $"Lấy Qr lỗi, xin kiểm tra lại thông tin ngân hàng của quản lý hoặc thử lại sau.",
							Errors = new Dictionary<string, string> { { "APIResponse", errorContent } }
						};
					}

					var content = await response.Content.ReadAsStringAsync();

					using var jsonDoc = JsonDocument.Parse(content);

					if (jsonDoc.RootElement.TryGetProperty("data", out var dataElement) &&
						dataElement.TryGetProperty("qrDataURL", out var qrElement))
					{
						var qrDataUrl = qrElement.GetString();

						return new ApiResponseModel<object>
						{
							Status = ApiResponseStatusConstant.SuccessStatus,
							Message = "Tạo QR thành công.",
							Data = new
							{
								Data = payload,
								QrCode = qrDataUrl
							}
						};
					}

					return new ApiResponseModel<object>
					{
						Data = payload,
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Không thể lấy mã tạo QR từ phản hồi VietQR.",

					};
				}
				catch (JsonException ex)
				{
					return new ApiResponseModel<object>
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Lỗi xử lý dữ liệu phản hồi từ VietQR.",
						Errors = new Dictionary<string, string> { { "JsonParse", ex.Message } }
					};
				}
				catch (HttpRequestException ex)
				{
					return new ApiResponseModel<object>
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Lỗi gửi yêu cầu tới VietQR API.",
						Errors = new Dictionary<string, string> { { "HttpRequest", ex.Message } }
					};
				}
				catch (Exception ex)
				{
					return new ApiResponseModel<object>
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Đã xảy ra lỗi trong quá trình tạo mã QR.",
						Errors = new Dictionary<string, string> { { "Exception", ex.Message } }
					};
				}
			}


	}
	} 
}
