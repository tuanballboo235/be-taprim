namespace TAPrim.Application.DTOs.Products
{
	public class CreateProductRequest
	{
		public string? ProductName{ get; set; }
		public int Status {  get; set; }
		public string? Description { get; set; }
        public IFormFile ProductImage { get; set; } = null!;
		public DateTime? CreatedAt { get; set; }
		public int CategoryId { get; set; }
		//public int DurationValue { get; set; }
		//public string? DurationUnit { get; set; }
		//public int? Quantity { get; set; }
		//public string? Label { get; set; }
		//public decimal? Price { get; set; }
		//public int? DiscountPercent { get; set; }
		//public string? ProductGuide { get; set; }


	}
}
