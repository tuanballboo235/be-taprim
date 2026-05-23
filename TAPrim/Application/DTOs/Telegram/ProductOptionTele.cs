namespace TAPrim.Application.DTOs.Telegram
{
	public class ProductOptionTele
	{
		public int ProductOptionId { get; set; }
		public string? ProductOptionName { get; set; }

		public int ProductId { get; set; }

		public int? DurationValue { get; set; }

		public string? DurationUnit { get; set; }

		public int? Quantity { get; set; }

		public string? Label { get; set; }

		public decimal? Price { get; set; }

		public int? DiscountPercent { get; set; }

		public string? ProductGuide { get; set; }

		public string? ProductOptionImage { get; set; }
		public string ProductOptionIdentifier { get; set; } = null!;

		public int? SellPlatform { get; set; }


	}
}
