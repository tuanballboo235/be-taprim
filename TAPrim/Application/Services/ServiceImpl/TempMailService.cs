using Microsoft.Extensions.Options;
using System.Net.Http;
using Newtonsoft.Json;
using TAPrim.Application.DTOs;
using TAPrim.Application.DTOs.Tempmail;
using TAPrim.Infrastructure.Repositories;
using TAPrim.Shared.Constants;
using TAPrim.Shared.Helpers;
using System.Transactions;
using TAPrim.Models;
using System.Net.Http.Headers;

namespace TAPrim.Application.Services.ServiceImpl
{
	public class TempMailService : ITempmailService
	{
		private readonly HttpClient _httpClient;
		private readonly VietQrDto _vietQrConfig;
		private readonly IPaymentRepository _paymentRepository;
		private readonly IProductRepository _productRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly ICategoryRepository _categoryRepository;
		private readonly TransactionCodeHelper _transactionCodeHelper;
		private readonly IProductAccountRepository _productAccountRepository;
		private readonly INetflixAccessLimiterMemoryService _inboundAccessLimiterRedisService;
		public TempMailService(HttpClient httpClient,
		IOptions<VietQrDto> options,
		IPaymentRepository paymentRepository,
		IOrderRepository orderRepository,
		TransactionCodeHelper transactionCodeHelper,
		IProductAccountRepository productAccountRepository,
		ICategoryRepository categoryRepository,
		IProductRepository productRepository,
		INetflixAccessLimiterMemoryService inboundAccessLimiterRedisService)
		{
			_httpClient = httpClient;
			_vietQrConfig = options.Value;
			_paymentRepository = paymentRepository;
			_orderRepository = orderRepository;
			_transactionCodeHelper = transactionCodeHelper;
			_productAccountRepository = productAccountRepository;
			_categoryRepository = categoryRepository;
			_productRepository = productRepository;
			_inboundAccessLimiterRedisService = inboundAccessLimiterRedisService;
		}

		// Lấy ra email netflix update house  
		public async Task<ApiResponseModel<List<TempmailEmailItemDto>>> EmailNetflixUpdateHouseFilter(string ipaddress)
		{
			var apiResponse = new ApiResponseModel<List<TempmailEmailItemDto>>();
			try
			{
				// URL chuẩn
				var url = $"https://tempmail.id.vn/api/email/310619";
				_httpClient.DefaultRequestHeaders.Authorization =
				new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "vQAtLsNbO4vmwVJO91iTiHeQfVFDznm0GGBxaUyq374d6c34");
				var response = await _httpClient.GetAsync(url);

				// Thêm header Authorization

				if (response.IsSuccessStatusCode)
				{
					var data = await response.Content.ReadAsStringAsync();

					try
					{

						var result = JsonConvert.DeserializeObject<TempmailApiResponseDto<TempmailDataDto>>(data);

						if (result != null && result.Success && result.Data != null && result.Data.Items != null)
						{
							var filteredEmails = result.Data.Items
								.Where(email =>
									email.Subject != null && (email.Subject.Contains(NetflixConstant.TemporaryNetflixCode) || email.Subject.Contains(NetflixConstant.UpdateFamilyNetflix)))
								.ToList();

							apiResponse.Status = ApiResponseStatusConstant.SuccessStatus;
							apiResponse.Data = filteredEmails ?? new List<TempmailEmailItemDto>();
							apiResponse.Message = filteredEmails.Count > 0 ? "Lấy danh sách email thành công" : $"Danh sách email trống {ipaddress}";

						}
						else
						{
							apiResponse.Status = ApiResponseStatusConstant.FailedStatus;
							apiResponse.Message = result?.Message ?? "No data found.";
						}


					}
					catch (System.Text.Json.JsonException ex)
					{
						apiResponse.Status = ApiResponseStatusConstant.FailedStatus;
						apiResponse.Message = $"JSON deserialization error: {ex.Message}";
					}
				}
				else
				{
					apiResponse.Status = ApiResponseStatusConstant.FailedStatus;
					apiResponse.Message = $"Error: {response.ReasonPhrase}";
				}
			}
			catch (HttpRequestException ex)
			{
				apiResponse.Status = ApiResponseStatusConstant.FailedStatus;
				apiResponse.Message = $"HttpRequestException: {ex.Message}";
			}
			catch (Exception ex)
			{
				apiResponse.Status = ApiResponseStatusConstant.FailedStatus;
				apiResponse.Message = $"Unexpected error: {ex.Message}";
			}

			return apiResponse;
		}

