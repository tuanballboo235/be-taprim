namespace TAPrim.Application.DTOs.Payment
{
	public class CreatePaymentRequest
	{
		public int? UserId { get; set; }
		public int ProductId { get; set; }
		public int? CouponId { get; set; }
		public decimal TotalAmount { get; set; }
		public string? EmailOrder { get; set; }
		public string? ClientNote { get; set; }
	}
}
