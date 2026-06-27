namespace TAPrim.Application.DTOs.Order
{
	public class AdminProductOrderFilterDto
	{
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
		public string? Keyword { get; set; }
		public int? OrderStatus { get; set; }
		public int? PaymentStatus { get; set; }
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 20;
	}
}
