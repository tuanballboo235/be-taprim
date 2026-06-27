using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TAPrim.Application.DTOs.Coupon;
using TAPrim.Application.Services;
using TAPrim.Common.Helpers;
using TAPrim.Shared.Constants;

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

		[HttpGet]
		[Authorize(Roles = AuthRoleConstants.AdminRoles)]
		public async Task<IActionResult> GetCoupons([FromQuery] CouponQueryDto filter)
		{
			return ApiResponseHelper.HandleApiResponse(await _couponService.GetCouponsAsync(filter));
		}

		[HttpPost]
		[Authorize(Roles = AuthRoleConstants.AdminRoles)]
		public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponDto request)
		{
			return ApiResponseHelper.HandleApiResponse(await _couponService.CreateCouponAsync(request));
		}

		[HttpPut("{couponId:int}")]
		[Authorize(Roles = AuthRoleConstants.AdminRoles)]
		public async Task<IActionResult> UpdateCoupon(int couponId, [FromBody] UpdateCouponDto request)
		{
			return ApiResponseHelper.HandleApiResponse(await _couponService.UpdateCouponAsync(couponId, request));
		}

		[HttpDelete("{couponId:int}")]
		[Authorize(Roles = AuthRoleConstants.AdminRoles)]
		public async Task<IActionResult> DeleteCoupon(int couponId)
		{
			return ApiResponseHelper.HandleApiResponse(await _couponService.DeleteCouponAsync(couponId));
		}

		[HttpPost("get-coupon-info-by-coupon-code")]
		public async Task<IActionResult> GetCouponByCode([FromBody] CouponRequest request)
		{
			return ApiResponseHelper.HandleApiResponse(await _couponService.GetCouponByCouponCode(request.CouponCode));
		}

		[HttpPut("{couponCode}/decrease-turn")]
		[Authorize(Roles = AuthRoleConstants.AdminRoles)]
		public async Task<IActionResult> DecreaseCouponTurn(string couponCode)
		{
			var success = await _couponService.DecreaseTurnByCodeAsync(couponCode);
			if (!success)
			{
				return NotFound(new { message = "Mã giảm giá không tồn tại hoặc không hợp lệ." });
			}

			return Ok(new { message = "Đã cập nhật lượt sử dụng của mã giảm giá." });
		}
	}
}
