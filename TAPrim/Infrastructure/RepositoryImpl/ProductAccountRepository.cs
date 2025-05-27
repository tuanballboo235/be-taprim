using TAPrim.Application.DTOs.ProductAccounts;
using TAPrim.Models;

namespace TAPrim.Infrastructure.RepositoryImpl
{
	public class ProductAccountRepository:IProductAccountRepository
	{
		private readonly TaprimContext _context;

		public ProductAccountRepository(TaprimContext context)
		{
			_context = context;
		}

		public async Task<Product?> GetProductByIdAsync(int productId)
		{
			return await _context.Products.FindAsync(productId);
		}

		public async Task AddProductAccountAsync(ProductAccount account)
		{
			_context.ProductAccounts.Add(account);
			await _context.SaveChangesAsync();
		}
	}
}
