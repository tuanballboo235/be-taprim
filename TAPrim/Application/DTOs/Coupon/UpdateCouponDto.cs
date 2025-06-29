namespace TAPrim.Application.DTOs.Coupon
{
	public class UpdateCouponDto
	{
		public string? CouponCode { get; set; }
		public int? DiscountPercent { get; set; }
		public DateTime? ValidFrom { get; set; }
		public DateTime? ValidUntil { get; set; }
		public bool? IsActive { get; set; }
		public int? RemainTurn { get; set; }
	}
}
