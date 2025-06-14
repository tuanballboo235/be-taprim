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
using TAPrim.Shared.Helpers;
using TAPrim.Application.DTOs.Products;

namespace TAPrim.Application.Services.ServiceImpl
{
	public class PaymentService : IPaymentService
	{
		private readonly HttpClient _httpClient;
		private readonly VietQrDto _vietQrConfig;
		private readonly IPaymentRepository _paymentRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly TransactionCodeHelper _transactionCodeHelper;
		private readonly IProductAccountRepository _productAccountRepository;


		public PaymentService(HttpClient httpClient,
			IOptions<VietQrDto> options,
			IPaymentRepository paymentRepository,
			IOrderRepository orderRepository,
			TransactionCodeHelper transactionCodeHelper,
			IProductAccountRepository productAccountRepository)
		{
			_httpClient = httpClient;
			_vietQrConfig = options.Value;
			_paymentRepository = paymentRepository;
			_orderRepository = orderRepository;
			_transactionCodeHelper = transactionCodeHelper;
			_productAccountRepository = productAccountRepository;
		}

		//hàm GenerateQrAsync
		public async Task<ApiResponseModel<object>> GenerateQrAsync(createPaymentRequest createPaymentRequest)
		{
			var errors = new Dictionary<string, string>();
			try
			{
				// Kiểm tra nếu product ko có product account thì báo lỗi 
				var productAccount = await _productAccountRepository.GetProductAccountByProductId(createPaymentRequest.ProductId);
				if (productAccount == null) {
					return new ApiResponseModel<object>
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = $"Sản phẩm đang hết hàng, vui lòng chờ admin cập nhật kho hàng, hoặc liên hệ qua zalo 0344665098 ",
					};
				}

				// Tạo payment
				var transactionCode = await _transactionCodeHelper.GetCode();
				var payment = new Payment
				{
					TransactionCode = transactionCode,
					PaymentMethod = 1, // QR Code
					CreateAt = DateTime.Now,
					UserId = createPaymentRequest.UserId,
					Amount = createPaymentRequest.TotalAmount,
					Status = 0 // Pending
				};
				await _paymentRepository.AddPaymentAsync(payment);

				//khởi tạo object để có thể gene ra vietqr
				var payload = new
				{
					accountNo = _vietQrConfig.DefaultAccountNo,
					accountName = _vietQrConfig.DefaultAccountName,
					acqId = _vietQrConfig.DefaultAcqId,
					addInfo = transactionCode,
					amount = createPaymentRequest.TotalAmount,
					template = _vietQrConfig.DefaultTemplate
				};

				var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.vietqr.io/v2/generate")
				{
					Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
				};

				httpRequest.Headers.Add("x-client-id", _vietQrConfig.ClientId);
				httpRequest.Headers.Add("x-api-key", _vietQrConfig.ApiKey);

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

		//hàm set product account dựa vào product Account sau khi người dùng thanh toán thành công 
		public async Task<ApiResponseModel<object>> SetProductAccountForPaymentByTransactionCode(SePayWebhookDto data, ProductResponseDto productInforResponse )
		{
			// replace chuỗi 
			var transactionCode = data.Content.Replace("QR - ", "");
			//Kiểm tra có chuyển khoản đúng số tiền và mã transaction Code ko ??
			var payment = await _paymentRepository.GetPaymentByTransactionCode(transactionCode);
			// Tạo order
			var order = new Order
			{
				UserId = productInforResponse.UserId,
				ProductId = productInforResponse.ProductId,
				CouponId = productInforResponse.CouponId,
				PaymentId = payment.PaymentId,
				TotalAmount = data.TransferAmount,
				Status = OrderStatus.Deactive, // Pending
				CreateAt = DateTime.UtcNow,
				UpdateAt = DateTime.UtcNow,
				RemainGetCode = 3
			};

			await _orderRepository.AddOrderAsync(order);


			// Nếu khách hàng chuyển sai tiền , thì ko cho thanh toán
			if (order.TotalAmount != data.TransferAmount)
			{
				return new ApiResponseModel<object>()
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = "Bạn đã chuyển khoan sai giá trị đơn hàng, vui lòng liên hệ zalo: 0344665098 để được hỗ trợ",
					Data = order
				};
			}


				var productAccount = await _productAccountRepository.GetProductAccountByProductId(order.ProductId);
			//kiểm tra còn tài khoản ko 
			if (productAccount == null)
			{
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = $"Sản phẩm đang hết hàng, vui lòng chờ admin cập nhật kho hàng, hoặc liên hệ qua zalo 0344665098 ",

				};
			}
			else
			{
				//Nếu lượt bán > 1 thì giảm lượt bán xuống
				if (productAccount.SellCount > 1)
				{
					productAccount.SellCount -= 1;
				}
				else //còn ko thì cập nhật trạng thái thành 1, là đã bán
				{
					productAccount.Status = ProductAccountStatusConstant.Unavailable;
				}

			}
			//sau khi thanh toán thành công thì set cho order tk 
			order.ProductAccountId = productAccount?.ProductAccountId;
			await _orderRepository.SaveChange();
			return new ApiResponseModel<object>()
			{
				Status = ApiResponseStatusConstant.SuccessStatus,
				Message = "Lấy tài khoản thành công",
			};
		}
	}
}
