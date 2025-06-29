namespace TAPrim.Application.DTOs.ProductOption
{
	public class ProductOptionDto
	{
		public int ProductOptionId { get; set; }
		public int? DurationValue { get; set; }

		public string? DurationUnit { get; set; }

		public int? Quantity { get; set; }

		public string? Label { get; set; }

		public decimal? Price { get; set; }

		public int? DiscountPercent { get; set; }

		public string? ProductGuide { get; set; }
		public int? StockAccount {  get; set; }
	}
}
