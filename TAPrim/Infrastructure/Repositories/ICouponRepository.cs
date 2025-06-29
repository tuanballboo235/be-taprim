using TAPrim.Models;

namespace TAPrim.Infrastructure.Repositories
{
    public interface ICouponRepository
	{
		Task<Coupon?> FindById(int? couponId);
		Task<Coupon?> FindByCode(string couponCode);
		Task UpdateAsync(Coupon coupon);
	}
}
