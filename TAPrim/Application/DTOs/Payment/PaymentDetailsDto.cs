namespace TAPrim.Application.DTOs.Payment
{
	public class PaymentDetailsDto
	{
		public int PaymentId { get; set; }

		public string TransactionCode { get; set; } = null!;

		public int? PaymentMethod { get; set; }

		public DateTime? PaidDateAt { get; set; }

		public DateTime CreateAt { get; set; }

		public int? UserId { get; set; }

		public decimal Amount { get; set; }

		public string? Note { get; set; }

		public int? Status { get; set; }
	}
}
