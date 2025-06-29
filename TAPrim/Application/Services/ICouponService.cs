using TAPrim.Application.DTOs.Common;

namespace TAPrim.Application.Services
{
	public interface ICouponService
	{
		Task<ApiResponseModel<object>> GetCouponByCouponCode(string couponCode);
	}
}
