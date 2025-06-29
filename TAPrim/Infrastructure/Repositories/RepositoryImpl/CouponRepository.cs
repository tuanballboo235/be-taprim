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

        //Lấy ra coupon thỏa mãn 
        public async Task<Coupon?> FindByCode(string couponCode)
        {
            return await _context.Coupons.Where(x=>x.CouponCode == couponCode && x.IsActive ==true && x.RemainTurn>0).FirstOrDefaultAsync();
        }
   

        public async Task<Coupon?> FindById(int? couponId)
        {
            return await _context.Coupons.FirstOrDefaultAsync(x=>x.CouponId == couponId);
        }

		public async Task UpdateAsync(Coupon coupon)
		{
			_context.Coupons.Update(coupon);
			await _context.SaveChangesAsync();
		}
	}
}
