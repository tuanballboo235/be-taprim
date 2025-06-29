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

	}
}
