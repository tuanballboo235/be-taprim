using Microsoft.EntityFrameworkCore;
using TAPrim.Models;

namespace TAPrim.Infrastructure.Repositories.RepositoryImpl
{
	public class CouponRepository : ICouponRepository
	{
		private readonly TaprimContext _context;

		public CouponRepository(TaprimContext context)
		{
			_context = context;
		}

		public async Task<List<Coupon>> GetCouponsAsync()
		{
			return await _context.Coupons
				.AsNoTracking()
				.OrderByDescending(x => x.CouponId)
				.ToListAsync();
		}

		public async Task<Coupon?> FindByCode(string couponCode)
		{
			var normalizedCode = couponCode.Trim().ToUpperInvariant();
			var now = DateTime.Now;

			return await _context.Coupons
				.Where(x =>
					x.CouponCode == normalizedCode &&
					x.IsActive == true &&
					x.RemainTurn > 0 &&
					(x.ValidFrom == null || x.ValidFrom <= now) &&
					(x.ValidUntil == null || x.ValidUntil >= now))
				.FirstOrDefaultAsync();
		}

		public async Task<Coupon?> FindAnyByCode(string couponCode)
		{
			var normalizedCode = couponCode.Trim().ToUpperInvariant();

			return await _context.Coupons
				.FirstOrDefaultAsync(x => x.CouponCode == normalizedCode);
		}

		public async Task<Coupon?> FindById(int? couponId)
		{
			return await _context.Coupons.FirstOrDefaultAsync(x => x.CouponId == couponId);
		}

		public async Task AddAsync(Coupon coupon)
		{
			var currentMaxId = await _context.Coupons
				.Select(x => (int?)x.CouponId)
				.MaxAsync() ?? 0;

			coupon.CouponId = currentMaxId + 1;
			_context.Coupons.Add(coupon);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateAsync(Coupon coupon)
		{
			_context.Coupons.Update(coupon);
			await _context.SaveChangesAsync();
		}
	}
}