		public async Task<ApiResponseModel<List<TempmailEmailItemDto>>> GetNetflixCodeLoginEmailFilter(string transactionCode)
		{
			var apiResponse = new ApiResponseModel<List<TempmailEmailItemDto>>();

			// Bị giới hạn số lần truy cập
			if (!await _inboundAccessLimiterRedisService.IsAllowedAsync(transactionCode))
			{
				apiResponse.Status = "Failed";
				apiResponse.Message = "Bạn đã vượt quá số lượt truy cập cho phép.";
				return apiResponse;
			}

			// Validate order
			var order = await _orderRepository.FindByPaymentTransactionCodeAsync(transactionCode);
			if (!await ValidateOrder(order, apiResponse)) return apiResponse;

			// Kiểm tra quyền
			if (!await IsAllowGetNetflixMail(order, apiResponse)) return apiResponse;

			// Nếu đã cache mail → không gọi lại TempMail
			var cachedEmails = await _inboundAccessLimiterRedisService.GetCachedEmailsAsync(transactionCode);
			if (cachedEmails != null)
			{
				apiResponse.Status = "Success";
				apiResponse.Data = cachedEmails;
				apiResponse.Message = "Dữ liệu được lấy từ cache.";
				return apiResponse;
			}

			// Gọi TempMail
			try
			{
				var url = $"https://tempmail.id.vn/api/email/310619";
				_httpClient.DefaultRequestHeaders.Authorization =
					new AuthenticationHeaderValue("Bearer", "vQAtLsNbO4vmwVJO91iTiHeQfVFDznm0GGBxaUyq374d6c34");

				var response = await _httpClient.GetAsync(url);
				if (!response.IsSuccessStatusCode)
				{
					apiResponse.Status = "Failed";
					apiResponse.Message = $"Lỗi khi gọi TempMail: {response.ReasonPhrase}";
					return apiResponse;
				}

				var data = await response.Content.ReadAsStringAsync();
				var result = JsonConvert.DeserializeObject<TempmailApiResponseDto<TempmailDataDto>>(data);

				var filteredEmails = result?.Data?.Items?
					.Where(x => x.Subject?.Contains(NetflixConstant.NetflixCodeLogin) == true)
					.OrderByDescending(x => x.CreatedAt)
					.Take(2)
					.ToList();

				// Cache dữ liệu & tăng lượt gọi
				await _inboundAccessLimiterRedisService.CacheEmailsAsync(transactionCode, filteredEmails);
				await _inboundAccessLimiterRedisService.RegisterRequestAsync(transactionCode);

				apiResponse.Status = "Success";
				apiResponse.Data = filteredEmails ?? new List<TempmailEmailItemDto>();
				apiResponse.Message = filteredEmails?.Count > 0 ? "Lấy danh sách thành công" : "Không có mã nào.";
			}
			catch (Exception ex)
			{
				apiResponse.Status = "Failed";
				apiResponse.Message = $"Lỗi hệ thống: {ex.Message}";
			}

			return apiResponse;
		}



		//lấy mã xác minh chatgpt
		public async Task<ApiResponseModel<List<TempmailEmailItemDto>>> GetChatgptVerificationEmailFilter(string transactionCode)
		{
			var apiResponse = new ApiResponseModel<List<TempmailEmailItemDto>>();
			try
			{

				//lấy ra order theo payment transaction Code
				var order = await _orderRepository.FindByPaymentTransactionCodeAsync(transactionCode);
				if (!await ValidateOrder(order, apiResponse))
				{
					return apiResponse;
				}
				// kiểm tra xem có quyền  lấy code chatgpt k o
				if (!await IsAllowGetChatgptMail(order, apiResponse))
				{
					return apiResponse;
				}

				// URL chuẩn
				var url = $"https://tempmail.id.vn/api/email/310619";
				_httpClient.DefaultRequestHeaders.Authorization =
				new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "vQAtLsNbO4vmwVJO91iTiHeQfVFDznm0GGBxaUyq374d6c34");
				var response = await _httpClient.GetAsync(url);

				// Thêm header Authorization

				if (response.IsSuccessStatusCode)
				{
					var data = await response.Content.ReadAsStringAsync();

					try
					{

						var result = JsonConvert.DeserializeObject<TempmailApiResponseDto<TempmailDataDto>>(data);

						if (result != null && result.Success && result.Data != null && result.Data.Items != null)
						{
							var filteredEmails = result.Data.Items
								.Where(email =>
									email.Subject != null && email.Subject.Contains(ChatgptConstant.ChatgptAuthenCode))
								.ToList();

							apiResponse.Status = ApiResponseStatusConstant.SuccessStatus;
							apiResponse.Data = filteredEmails ?? new List<TempmailEmailItemDto>();
							apiResponse.Message = filteredEmails.Count > 0 ? "Lấy danh sách email thành công" : "Danh sách email trống";
						}
						else
						{
							apiResponse.Status = ApiResponseStatusConstant.FailedStatus;
							apiResponse.Message = result?.Message ?? "No data found.";
						}


					}
					catch (System.Text.Json.JsonException ex)
					{
						apiResponse.Status = ApiResponseStatusConstant.FailedStatus;
						apiResponse.Message = $"JSON deserialization error: {ex.Message}";
					}
				}
				else
				{
					apiResponse.Status = ApiResponseStatusConstant.FailedStatus;
					apiResponse.Message = $"Error: {response.ReasonPhrase}";
				}
			}
			catch (HttpRequestException ex)
			{
				apiResponse.Status = ApiResponseStatusConstant.FailedStatus;
				apiResponse.Message = $"HttpRequestException: {ex.Message}";
			}
			catch (Exception ex)
			{
				apiResponse.Status = ApiResponseStatusConstant.FailedStatus;
				apiResponse.Message = $"Unexpected error: {ex.Message}";
			}

