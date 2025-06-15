namespace TAPrim.Application.DTOs.Products
{
	public class CreateProductRequest
	{
		public string ProductName { get; set; } = null!;
		public int Status { get; set; }
		public double Price { get; set; }
		public int CategoryId { get; set; }
		public string? AttentionNote { get; set; }
		public int? DiscountPercentDisplay { get; set; }
		public string? Description { get; set; }
		public string? ProductCode { get; set; }
		public IFormFile ProductImage { get; set; } = null!;
		public int? DurationDay { get; set; }
	}
}
