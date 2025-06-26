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
		public async Task<ProductDetailResponseDto?> GetProductByIdAsync(int id)
		{
			return await _context.ProductOptions.Include(x => x.Product)
							 .ThenInclude(p => p.Category).Select(x => new ProductDetailResponseDto
							 {
								 ProductId = x.Product.ProductId,
								 ProductName = x.Product.ProductName ?? "N/A",
								 Price = x.Price,
								 CategoryName = x.Product.Category.CategoryName,
								 CategoryId = x.Product.Category.CategoryId,
								 Description = x.Product.Description,
								 Status = x.Product.Status,
								 ProductImage = x.Product.ProductImage,
							 }).Where(x => x.Status == 1)
							 .FirstOrDefaultAsync(x => x.ProductId == id);
		}

		public async Task<List<ProductDetailResponseDto>> GetAllAsync()
		{

			return await _context.ProductOptions.Include(x => x.Product)
								 .ThenInclude(p => p.Category).Select(x=>new ProductDetailResponseDto
								 {
									 ProductId=x.Product.ProductId,
									 ProductName=x.Product.ProductName ?? "N/A",
									 Price=x.Price,
									 CategoryName=x.Product.Category.CategoryName,
									 CategoryId=x.Product.Category.CategoryId,
									 Description=x.Product.Description,
									 Status=x.Product.Status,
									 ProductImage=x.Product.ProductImage,
								 }).Where(x=>x.Status==1)
								 .ToListAsync();
		}

		public async Task<bool> UpdateProductAsync(Product updated)
		{
			 _context.Products.Update(updated);	
			return await _context.SaveChangesAsync() > 0;
		}


	}
}
