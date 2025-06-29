using System.ComponentModel.DataAnnotations;

namespace TAPrim.Application.DTOs.Payment
{
	public class CreatePaymentRequest
	{
		public int? UserId { get; set; }
		public int ProductOptionId { get; set; }
		public int? CouponId { get; set; }

		[Range(typeof(decimal), "1000", "79228162514264337593543950335", ErrorMessage = "Giá tiền phải từ 1.000 trở lên")]
		public decimal TotalAmount { get; set; }
		public string? EmailOrder { get; set; }
		public string? ClientNote { get; set; }
	}
}
