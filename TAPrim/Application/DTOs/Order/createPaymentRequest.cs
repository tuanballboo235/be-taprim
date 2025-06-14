namespace TAPrim.Application.DTOs.Payment
{
	public class createPaymentRequest
	{
		public int? UserId { get; set; }
		public int ProductId { get; set; }
		public int? CouponId { get; set; }
		public decimal TotalAmount { get; set; }
	}
}
