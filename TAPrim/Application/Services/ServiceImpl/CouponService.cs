using TAPrim.Application.DTOs.Common;
using TAPrim.Common.Helpers;
using TAPrim.Infrastructure.Repositories;
using TAPrim.Models;
using TAPrim.Shared.Constants;

namespace TAPrim.Application.Services.ServiceImpl
{
	public class CouponService : ICouponService
	{
		private readonly ICouponRepository _couponRepository;
		public CouponService(ICouponRepository couponRepository)
		{
			_couponRepository = couponRepository;
		}

		public async Task<ApiResponseModel<object>> GetCouponByCouponCode(string couponCode)
		{
			try
			{
				var coupon = await _couponRepository.FindByCode(couponCode);
				if (coupon == null) {
					return new ApiResponseModel<object>
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Mã giảm giá không khả dụng",
						Data = await _couponRepository.FindByCode(couponCode),
					};
				}
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.SuccessStatus,
					Message = "Mã giảm giá khả dụng",
					Data = await _couponRepository.FindByCode(couponCode),
				};
			}
			catch (Exception ex)
			{
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = ex.Message,
				};

			}
		}
	}
}