using TAPrim.Application.DTOs.Common;
using TAPrim.Application.DTOs.Coupon;

namespace TAPrim.Application.Services
{
	public interface ICouponService
	{
		Task<ApiResponseModel<object>> GetCouponsAsync(CouponQueryDto filter);
		Task<ApiResponseModel<object>> GetCouponByCouponCode(string couponCode);
		Task<ApiResponseModel<object>> CreateCouponAsync(CreateCouponDto request);
		Task<ApiResponseModel<object>> UpdateCouponAsync(int couponId, UpdateCouponDto request);
		Task<ApiResponseModel<object>> DeleteCouponAsync(int couponId);
		Task<bool> DecreaseTurnByCodeAsync(string couponCode);
	}
}
