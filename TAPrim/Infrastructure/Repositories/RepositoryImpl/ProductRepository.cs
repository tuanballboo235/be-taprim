using Microsoft.EntityFrameworkCore;
using System;
using TAPrim.Models;

namespace TAPrim.Infrastructure.Repositories.RepositoryImpl
{
    public class ProductRepository : IProductRepository
    {
        private readonly TaprimContext _context;

        public ProductRepository(TaprimContext context)
        {
            _context = context;
        }

        public async Task AddProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }
		public async Task<Product?> GetProductByIdAsync(int id)
		{
			return await _context.Products
				.Include(p => p.Category)
				.FirstOrDefaultAsync(p => p.ProductId == id);
		}
	}
}
