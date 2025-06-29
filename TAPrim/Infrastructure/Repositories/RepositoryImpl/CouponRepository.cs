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

        public async Task<Coupon?> FindByCode(string couponCode)
        {
            return await _context.Coupons.Where(x=>x.CouponCode == couponCode).FirstOrDefaultAsync();
        }
   

        public async Task<Coupon?> FindById(int? couponId)
        {
            return await _context.Coupons.FirstOrDefaultAsync(x=>x.CouponId == couponId);
        }
    }
}
