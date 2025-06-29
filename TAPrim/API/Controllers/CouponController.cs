using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TAPrim.Application.DTOs.Coupon;
using TAPrim.Application.Services;

namespace TAPrim.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CouponController : ControllerBase
	{
		private readonly ICouponService _couponService;
		public CouponController(ICouponService couponService)
		{
			_couponService = couponService;
		}


		[HttpPost("get-coupon-info-by-coupon-code")]
		public async Task<ActionResult> GetCategoryTree([FromBody] CouponRequest request)
		{
			return Ok( await _couponService.GetCouponByCouponCode(request.CouponCode));
		}
		[HttpPut("{couponCode}/decrease-turn")]
		public async Task<IActionResult> DecreaseCouponTurn(string couponCode)
		{
			var success = await _couponService.DecreaseTurnByCodeAsync(couponCode);
			if (!success)
				return NotFound(new { message = "Mã giảm giá không tồn tại hoặc không hợp lệ." });

			return Ok(new { message = "Đã cập nhật lượt sử dụng của mã giảm giá." });
		}

	}
}
