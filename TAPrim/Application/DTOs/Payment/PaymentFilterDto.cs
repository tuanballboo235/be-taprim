namespace TAPrim.Application.DTOs.Payment
{
	public class PaymentFilterDto
	{
		public string? TransactionCode { get; set; }
		public int? UserId { get; set; }
		public int? PaymentMethod { get; set; }
		public int? Status { get; set; }
		public decimal? Amount { get; set; }
		public DateTime? PaidAt { get; set; }
	}
}