			return apiResponse;
		}

		//Lấy Nội dung Email theo emailId
		public async Task<ApiResponseModel<TempMailMessage>> GetMailContentByEmailId(string emailId)
		{
			var apiResponse = new ApiResponseModel<TempMailMessage>();
			try
			{
				// URL chuẩn
				var url = $"https://tempmail.id.vn/api/message/{emailId}";
				_httpClient.DefaultRequestHeaders.Authorization =
				new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "vQAtLsNbO4vmwVJO91iTiHeQfVFDznm0GGBxaUyq374d6c34");
				var response = await _httpClient.GetAsync(url);

				// Thêm header Authorization

				if (response.IsSuccessStatusCode)
				{
					var data = await response.Content.ReadAsStringAsync();

					try
					{

						var result = JsonConvert.DeserializeObject<TempmailApiResponseDto<TempMailMessage>>(data);

						if (result != null)
						{
						
							apiResponse.Status = ApiResponseStatusConstant.SuccessStatus;
							apiResponse.Data = result.Data ?? new TempMailMessage() ;
							apiResponse.Message = result != null ? "Lấy danh sách email thành công" : "Danh sách email trống";
						}
						else
						{
							apiResponse.Status = ApiResponseStatusConstant.FailedStatus;
							apiResponse.Message = result != null ? "Lấy danh sách email thành công" : "Danh sách email trống";
						}


					}
					catch (System.Text.Json.JsonException ex)
					{
						apiResponse.Status = ApiResponseStatusConstant.FailedStatus;
						apiResponse.Message = $"JSON deserialization error: {ex.Message}";
					}
				}
				else
				{
					apiResponse.Status = ApiResponseStatusConstant.FailedStatus;
					apiResponse.Message = $"Error: {response.ReasonPhrase}";
				}
			}
			catch (HttpRequestException ex)
			{
				apiResponse.Status = ApiResponseStatusConstant.FailedStatus;
				apiResponse.Message = $"HttpRequestException: {ex.Message}";
			}
			catch (Exception ex)
			{
				apiResponse.Status = ApiResponseStatusConstant.FailedStatus;
				apiResponse.Message = $"Unexpected error: {ex.Message}";
			}

			return apiResponse;
		}

		//======================================================================
		private Task<bool> ValidateOrder(Order order, ApiResponseModel<List<TempmailEmailItemDto>> apiResponse)
		{
			if (order == null)
			{
				apiResponse.Status = ApiResponseStatusConstant.FailedStatus;
				apiResponse.Message = "Đơn hàng này không tồn tại hoặc đã bị vô hiệu hóa.";
				return Task.FromResult(false);
			}

			if (order.ExpiredAt <= DateTime.Now)
			{
				apiResponse.Status = ApiResponseStatusConstant.FailedStatus;
				apiResponse.Message = "Đơn hàng này đã hết hiệu lực.";
				return Task.FromResult(false);
			}

			return Task.FromResult(true);
		}


		//Hàm kiểm tra xem với payment transactionCode truyền vào có đc phép lấy email NETFLIX hay ko
		private async Task<bool> IsAllowGetNetflixMail(Order order, ApiResponseModel<List<TempmailEmailItemDto>> apiResponse)
		{
			var product = await _productRepository.GetProductByIdAsync(order.ProductId);
			var category = await _categoryRepository.GetCategoryWithParentAsync(product.CategoryId);
			if (category.CategoryName != CategoryConstant.NetflixCategory)
			{
				apiResponse.Status = ApiResponseStatusConstant.FailedStatus;
				apiResponse.Message = "Đơn hàng không có quyền lấy mã mail Netflix.";
				return false;
			}

			return true;
		}

		//Hàm kiểm tra xem với payment transactionCode truyền vào có đc phép lấy email CHATGPT hay ko
		private async Task<bool> IsAllowGetChatgptMail(Order order, ApiResponseModel<List<TempmailEmailItemDto>> apiResponse)
		{
			var product = await _productRepository.GetProductByIdAsync(order.ProductId);
			var category = await _categoryRepository.GetCategoryWithParentAsync(product.CategoryId);
			if (category.CategoryName != CategoryConstant.ChatgptCategory)
			{
				apiResponse.Status = ApiResponseStatusConstant.FailedStatus;
				apiResponse.Message = "Đơn hàng không có quyền lấy mã mail Chatgpt.";
				return false;
			}

			return true;
		}
	}
}
