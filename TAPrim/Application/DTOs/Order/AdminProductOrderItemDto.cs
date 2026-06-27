namespace TAPrim.Application.DTOs.Order
{
	public class AdminProductOrderItemDto
	{
		public int OrderId { get; set; }
		public int PaymentId { get; set; }
		public string? PaymentTransactionCode { get; set; }
		public int ProductId { get; set; }
		public string? ProductName { get; set; }
		public int ProductOptionId { get; set; }
		public string? ProductOptionLabel { get; set; }
		public int? ProductAccountId { get; set; }
		public string? ProductAccountData { get; set; }
		public int Quantity { get; set; }
		public string? ContactInfo { get; set; }
		public int? OrderStatus { get; set; }
		public int? PaymentStatus { get; set; }
		public int? PaymentMethod { get; set; }
		public DateTime? CreateAt { get; set; }
		public DateTime? PaidAt { get; set; }
		public DateTime? ExpiredAt { get; set; }
		public decimal? OriginalAmount { get; set; }
		public decimal? TransactionFee { get; set; }
		public decimal? DiscountAmount { get; set; }
		public decimal? TotalAmount { get; set; }
		public string? CouponCode { get; set; }
		public int? CouponDiscountPercent { get; set; }
	}
}
