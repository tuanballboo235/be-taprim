using TAPrim.Application.DTOs.Common;
using TAPrim.Application.DTOs.Coupon;
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

		public async Task<ApiResponseModel<object>> GetCouponsAsync(CouponQueryDto filter)
		{
			try
			{
				var keyword = filter.Keyword?.Trim().ToUpperInvariant();
				var coupons = await _couponRepository.GetCouponsAsync();

				if (!string.IsNullOrWhiteSpace(keyword))
				{
					coupons = coupons
						.Where(x => (x.CouponCode ?? string.Empty).Contains(keyword))
						.ToList();
				}

				if (filter.IsActive.HasValue)
				{
					coupons = coupons
						.Where(x => x.IsActive.GetValueOrDefault() == filter.IsActive.Value)
						.ToList();
				}

				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.SuccessStatus,
					Message = "Lấy danh sách mã giảm giá thành công",
					Data = coupons.Select(ToResponseDto).ToList()
				};
			}
			catch (Exception ex)
			{
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = "Không thể lấy danh sách mã giảm giá",
					Errors = new Dictionary<string, string> { { "Exception", ex.Message } }
				};
			}
		}

		public async Task<ApiResponseModel<object>> GetCouponByCouponCode(string couponCode)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(couponCode))
				{
					return Failed("Vui lòng nhập mã giảm giá.");
				}

				var coupon = await _couponRepository.FindAnyByCode(couponCode);
				if (coupon == null)
				{
					return Failed("Mã giảm giá không tồn tại.");
				}

				var validationMessage = GetCouponInvalidMessage(coupon);
				if (!string.IsNullOrWhiteSpace(validationMessage))
				{
					return Failed(validationMessage, ToResponseDto(coupon));
				}

				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.SuccessStatus,
					Message = "Mã giảm giá khả dụng",
					Data = ToResponseDto(coupon)
				};
			}
			catch (Exception ex)
			{
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = "Không thể kiểm tra mã giảm giá",
					Errors = new Dictionary<string, string> { { "Exception", ex.Message } }
				};
			}
		}

		public async Task<ApiResponseModel<object>> CreateCouponAsync(CreateCouponDto request)
		{
			try
			{
				var validationMessage = ValidateCouponRequest(
					request.CouponCode,
					request.DiscountPercent,
					request.ValidFrom,
					request.ValidUntil,
					request.RemainTurn);

				if (!string.IsNullOrWhiteSpace(validationMessage))
				{
					return Failed(validationMessage);
				}

				var normalizedCode = NormalizeCode(request.CouponCode);
				var existedCoupon = await _couponRepository.FindAnyByCode(normalizedCode);
				if (existedCoupon != null)
				{
					return Failed("Mã giảm giá đã tồn tại.");
				}

				var coupon = new Coupon
				{
					CouponCode = normalizedCode,
					DiscountPercent = request.DiscountPercent,
					ValidFrom = request.ValidFrom,
					ValidUntil = request.ValidUntil,
					IsActive = request.IsActive ?? true,
					RemainTurn = request.RemainTurn
				};

				await _couponRepository.AddAsync(coupon);

				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.SuccessStatus,
					Message = "Tạo mã giảm giá thành công",
					Data = ToResponseDto(coupon)
				};
			}
			catch (Exception ex)
			{
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = "Không thể tạo mã giảm giá",
					Errors = new Dictionary<string, string> { { "Exception", ex.Message } }
				};
			}
		}

		public async Task<ApiResponseModel<object>> UpdateCouponAsync(int couponId, UpdateCouponDto request)
		{
			try
			{
				var coupon = await _couponRepository.FindById(couponId);
				if (coupon == null)
				{
					return Failed("Không tìm thấy mã giảm giá.");
				}

				var nextCode = request.CouponCode == null
					? coupon.CouponCode
					: NormalizeCode(request.CouponCode);
				var nextDiscount = request.DiscountPercent ?? coupon.DiscountPercent;
				var nextRemainTurn = request.RemainTurn ?? coupon.RemainTurn;
				var nextValidFrom = request.ValidFrom ?? coupon.ValidFrom;
				var nextValidUntil = request.ValidUntil ?? coupon.ValidUntil;

				var validationMessage = ValidateCouponRequest(
					nextCode,
					nextDiscount,
					nextValidFrom,
					nextValidUntil,
					nextRemainTurn);

				if (!string.IsNullOrWhiteSpace(validationMessage))
				{
					return Failed(validationMessage);
				}

				if (!string.Equals(nextCode, coupon.CouponCode, StringComparison.OrdinalIgnoreCase))
				{
					var existedCoupon = await _couponRepository.FindAnyByCode(nextCode ?? string.Empty);
					if (existedCoupon != null && existedCoupon.CouponId != coupon.CouponId)
					{
						return Failed("Mã giảm giá đã tồn tại.");
					}
				}

				coupon.CouponCode = nextCode;
				coupon.DiscountPercent = nextDiscount;
				coupon.ValidFrom = nextValidFrom;
				coupon.ValidUntil = nextValidUntil;
				coupon.IsActive = request.IsActive ?? coupon.IsActive;
				coupon.RemainTurn = nextRemainTurn;

				await _couponRepository.UpdateAsync(coupon);

				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.SuccessStatus,
					Message = "Cập nhật mã giảm giá thành công",
					Data = ToResponseDto(coupon)
				};
			}
			catch (Exception ex)
			{
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = "Không thể cập nhật mã giảm giá",
					Errors = new Dictionary<string, string> { { "Exception", ex.Message } }
				};
			}
		}

		public async Task<ApiResponseModel<object>> DeleteCouponAsync(int couponId)
		{
			try
			{
				var coupon = await _couponRepository.FindById(couponId);
				if (coupon == null)
				{
					return Failed("Không tìm thấy mã giảm giá.");
				}

				coupon.IsActive = false;
				await _couponRepository.UpdateAsync(coupon);

				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.SuccessStatus,
					Message = "Đã vô hiệu hóa mã giảm giá",
					Data = ToResponseDto(coupon)
				};
			}
			catch (Exception ex)
			{
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = "Không thể xóa mã giảm giá",
					Errors = new Dictionary<string, string> { { "Exception", ex.Message } }
				};
			}
		}

		public async Task<bool> DecreaseTurnByCodeAsync(string couponCode)
		{
			var coupon = await _couponRepository.FindByCode(couponCode);
			if (coupon == null || !coupon.IsActive.GetValueOrDefault()) return false;

			if (coupon.RemainTurn > 0)
			{
				coupon.RemainTurn -= 1;
			}

			await _couponRepository.UpdateAsync(coupon);
			return true;
		}

		private static ApiResponseModel<object> Failed(string message, object? data = null)
		{
			return new ApiResponseModel<object>
			{
				Status = ApiResponseStatusConstant.FailedStatus,
				Message = message,
				Data = data
			};
		}

		private static string NormalizeCode(string? couponCode)
		{
			return (couponCode ?? string.Empty).Trim().ToUpperInvariant();
		}

		private static string? ValidateCouponRequest(
			string? couponCode,
			int? discountPercent,
			DateTime? validFrom,
			DateTime? validUntil,
			int remainTurn)
		{
			var normalizedCode = NormalizeCode(couponCode);
			if (string.IsNullOrWhiteSpace(normalizedCode))
			{
				return "Vui lòng nhập mã giảm giá.";
			}

			if (normalizedCode.Length > 10)
			{
				return "Mã giảm giá tối đa 10 ký tự.";
			}

			if (!discountPercent.HasValue || discountPercent <= 0 || discountPercent > 100)
			{
				return "Phần trăm giảm giá phải từ 1 đến 100.";
			}

			if (remainTurn < 0)
			{
				return "Lượt sử dụng còn lại không được âm.";
			}

			if (validFrom.HasValue && validUntil.HasValue && validFrom > validUntil)
			{
				return "Ngày bắt đầu không được lớn hơn ngày hết hạn.";
			}

			return null;
		}

		private static string? GetCouponInvalidMessage(Coupon coupon)
		{
			var now = DateTime.Now;

			if (!coupon.IsActive.GetValueOrDefault())
			{
				return "Mã giảm giá không còn hiệu lực.";
			}

			if (coupon.ValidFrom.HasValue && coupon.ValidFrom.Value > now)
			{
				return "Mã giảm giá chưa có hiệu lực.";
			}

			if (coupon.ValidUntil.HasValue && coupon.ValidUntil.Value < now)
			{
				return "Mã giảm giá đã hết hạn.";
			}

			if (coupon.RemainTurn <= 0)
			{
				return "Mã giảm giá đã hết lượt sử dụng.";
			}

			return null;
		}

		private static CouponResponseDto ToResponseDto(Coupon coupon)
		{
			return new CouponResponseDto
			{
				CouponId = coupon.CouponId,
				CouponCode = coupon.CouponCode,
				DiscountPercent = coupon.DiscountPercent,
				ValidFrom = coupon.ValidFrom,
				ValidUntil = coupon.ValidUntil,
				IsActive = coupon.IsActive,
				RemainTurn = coupon.RemainTurn,
				StatusLabel = GetCouponInvalidMessage(coupon) ?? "Đang hiệu lực"
			};
		}
	}
}
