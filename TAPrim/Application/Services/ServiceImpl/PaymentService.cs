using Microsoft.Extensions.Options;
using System.Data;
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
		private const int PendingPaymentTimeoutMinutes = 2;

		private readonly HttpClient _httpClient;
		private readonly VietQrDto _vietQrConfig;
		private readonly TaprimContext _context;
		private readonly IPaymentRepository _paymentRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly TransactionCodeHelper _transactionCodeHelper;
		private readonly IProductAccountRepository _productAccountRepository;
		private readonly ICouponRepository _couponRepository;
		private readonly IProductRepository _productRepository;
		private readonly ISendMailService _sendMailService;

		public PaymentService(HttpClient httpClient,
			IOptions<VietQrDto> options,
			TaprimContext context,
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
			_context = context;
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
			string? transactionCode = null;
			try
			{
				await ReleaseExpiredPendingReservationsAsync();

				var quantity = createPaymentRequest.Quantity <= 0 ? 1 : createPaymentRequest.Quantity;
				var now = DateTime.Now;
				var transactionFee = Math.Max(0, createPaymentRequest.TransactionFee);
				var totalAmount = createPaymentRequest.TotalAmount;
				decimal originalAmount = 0;
				decimal discountAmount = 0;
				int? couponId = null;
				string? couponCode = null;
				int? couponDiscountPercent = null;

				await using (var dbTransaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable))
				{
					var productOption = await _context.ProductOptions
						.AsNoTracking()
						.FirstOrDefaultAsync(x => x.ProductOptionId == createPaymentRequest.ProductOptionId);

					if (productOption == null || productOption.Price == null)
					{
						return new ApiResponseModel<object>
						{
							Status = ApiResponseStatusConstant.FailedStatus,
							Message = "Không tìm thấy gói sản phẩm."
						};
					}

					originalAmount = Math.Round(productOption.Price.Value * quantity, 0, MidpointRounding.AwayFromZero);

					if (!string.IsNullOrWhiteSpace(createPaymentRequest.CouponCode))
					{
						var normalizedCouponCode = createPaymentRequest.CouponCode.Trim().ToUpperInvariant();
						var coupon = await _context.Coupons
							.FromSqlInterpolated($@"
								SELECT *
								FROM Coupon WITH (UPDLOCK, HOLDLOCK)
								WHERE couponCode = {normalizedCouponCode}")
							.FirstOrDefaultAsync();

						var couponValidationMessage = GetCouponInvalidMessage(coupon, now);
						if (!string.IsNullOrWhiteSpace(couponValidationMessage))
						{
							return new ApiResponseModel<object>
							{
								Status = ApiResponseStatusConstant.FailedStatus,
								Message = couponValidationMessage
							};
						}

						couponId = coupon!.CouponId;
						couponCode = coupon.CouponCode;
						couponDiscountPercent = coupon.DiscountPercent;
						discountAmount = Math.Round(
							originalAmount * ((coupon.DiscountPercent ?? 0) / 100m),
							0,
							MidpointRounding.AwayFromZero);
						coupon.RemainTurn -= 1;
					}

					totalAmount = Math.Max(0, originalAmount + transactionFee - discountAmount);

					var productAccounts = await _context.ProductAccounts
						.FromSqlInterpolated($@"
							SELECT *
							FROM ProductAccount WITH (UPDLOCK, HOLDLOCK)
							WHERE productOptionId = {createPaymentRequest.ProductOptionId}
								AND status = {ProductAccountStatusConstant.Available}
								AND sellCount > 0
								AND sellFrom < {now}
								AND sellTo > {now}")
						.OrderByDescending(x => x.DateChangePass)
						.ThenBy(x => x.ProductAccountId)
						.ToListAsync();

					var availableQuantity = productAccounts.Sum(x => x.SellCount ?? 0);
					if (availableQuantity < quantity)
					{
						return new ApiResponseModel<object>
						{
							Status = ApiResponseStatusConstant.FailedStatus,
							Message = availableQuantity <= 0
								? "Sản phẩm đang hết hàng, vui lòng liên hệ admin hoặc thử lại sau."
								: $"Chỉ còn {availableQuantity} tài khoản khả dụng, vui lòng giảm số lượng."
						};
					}

					var remainingQuantity = quantity;
					var reservationItems = new List<PaymentReservationItem>();

					foreach (var account in productAccounts)
					{
						if (remainingQuantity <= 0) break;

						var currentSellCount = account.SellCount ?? 0;
						var reservedQuantity = Math.Min(currentSellCount, remainingQuantity);
						if (reservedQuantity <= 0) continue;

						account.SellCount = currentSellCount - reservedQuantity;
						reservationItems.Add(new PaymentReservationItem
						{
							ProductAccountId = account.ProductAccountId,
							Quantity = reservedQuantity
						});

						remainingQuantity -= reservedQuantity;
					}

					transactionCode = await _transactionCodeHelper.GetCode();

					var payment = new Payment
					{
						TransactionCode = transactionCode,
						PaymentMethod = 1,
						CreateAt = now,
						UserId = createPaymentRequest.UserId,
						Amount = totalAmount,
						Status = PaymentConstatnt.Pending
					};

					_context.Payments.Add(payment);
					await _context.SaveChangesAsync();

					var reservationMetadata = new PaymentReservationMetadata
					{
						Quantity = quantity,
						ClientNote = createPaymentRequest.ClientNote,
						OriginalAmount = originalAmount,
						TransactionFee = transactionFee,
						DiscountAmount = discountAmount,
						FinalAmount = totalAmount,
						CouponId = couponId,
						CouponCode = couponCode,
						CouponDiscountPercent = couponDiscountPercent,
						Items = reservationItems
					};

					var order = new Order
					{
						ProductOptionId = createPaymentRequest.ProductOptionId,
						ProductAccountId = reservationItems.FirstOrDefault()?.ProductAccountId,
						PaymentId = payment.PaymentId,
						CreateAt = now,
						Status = OrderStatus.Deactive,
						CouponId = couponId ?? createPaymentRequest.CouponId,
						TotalAmount = totalAmount,
						ContactInfo = createPaymentRequest.EmailOrder,
						ClientNote = JsonSerializer.Serialize(reservationMetadata),
						ExpiredAt = now.AddMinutes(PendingPaymentTimeoutMinutes)
					};

					_context.Orders.Add(order);
					await _context.SaveChangesAsync();
					await dbTransaction.CommitAsync();
				}

				//khởi tạo object để có thể gene ra vietqr
				var payload = new
				{
					accountNo = _vietQrConfig.DefaultAccountNo,
					accountName = _vietQrConfig.DefaultAccountName,
					acqId = _vietQrConfig.DefaultAcqId,
					addInfo = transactionCode,
					amount = Math.Ceiling(totalAmount),
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
					await ClearOrderAndPaymentTempByTrancsactionCode(transactionCode);
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
							QrCode = qrDataUrl,
							OriginalAmount = originalAmount,
							TransactionFee = transactionFee,
							DiscountAmount = discountAmount,
							TotalAmount = totalAmount,
							CouponCode = couponCode,
							CouponDiscountPercent = couponDiscountPercent
						}
					};
				}


				if (!string.IsNullOrWhiteSpace(transactionCode))
				{
					await ClearOrderAndPaymentTempByTrancsactionCode(transactionCode);
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
				if (!string.IsNullOrWhiteSpace(transactionCode))
				{
					await ClearOrderAndPaymentTempByTrancsactionCode(transactionCode);
				}

				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = "Lỗi xử lý dữ liệu phản hồi từ VietQR.",
					Errors = new Dictionary<string, string> { { "JsonParse", ex.Message } }
				};
			}
			catch (HttpRequestException ex)
			{
				if (!string.IsNullOrWhiteSpace(transactionCode))
				{
					await ClearOrderAndPaymentTempByTrancsactionCode(transactionCode);
				}

				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = "Lỗi gửi yêu cầu tới VietQR API.",
					Errors = new Dictionary<string, string> { { "HttpRequest", ex.Message } }
				};
			}
			catch (Exception ex)
			{
				if (!string.IsNullOrWhiteSpace(transactionCode))
				{
					await ClearOrderAndPaymentTempByTrancsactionCode(transactionCode);
				}

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
				var transactionCode = data.Content.Replace("QR - ", "").Trim();
				await using var dbTransaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

				var payment = await _context.Payments
					.Include(x => x.Order)
					.FirstOrDefaultAsync(x => x.TransactionCode == transactionCode);

				var order = payment?.Order;
				if (payment == null || order == null)
				{
					return new ApiResponseModel<object>()
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Đơn hàng không tồn tại",
					};
				}

				if (payment.Status == PaymentConstatnt.Paid)
				{
					return new ApiResponseModel<object>()
					{
						Status = ApiResponseStatusConstant.SuccessStatus,
						Message = "Đơn hàng đã được xác nhận thanh toán",
					};
				}

				if (order.TotalAmount != data.TransferAmount)
				{
					await RestoreOrderReservationAsync(order);
					_context.Orders.Remove(order);
					_context.Payments.Remove(payment);
					await _context.SaveChangesAsync();
					await dbTransaction.CommitAsync();

					return new ApiResponseModel<object>()
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Bạn đã chuyển khoản sai giá trị đơn hàng, vui lòng liên hệ Zalo 0344665098 để được hỗ trợ.",
					};
				}

				payment.Status = PaymentConstatnt.Paid;
				payment.PaidDateAt = DateTime.Parse(data.TransactionDate);

				var dayAccount = 30;
				order.Status = OrderStatus.Active;
				order.RemainGetCode = 3;
				order.ExpiredAt = DateTime.Now.AddDays(dayAccount);

				await _context.SaveChangesAsync();
				await dbTransaction.CommitAsync();

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
					Message = "Xác nhận thanh toán thất bại",
				};

			}
		}
		public async Task<ApiResponseModel<object>> GetPaymentsAsync(PaymentFilterDto filter)
		{
			try
			{
				await ReleaseExpiredPendingReservationsAsync(filter.TransactionCode);

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

		public async Task<ApiResponseModel<object>> ClearOrderAndPaymentTempByTrancsactionCode(string transactionCode)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(transactionCode))
				{
					return new ApiResponseModel<object>()
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Mã giao dịch không hợp lệ"
					};
				}

				await using var dbTransaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
				var payment = await _context.Payments
					.Include(x => x.Order)
					.FirstOrDefaultAsync(x => x.TransactionCode == transactionCode.Trim());

				if (payment == null)
				{
					return new ApiResponseModel<object>()
					{
						Status = ApiResponseStatusConstant.SuccessStatus
					};
				}

				var order = payment.Order;
				if (payment.Status == PaymentConstatnt.Paid || order?.Status == OrderStatus.Active)
				{
					return new ApiResponseModel<object>()
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Đơn hàng đã thanh toán, không thể hủy đơn tạm."
					};
				}

				if (order != null)
				{
					await RestoreOrderReservationAsync(order);
					_context.Orders.Remove(order);
				}

				_context.Payments.Remove(payment);
				await _context.SaveChangesAsync();
				await dbTransaction.CommitAsync();

				return new ApiResponseModel<object>()
				{
					Status = ApiResponseStatusConstant.SuccessStatus
				};
			}
			catch (Exception ex) {
				return new ApiResponseModel<object>()
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = "Không thể hủy đơn tạm"
				};
			}
		}

		public async Task ReleaseExpiredPendingReservationsAsync(string? transactionCode = null)
		{
			await using var dbTransaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
			var now = DateTime.Now;
			var query = _context.Orders
				.Include(x => x.Payment)
				.Where(x =>
					x.Status == OrderStatus.Deactive &&
					x.Payment.Status == PaymentConstatnt.Pending &&
					x.ExpiredAt != null &&
					x.ExpiredAt <= now);

			if (!string.IsNullOrWhiteSpace(transactionCode))
			{
				var normalizedTransactionCode = transactionCode.Trim();
				query = query.Where(x => x.Payment.TransactionCode == normalizedTransactionCode);
			}

			var expiredOrders = await query.ToListAsync();
			foreach (var order in expiredOrders)
			{
				await RestoreOrderReservationAsync(order);
				_context.Orders.Remove(order);
				_context.Payments.Remove(order.Payment);
			}

			if (expiredOrders.Count > 0)
			{
				await _context.SaveChangesAsync();
			}

			await dbTransaction.CommitAsync();
		}

		private async Task RestoreOrderReservationAsync(Order order)
		{
			var metadata = TryReadReservationMetadata(order.ClientNote);
			var items = metadata?.Items
				.Where(x => x.ProductAccountId > 0 && x.Quantity > 0)
				.ToList() ?? new List<PaymentReservationItem>();

			if (items.Count == 0 && order.ProductAccountId.HasValue)
			{
				items.Add(new PaymentReservationItem
				{
					ProductAccountId = order.ProductAccountId.Value,
					Quantity = 1
				});
			}

			foreach (var item in items.GroupBy(x => x.ProductAccountId)
				.Select(x => new PaymentReservationItem
				{
					ProductAccountId = x.Key,
					Quantity = x.Sum(i => i.Quantity)
				}))
			{
				var account = await _context.ProductAccounts
					.FirstOrDefaultAsync(x => x.ProductAccountId == item.ProductAccountId);

				if (account == null) continue;

				account.SellCount = (account.SellCount ?? 0) + item.Quantity;
			}

			if (metadata?.CouponId > 0)
			{
				var coupon = await _context.Coupons
					.FirstOrDefaultAsync(x => x.CouponId == metadata.CouponId);

				if (coupon != null)
				{
					coupon.RemainTurn += 1;
				}
			}
		}

		private static PaymentReservationMetadata? TryReadReservationMetadata(string? clientNote)
		{
			if (string.IsNullOrWhiteSpace(clientNote))
			{
				return null;
			}

			try
			{
				return JsonSerializer.Deserialize<PaymentReservationMetadata>(clientNote);
			}
			catch (JsonException)
			{
				return null;
			}
		}

		private static string? GetCouponInvalidMessage(Coupon? coupon, DateTime now)
		{
			if (coupon == null)
			{
				return "Mã giảm giá không tồn tại.";
			}

			if (!coupon.IsActive.GetValueOrDefault())
			{
				return "Mã giảm giá không còn hiệu lực.";
			}

			if (coupon.ValidFrom.HasValue && coupon.ValidFrom.Value > now)
			{
				return "Mã giảm giá chưa có hiệu lực.";
			}

			if (coupon.ValidUntil.HasValue && coupon.ValidUntil.Value < now)
			{
				return "Mã giảm giá đã hết hạn.";
			}

			if (coupon.RemainTurn <= 0)
			{
				return "Mã giảm giá đã hết lượt sử dụng.";
			}

			if (!coupon.DiscountPercent.HasValue || coupon.DiscountPercent <= 0)
			{
				return "Mã giảm giá không còn hiệu lực.";
			}

			return null;
		}
	}
}
