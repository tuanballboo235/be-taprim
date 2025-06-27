namespace TAPrim.Application.DTOs.ProductAccounts
{
	public class ProductAccountQueryDto
	{

		public int ProductOptionId { get; set; }

		public string? Username { get; set; }
		public int? Status { get; set; }

		public DateTime? FromDateChangePass { get; set; }
		public DateTime? ToDateChangePass { get; set; }

		public int? MinSellCount { get; set; }
		public int? MaxSellCount { get; set; }

		public int PageIndex { get; set; } = 1;
		public int PageSize { get; set; } = 10;
	}
}
