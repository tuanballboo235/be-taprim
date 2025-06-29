using TAPrim.Application.DTOs.Common;
using TAPrim.Application.DTOs.Coupon;

namespace TAPrim.Application.Services
{
	public interface ICouponService
	{
		Task<ApiResponseModel<object>> GetCouponByCouponCode(string couponCode);
		Task<bool> DecreaseTurnByCodeAsync(string couponCode);
	}
}
