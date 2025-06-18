namespace TAPrim.Application.DTOs.Products
{
	public class UpdateProductRequest
	{
		public string? ProductName { get; set; }
		public int? DiscountPercentDisplay { get; set; }
		public double? Price { get; set; }
		public int? Status { get; set; }
		public int? CategoryId { get; set; }
		public string? AttentionNote { get; set; }
		public string? Description { get; set; }
		public string? ProductCode { get; set; }
		public IFormFile? ProductImage { get; set; }
	}
}
