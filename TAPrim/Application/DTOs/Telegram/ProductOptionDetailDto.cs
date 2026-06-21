namespace TAPrim.Application.DTOs.Telegram
{
	public class ProductOptionDetailDto
	{
		public int ProductOptionId { get; set; }

		public string ProductOptionIdentifier { get; set; } = null!;

		public string? ProductOptionName { get; set; }

		public decimal? Price { get; set; }

		public int StockAccount { get; set; }

		public int SoldCount { get; set; }

		public string? ProductGuide { get; set; }

		public string? ProductOptionImage { get; set; }

		public int MinQuantity { get; set; } = 1;

		public int MaxQuantity { get; set; }

		public bool CanPurchase { get; set; }

		public string QuantityPromptMessage { get; set; } = string.Empty;

		public string TelegramHtmlMessage { get; set; } = string.Empty;
	}
}
