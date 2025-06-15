using TAPrim.Models;

namespace TAPrim.Infrastructure.Repositories
{
    public interface ICouponRepository
	{
		Task<Coupon?> FindById(int? couponId);

	}
}
