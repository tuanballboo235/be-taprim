namespace TAPrim.Application.DTOs.Products
{
	public class CategoryWithProductsDto
	{
		public string? Title { get; set; }
		public string? Description { get; set; }
        public int CategoryId { get; set; }
        public List<ProductDto> Products { get; set; }
	}
}
