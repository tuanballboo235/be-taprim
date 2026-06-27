namespace TAPrim.Application.DTOs.Payment
{
	public class PaymentReservationMetadata
	{
		public int Quantity { get; set; } = 1;
		public string? ClientNote { get; set; }
		public decimal OriginalAmount { get; set; }
		public decimal TransactionFee { get; set; }
		public decimal DiscountAmount { get; set; }
		public decimal FinalAmount { get; set; }
		public int? CouponId { get; set; }
		public string? CouponCode { get; set; }
		public int? CouponDiscountPercent { get; set; }
		public List<PaymentReservationItem> Items { get; set; } = new();
	}

	public class PaymentReservationItem
	{
		public int ProductAccountId { get; set; }
		public int Quantity { get; set; }
	}
}
