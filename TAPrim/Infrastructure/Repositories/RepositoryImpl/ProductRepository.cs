using Microsoft.EntityFrameworkCore;
using System;
using TAPrim.Application.DTOs.Products;
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

		public async Task<List<ProductDetailResponseDto>> GetAllAsync()
		{
			return await _context.Products
								 .Include(p => p.Category).Select(x=>new ProductDetailResponseDto
								 {
									 ProductId=x.ProductId,
									 ProductName=x.ProductName,
									 Price=x.Price,
									 CategoryName=x.Category.CategoryName,
									 CategoryId=x.Category.CategoryId,
									 ProductCode=x.ProductCode,
									 Description=x.Description,
									 ProductImage=x.ProductImage
								 })
								 .ToListAsync();
		}
	}
}
