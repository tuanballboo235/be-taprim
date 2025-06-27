namespace TAPrim.Application.DTOs.Category
{
	public class CategoryTreeDto
	{
		public int CategoryId { get; set; }
		public string CategoryName { get; set; } = string.Empty;
		public string? CategoryType { get; set; }
		public List<CategoryTreeDto> Children { get; set; } = new();
	}
}
