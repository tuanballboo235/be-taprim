using TAPrim.Models;

namespace TAPrim.Infrastructure.Repositories
{
	public interface ICouponRepository
	{
		Task<List<Coupon>> GetCouponsAsync();
		Task<Coupon?> FindById(int? couponId);
		Task<Coupon?> FindByCode(string couponCode);
		Task<Coupon?> FindAnyByCode(string couponCode);
		Task AddAsync(Coupon coupon);
		Task UpdateAsync(Coupon coupon);
	}
}
