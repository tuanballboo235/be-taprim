using System.Globalization;
using System.Net;
using System.Text;
using BasketballAcademyManagementSystemAPI.Common.Helpers;
using TAPrim.Models;
using TAPrim.Shared.Constants;

namespace TAPrim.Application.Services.ServiceImpl
{
	public class OrderNotificationService : IOrderNotificationService
	{
		private static readonly CultureInfo ViCulture = CultureInfo.GetCultureInfo("vi-VN");
		private readonly EmailHelper _emailHelper;

		public OrderNotificationService(EmailHelper emailHelper)
		{
			_emailHelper = emailHelper;
		}

		public async Task<bool> SendOrderUpdateNotificationAsync(Order order, string? customMessage)
		{
			var email = NormalizeEmail(order.ContactInfo);
			if (email == null)
			{
				return false;
			}

			var transactionCode = GetTransactionCode(order);
			var body = BuildMailBody(
				"Cập nhật đơn hàng TAPRIM",
				customMessage,
				$"TAPRIM đã cập nhật thông tin đơn hàng {transactionCode}. Vui lòng kiểm tra lại thông tin bên dưới.",
				new Dictionary<string, string?>
				{
					["Mã giao dịch"] = transactionCode,
					["Sản phẩm"] = order.ProductOption?.Product?.ProductName,
					["Gói"] = order.ProductOption?.Label,
					["Trạng thái đơn hàng"] = GetOrderStatusLabel(order.Status),
					["Tài khoản"] = GetAccountDisplayValue(order.ProductAccount),
					["Lượt lấy mã còn"] = order.RemainGetCode.ToString(ViCulture),
					["Hết hạn"] = FormatDateTime(order.ExpiredAt),
					["Tổng tiền"] = FormatMoney(order.TotalAmount)
				});

			return await SendSafeAsync(email, $"TAPRIM - Cập nhật đơn hàng {transactionCode}", body);
		}

		public async Task<bool> SendProductAccountUpdateNotificationAsync(Order order, ProductAccount productAccount, string? customMessage)
		{
			var email = NormalizeEmail(order.ContactInfo);
			if (email == null)
			{
				return false;
			}

			var transactionCode = GetTransactionCode(order);
			var body = BuildMailBody(
				"Cập nhật tài khoản sản phẩm TAPRIM",
				customMessage,
				$"TAPRIM đã cập nhật thông tin tài khoản sản phẩm cho đơn hàng {transactionCode}. Vui lòng kiểm tra lại thông tin bên dưới.",
				new Dictionary<string, string?>
				{
					["Mã giao dịch"] = transactionCode,
					["Sản phẩm"] = order.ProductOption?.Product?.ProductName,
					["Gói"] = order.ProductOption?.Label,
					["Tài khoản"] = GetAccountDisplayValue(productAccount),
					["Trạng thái tài khoản"] = GetProductAccountStatusLabel(productAccount.Status),
					["Lượt bán còn"] = (productAccount.SellCount ?? 0).ToString(ViCulture),
					["Khoảng bán"] = $"{FormatDate(productAccount.SellFrom)} - {FormatDate(productAccount.SellTo)}"
				});

			return await SendSafeAsync(email, $"TAPRIM - Cập nhật tài khoản {transactionCode}", body);
		}

		private async Task<bool> SendSafeAsync(string email, string subject, string body)
		{
			try
			{
				await _emailHelper.SendEmailAsync(email, subject, body);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private static string BuildMailBody(string title, string? customMessage, string fallbackMessage, Dictionary<string, string?> details)
		{
			var message = string.IsNullOrWhiteSpace(customMessage) ? fallbackMessage : customMessage.Trim();
			var rows = BuildDetailRows(details);

			return $@"
				<div style=""font-family:Arial,sans-serif;color:#0f172a;line-height:1.6;background:#f8fafc;padding:24px"">
					<div style=""max-width:640px;margin:0 auto;background:#ffffff;border:1px solid #e2e8f0;border-radius:12px;overflow:hidden"">
						<div style=""background:#15803d;color:#ffffff;padding:18px 22px"">
							<h2 style=""margin:0;font-size:20px"">{WebUtility.HtmlEncode(title)}</h2>
						</div>
						<div style=""padding:22px"">
							<p style=""margin:0 0 16px"">{ToHtml(message)}</p>
							<table style=""width:100%;border-collapse:collapse;font-size:14px"">
								<tbody>{rows}</tbody>
							</table>
							<p style=""margin:18px 0 0;color:#64748b;font-size:13px"">Email này được gửi tự động từ TAPRIM. Nếu thông tin chưa đúng, vui lòng liên hệ hỗ trợ.</p>
						</div>
					</div>
				</div>";
		}

		private static string BuildDetailRows(Dictionary<string, string?> details)
		{
			var builder = new StringBuilder();
			foreach (var item in details)
			{
				if (string.IsNullOrWhiteSpace(item.Value))
				{
					continue;
				}

				builder.Append($@"
					<tr>
						<td style=""border-top:1px solid #e2e8f0;padding:10px 12px;color:#64748b;width:38%"">{WebUtility.HtmlEncode(item.Key)}</td>
						<td style=""border-top:1px solid #e2e8f0;padding:10px 12px;font-weight:600;color:#0f172a"">{ToHtml(item.Value)}</td>
					</tr>");
			}

			return builder.ToString();
		}

		private static string ToHtml(string value)
		{
			return WebUtility.HtmlEncode(value)
				.Replace("\r\n", "\n")
				.Replace("\n", "<br />");
		}

		private static string? NormalizeEmail(string? contactInfo)
		{
			var email = contactInfo?.Trim();
			return !string.IsNullOrWhiteSpace(email) && email.Contains('@') ? email : null;
		}

		private static string GetTransactionCode(Order order)
		{
			return !string.IsNullOrWhiteSpace(order.Payment?.TransactionCode)
				? order.Payment.TransactionCode
				: $"ORDER-{order.OrderId}";
		}

		private static string? GetAccountDisplayValue(ProductAccount? account)
		{
			if (account == null)
			{
				return null;
			}

			if (!string.IsNullOrWhiteSpace(account.AccountData))
			{
				return account.AccountData.Trim();
			}

			var username = account.UsernameProductAccount?.Trim();
			var password = account.PasswordProductAccount?.Trim();
			return string.IsNullOrWhiteSpace(password) ? username : $"{username}:{password}";
		}

		private static string GetOrderStatusLabel(int? status)
		{
			if (status == OrderStatus.Active) return "Đã kích hoạt";
			if (status == OrderStatus.Deactive) return "Chưa kích hoạt";
			return "Không xác định";
		}

		private static string GetProductAccountStatusLabel(int status)
		{
			return status switch
			{
				0 => "Chưa sử dụng",
				1 => "Đang bán",
				2 => "Hết hạn",
				_ => "Không xác định"
			};
		}

		private static string FormatMoney(decimal? amount)
		{
			return amount.HasValue ? $"{amount.Value.ToString("N0", ViCulture)} đ" : "N/A";
		}

		private static string FormatDateTime(DateTime? value)
		{
			return value.HasValue ? value.Value.ToString("dd/MM/yyyy HH:mm", ViCulture) : "N/A";
		}

		private static string FormatDate(DateTime? value)
		{
			return value.HasValue ? value.Value.ToString("dd/MM/yyyy", ViCulture) : "N/A";
		}
	}
}