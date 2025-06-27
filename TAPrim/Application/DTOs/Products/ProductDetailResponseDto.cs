namespace TAPrim.Application.DTOs.Products
{
	public class ProductDetailResponseDto
	{
		public int ProductId { get; set; }
		public string? ProductName { get; set; } = null!;
		public decimal? Price { get; set; }
		public int? DiscountPercentDisplay { get; set; }
        public int? Status { get; set; }
        public string? AttentionNote { get; set; }
		public string? Description { get; set; }
		public string ProductImage { get; set; } = null!;
		public string? Label { get; set; }
		public string? ProductCode { get; set; }
		public int CategoryId { get; set; }
		public string? CategoryName { get; set; } = null!;
		public int? AccountStockQuantity { get; set; }
	}
}
