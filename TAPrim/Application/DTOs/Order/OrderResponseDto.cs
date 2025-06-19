using TAPrim.Models;

namespace TAPrim.Application.DTOs.Order
{
	public class OrderResponseDto
	{
		public int OrderId { get; set; }

		public int? CouponId { get; set; }
		public string? CouponCode { get; set; }
		public int? CouponDiscountPersent { get; set; }
		public int ProductId { get; set; }
		public string? ProductName { get; set; }
		public int? ProductAccountId { get; set; }
        public string? ProductAccountData { get; set; }
        public int? Status { get; set; }
		public DateTime? CreateAt { get; set; }
		public int RemainGetCode { get; set; }
		public DateTime? ExpiredAt { get; set; }
		public string? ContactInfo { get; set; }
		public string? PaymentTransactionCode { get; set; }
		public DateTime? PaidAt {  get; set; }
		public decimal? TotalAmount { get; set; }
		public string? ClientNote { get; set; }
	}
}
