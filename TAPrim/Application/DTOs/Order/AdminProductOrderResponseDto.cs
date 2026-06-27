namespace TAPrim.Application.DTOs.Order
{
	public class AdminProductOrderResponseDto
	{
		public IEnumerable<AdminProductOrderItemDto> Items { get; set; } = new List<AdminProductOrderItemDto>();
		public AdminProductOrderSummaryDto Summary { get; set; } = new();
		public int TotalRecords { get; set; }
		public int TotalPages { get; set; }
		public int CurrentPage { get; set; }
		public int PageSize { get; set; }
	}
}
