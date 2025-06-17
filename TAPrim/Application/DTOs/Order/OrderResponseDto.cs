using TAPrim.Models;

namespace TAPrim.Application.DTOs.Order
{
	public class OrderResponseDto
	{
		public int OrderId { get; set; }

		public int? CouponId { get; set; }

		public int ProductId { get; set; }

		public int? ProductAccountId { get; set; }

		public int? Status { get; set; }

		public DateTime? CreateAt { get; set; }

		public DateTime? UpdateAt { get; set; }

		public int RemainGetCode { get; set; }

		public DateTime? ExpiredAt { get; set; }

		public string? ContactInfo { get; set; }

		public int PaymentId { get; set; }

		public decimal? TotalAmount { get; set; }

		public string? ClientNote { get; set; }

	}
}
