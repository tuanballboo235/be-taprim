using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;
using TAPrim.Application.DTOs;
using TAPrim.Infrastructure.Repositories;
using TAPrim.Shared.Constants;

namespace TAPrim.Application.Services.ServiceImpl
{
	public class PaymentService:IPaymentService
	{
		private readonly HttpClient _httpClient;
		private readonly VietQrDto _options;
		private readonly IPaymentRepository _paymentRepository;

		public PaymentService(HttpClient httpClient, IOptions<VietQrDto> options, IPaymentRepository paymentRepository)
		{
			_httpClient = httpClient;
			_options = options.Value;
			_paymentRepository = paymentRepository;
		}

		//public async Task<ApiResponseModel<object>> GenerateQrAsync(string paymentId)
		//{
		//	var errors = new Dictionary<string, string>();

		//	try
		//	{
		//		// 1. Lấy thông tin payment
		//		var payment = await _teamFundRepository.GetPaymentByIdAsync(paymentId);
		//		if (payment == null)
		//		{
		//			return new ApiResponseModel<object>
		//			{
		//				Status = ApiResponseStatusConstant.FailedStatus,
		//				Message = "Không tìm thấy thông tin thanh toán.",
		//				Errors = new Dictionary<string, string> { { "PaymentId", $"Không tồn tại paymentId: {paymentId}" } }
		//			};
		//		}

		//		// Lấy thông tin manager từ payment
		//		var managerPaymentInfo = await _teamFundRepository.GetManagerPaymentByPaymentId(paymentId);
		//		if (managerPaymentInfo == null)
		//		{
		//			return new ApiResponseModel<object>
		//			{
		//				Status = ApiResponseStatusConstant.FailedStatus,
		//				Message = "Không có quản lí quỹ đội, xin vui lòng thêm quản lí cho đội bóng này.",
		//				Errors = new Dictionary<string, string> { { "notFoundFundManager", "Không có quản lí quỹ đội, xin vui lòng thêm quản lí cho đội bóng này." } }
		//			};
		//		}

		//		if (string.IsNullOrWhiteSpace(managerPaymentInfo.BankBinId) || string.IsNullOrWhiteSpace(managerPaymentInfo.BankAccountNumber))
		//		{
		//			return new ApiResponseModel<object>
		//			{
		//				Status = ApiResponseStatusConstant.FailedStatus,
		//				Message = "Không thể tạo mã thanh toán do thiếu thông tin ngân hàng của người quản lí.",
		//				Errors = new Dictionary<string, string> { { "BankInfo", "Thiếu BankBinId hoặc BankAccountNumber." } }
		//			};
		//		}

		//		var payload = new
		//		{
		//			accountNo = managerPaymentInfo.BankAccountNumber,
		//			accountName = managerPaymentInfo.BankName,
		//			acqId = managerPaymentInfo.BankBinId,
		//			addInfo = payment.PaymentId,
		//			amount = payment.PaymentItems.Sum(x => x.Amount),
		//			template = _options.DefaultTemplate
		//		};

		//		var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.vietqr.io/v2/generate")
		//		{
		//			Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
		//		};

		//		httpRequest.Headers.Add("x-client-id", _options.ClientId);
		//		httpRequest.Headers.Add("x-api-key", _options.ApiKey);

		//		var response = await _httpClient.SendAsync(httpRequest);

		//		if (!response.IsSuccessStatusCode)
		//		{
		//			var errorContent = await response.Content.ReadAsStringAsync();
		//			return new ApiResponseModel<object>
		//			{
		//				Status = ApiResponseStatusConstant.FailedStatus,
		//				Message = $"Lấy Qr lỗi, xin kiểm tra lại thông tin ngân hàng của quản lý hoặc thử lại sau.",
		//				Errors = new Dictionary<string, string> { { "APIResponse", errorContent } }
		//			};
		//		}

		//		var content = await response.Content.ReadAsStringAsync();

		//		using var jsonDoc = JsonDocument.Parse(content);

		//		if (jsonDoc.RootElement.TryGetProperty("data", out var dataElement) &&
		//			dataElement.TryGetProperty("qrDataURL", out var qrElement))
		//		{
		//			var qrDataUrl = qrElement.GetString();

		//			return new ApiResponseModel<object>
		//			{
		//				Status = ApiResponseStatusConstant.SuccessStatus,
		//				Message = "Tạo QR thành công.",
		//				Data = new
		//				{
		//					Data = payload,
		//					PaymentMethod = managerPaymentInfo.PaymentMethod.ToString(),
		//					QrCode = qrDataUrl
		//				}
		//			};
		//		}

		//		return new ApiResponseModel<object>
		//		{
		//			Data = payload,
		//			Status = ApiResponseStatusConstant.FailedStatus,
		//			Message = "Không thể lấy mã tạo QR từ phản hồi VietQR.",

		//		};
		//	}
		//	catch (JsonException ex)
		//	{
		//		return new ApiResponseModel<object>
		//		{
		//			Status = ApiResponseStatusConstant.FailedStatus,
		//			Message = "Lỗi xử lý dữ liệu phản hồi từ VietQR.",
		//			Errors = new Dictionary<string, string> { { "JsonParse", ex.Message } }
		//		};
		//	}
		//	catch (HttpRequestException ex)
		//	{
		//		return new ApiResponseModel<object>
		//		{
		//			Status = ApiResponseStatusConstant.FailedStatus,
		//			Message = "Lỗi gửi yêu cầu tới VietQR API.",
		//			Errors = new Dictionary<string, string> { { "HttpRequest", ex.Message } }
		//		};
		//	}
		//	catch (Exception ex)
		//	{
		//		return new ApiResponseModel<object>
		//		{
		//			Status = ApiResponseStatusConstant.FailedStatus,
		//			Message = "Đã xảy ra lỗi trong quá trình tạo mã QR.",
		//			Errors = new Dictionary<string, string> { { "Exception", ex.Message } }
		//		};
		//	}
		//}


	}
}
