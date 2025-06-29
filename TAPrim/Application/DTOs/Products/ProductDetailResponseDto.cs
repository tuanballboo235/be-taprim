using TAPrim.Application.DTOs.ProductOption;

namespace TAPrim.Application.DTOs.Products
{
	public class ProductDetailResponseDto
	{

		public int ProductId { get; set; }
		public string? ProductName { get; set; }
		public string? ProductImage { get; set; }
		public int CategoryId { get; set; }
		public string? CategoryName { get; set; }
        public string? Description { get; set; }
		public int? Status {  get; set; }

		public List<ProductOptionDto>? ProductOptions { get; set; }
	}
}
