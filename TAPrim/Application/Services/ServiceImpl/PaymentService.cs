using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;
using TAPrim.Infrastructure.Repositories;
using TAPrim.Shared.Constants;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using TAPrim.Models;
using TAPrim.Application.DTOs.Payment;
using TAPrim.Shared.Helpers;
using TAPrim.Application.DTOs.Products;
using System.Transactions;
using TAPrim.Application.DTOs.Common;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
		private readonly ICouponRepository _couponRepository;
		private readonly IProductRepository _productRepository;
		private readonly ISendMailService _sendMailService;

		public PaymentService(HttpClient httpClient,
			IOptions<VietQrDto> options,
			IPaymentRepository paymentRepository,
			IOrderRepository orderRepository,
			TransactionCodeHelper transactionCodeHelper,
			IProductAccountRepository productAccountRepository,
			ICouponRepository couponRepository,
			IProductRepository productRepository,
			ISendMailService sendMailService
			)
		{
			_httpClient = httpClient;
			_vietQrConfig = options.Value;
			_paymentRepository = paymentRepository;
			_orderRepository = orderRepository;
			_transactionCodeHelper = transactionCodeHelper;
			_productAccountRepository = productAccountRepository;
			_couponRepository = couponRepository;
			_productRepository = productRepository;
			_sendMailService = sendMailService;
		}
		public async Task<ApiResponseModel<object>> TestEmail()
		{
			//gửi email thông báo tới khách hàng
			await _sendMailService.SendMailByMailTemplateIdAsync(MailTemplateConstant.PaymentSucess, "tuanballboo6@gmail.com", new
			{
				TransactionCode = "1234",
				TransactionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
				Status = PaymentConstatnt.Paid == 1 ? "Đã thanh toán" : "Chưa thanh toán"
			});
			return new ApiResponseModel<object>()
			{
				Status = ApiResponseStatusConstant.SuccessStatus,
				Message = "Lấy tài khoản thành công",
			};
		}
		//hàm GenerateQrAsync
		public async Task<ApiResponseModel<object>> GenerateQrAsync(CreatePaymentRequest createPaymentRequest)
		{
			var errors = new Dictionary<string, string>();
			try
			{
				// Kiểm tra nếu product ko có product account thì báo lỗi 
				var productAccount = await _productAccountRepository.GetListProductAccountByProductId(createPaymentRequest.ProductId);
				if (productAccount == null)
				{
					return new ApiResponseModel<object>
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = $"Sản phẩm đang hết hàng, vui lòng chờ admin cập nhật kho hàng, hoặc liên hệ qua zalo 0344665098 ",
					};
				}

				// Tính toán amount + coupon

				dynamic couponValue = null;
				decimal totalAmount = createPaymentRequest.TotalAmount;
				// Xử lí giá đơn hàng dựa vào coupon
				if (createPaymentRequest.CouponId != null)
				{
					//lấy ra giá trị 
					couponValue = (await _couponRepository.FindById(createPaymentRequest.CouponId))?.DiscountPercent;

					if (couponValue == null) totalAmount = createPaymentRequest.TotalAmount; //nếu ko có couponId
					else totalAmount = (createPaymentRequest.TotalAmount * couponValue) / 100; // nếu có couponid
				}


				// Tạo payment
				var transactionCode = await _transactionCodeHelper.GetCode();
				var payment = new Payment
				{
					TransactionCode = transactionCode,
					PaymentMethod = 1, // QR Code
					CreateAt = DateTime.Now,
					UserId = createPaymentRequest.UserId,
					Amount = totalAmount,
					Status = 0 // Pending
				};
				await _paymentRepository.AddPaymentAsync(payment);

				// Tạo order tạm
				var order = new Order
				{
					ProductId = createPaymentRequest.ProductId,
					PaymentId = payment.PaymentId,
					CreateAt = DateTime.Now,
					Status = OrderStatus.Deactive,//Not Active
					CouponId = createPaymentRequest.CouponId,
					TotalAmount = totalAmount,
					ContactInfo = createPaymentRequest.EmailOrder,
					ClientNote = createPaymentRequest.ClientNote,
				};
				await _orderRepository.AddOrderAsync(order);
				//khởi tạo object để có thể gene ra vietqr
				var payload = new
				{
					accountNo = _vietQrConfig.DefaultAccountNo,
					accountName = _vietQrConfig.DefaultAccountName,
					acqId = _vietQrConfig.DefaultAcqId,
					addInfo = transactionCode,
					amount = totalAmount,
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
							TransactionCode = transactionCode,
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
		public async Task<ApiResponseModel<object>> SetProductAccountForPaymentByTransactionCode(SePayWebhookDto data)
		{
			try
			{
				// replace chuỗi 
				var transactionCode = data.Content.Replace("QR - ", "");

				var payment = await _paymentRepository.GetPaymentByTransactionCode(transactionCode);


				// cập nhật order
				var order = await _orderRepository.FindByPaymentTransactionCodeAsync(transactionCode);

				if (order == null)
				{
					return new ApiResponseModel<object>()
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Đơn hàng không tồn tại",
					};
				}
				// Nếu khách hàng chuyển sai tiền , thì ko cho thanh toán
				if (order.TotalAmount != data.TransferAmount)
				{
					return new ApiResponseModel<object>()
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Bạn đã chuyển khoản sai giá trị đơn hàng, vui lòng liên hệ zalo: 0344665098 để được hỗ trợ",
					};
				}

				//Cập nhật lại trạng thái payment 
				payment.Status = PaymentConstatnt.Paid;
				payment.PaidDateAt = DateTime.Parse(data.TransactionDate);
				payment.Status = PaymentConstatnt.Paid;

				await _paymentRepository.SaveChange();

				//Lấy ra danh sách account
				var productAccountList = await _productAccountRepository.GetListProductAccountByProductId(order.ProductId);
				//kiểm tra còn tài khoản ko 
				if (productAccountList.Count() <= 0) {
					return new ApiResponseModel<object>
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = $"Sản phẩm đang hết hàng, vui lòng chờ admin cập nhật kho hàng, hoặc liên hệ qua zalo 0344665098",

					};

				}
				else
				{
					foreach (var account in productAccountList)
					{
						if ( // ko thỏa mãn productAccount
							(DateTime.Now > account.SellFrom && DateTime.Now < account.SellTo) &&
							account.Status == ProductAccountStatusConstant.Available && account.SellCount > 0)
						{

							//Nếu lượt bán > 1 thì giảm lượt bán xuống
							if (account.SellCount > 0)
							{
								account.SellCount -= 1;
							}
							else //còn ko thì cập nhật trạng thái thành 0, là đã bán
							{
								account.Status = ProductAccountStatusConstant.Unavailable;
							}
							//sau khi thanh toán thành công thì set cho order tk 
							order.ProductAccountId = account?.ProductAccountId;
							
						}
						else
						{
							return new ApiResponseModel<object>
							{
								Status = ApiResponseStatusConstant.FailedStatus,
								Message = $"Sản phẩm đang hết hàng, vui lòng chờ admin cập nhật kho hàng, hoặc liên hệ qua zalo 0344665098",

							};
						}
					}

				}

				//lấy ra hạn product
				var dayAccount = (await _productRepository.GetProductByIdAsync(order.ProductId))?.DurationDay;
				
				order.Status = OrderStatus.Active;
				order.RemainGetCode = 3;
				order.ExpiredAt = DateTime.Now.AddDays(dayAccount ?? 0);

				await _orderRepository.SaveChange();

				//gửi email thông báo tới khách hàng
				await _sendMailService.SendMailByMailTemplateIdAsync(MailTemplateConstant.PaymentSucess,order.ContactInfo, new
				{
					TransactionCode = transactionCode,
					TransactionDate = DateTime.Parse(data.TransactionDate).ToString("yyyy-MM-dd HH:mm"),
					Status = PaymentConstatnt.Paid == 1 ? "Đã thanh toán" : "Chưa thanh toán"
				});
				return new ApiResponseModel<object>()
				{
					Status = ApiResponseStatusConstant.SuccessStatus,
					Message = "Lấy tài khoản thành công",
				};
			}
			catch (Exception ex)
			{
				return new ApiResponseModel<object>()
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = "Lấy tài khoản thành công",
				};

			}
		}
		public async Task<ApiResponseModel<object>> GetPaymentsAsync(PaymentFilterDto filter)
		{
			try
			{
				return new ApiResponseModel<object>()
				{
					Status = ApiResponseStatusConstant.SuccessStatus,
					Message = "Lấy danh sách thành công",
					Data = await _paymentRepository.GetPaymentsAsync(filter)
				};

			}
			catch (Exception ex)
			{
				return new ApiResponseModel<object>()
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = "Lấy danh sách thất bại",
				};
			}
		}
	}
}
