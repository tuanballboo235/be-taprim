namespace TAPrim.Application.DTOs.Order
{
	public class UpdateOrderRequestDto
	{
		public int? ProductAccountId  { get; set; }
		public int? Status { get; set; }
		public int? RemainCode { get; set; }
		public DateTime? ExpireDate { get; set; }
		public string? ContactInfo { get; set; } = null!;	
		public decimal? TotalAmount { get; set; }
	}
}
