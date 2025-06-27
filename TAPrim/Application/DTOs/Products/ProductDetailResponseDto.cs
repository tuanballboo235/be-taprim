namespace TAPrim.Application.DTOs.Products
{
	public class ProductDetailResponseDto
	{
		public int ProductId { get; set; }
		public string? ProductName { get; set; } = null!;
		public int? Status { get; set; }
		public int CategoryId { get; set; }
		public string? CategoryName { get; set; } = null!;
		public string? Description { get; set; }
		public string ProductImage { get; set; } = null!;

		//public int? DurationValue { get; set; }
		//public string? DurationUnit { get; set; }
		//public int? Quantity { get; set; }
		//public string? Label { get; set; }
		//public decimal? Price { get; set; }
		//public int? DiscountPercent { get; set; }
		//public string? ProductGuide { get; set; }
	}
}
