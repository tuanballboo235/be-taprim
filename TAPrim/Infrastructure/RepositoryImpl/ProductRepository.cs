using System;
using TAPrim.Models;

namespace TAPrim.Infrastructure.RepositoryImpl
{
	public class ProductRepository:IProductRepository
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
	}
}
