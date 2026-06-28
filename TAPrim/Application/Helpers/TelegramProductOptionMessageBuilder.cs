using System.Net;
using TAPrim.Application.DTOs.Telegram;

namespace TAPrim.Application.Helpers
{
	public static class TelegramProductOptionMessageBuilder
	{
		public static string BuildHtmlMessage(ProductOptionDetailDto detail)
		{
			var name = EscapeHtml(detail.ProductOptionName);
			var price = detail.Price?.ToString("N0") ?? "0";
			var guideBlock = BuildGuideBlock(detail.ProductGuide);
			var quantityPrompt = detail.CanPurchase
				? $"📝 Vui lòng nhập số lượng muốn mua (1-{detail.MaxQuantity}):"
				: "⚠️ Sản phẩm hiện đang hết hàng.";

			return $"""
				<b>⭕ {name}</b>
				💰 Giá: <b>{price}đ/tài khoản</b>
				📦 Tồn kho: <b>{detail.StockAccount} tài khoản - {detail.SellCount} lượt bán</b>
				📊 Đã bán: <b>{detail.SoldCount} tài khoản</b>

				{guideBlock}{quantityPrompt}
				""".Trim();
		}

		private static string BuildGuideBlock(string? guide)
		{
			if (string.IsNullOrWhiteSpace(guide))
			{
				return string.Empty;
			}

			var escapedGuide = EscapeHtml(guide.Trim());
			return $"<blockquote expandable>{escapedGuide}</blockquote>\n";
		}

		private static string EscapeHtml(string? text)
		{
			return string.IsNullOrEmpty(text)
				? string.Empty
				: WebUtility.HtmlEncode(text);
		}
	}
}
