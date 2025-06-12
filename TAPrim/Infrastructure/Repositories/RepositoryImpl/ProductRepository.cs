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

		public async Task<Product> UpdateProductAsync(Product updated)
		{
			var existing = await _context.Products.FindAsync(updated.ProductId);
			if (existing == null)
				throw new Exception("Product not found");

			// Cập nhật thông tin cơ bản
			existing.ProductName = updated.ProductName;
			existing.DiscountPercentDisplay = updated.DiscountPercentDisplay;
			existing.Price = updated.Price;
			existing.Status = updated.Status;
			existing.CategoryId = updated.CategoryId;
			existing.AttentionNote = updated.AttentionNote;
			existing.Description = updated.Description;
			existing.ProductCode = updated.ProductCode;
			existing.ProductImage = updated.ProductImage;

			await _context.SaveChangesAsync();
			return existing;
		}


	}
}
