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

		public async Task AddProductOptionAsync(ProductOption productOption)
		{
			_context.ProductOptions.Add(productOption);
			await _context.SaveChangesAsync();
		}

		public async Task<bool> UpdateProductAsync(Product updated)
		{
			_context.Products.Update(updated);
			return await _context.SaveChangesAsync() > 0;
		}

		public async Task<bool> UpdateProductOptionAsync(ProductOption updated)
		{
			_context.ProductOptions.Update(updated);
			return await _context.SaveChangesAsync() > 0;
		}
		public async Task<Product?> GetProductById(int id)
		{
			return await _context.Products.Include(x=>x.ProductOptions).FirstOrDefaultAsync(x=>x.ProductId == id);
		}

		public async Task<ProductDetailResponseDto?> GetProductByIdAsync(int id)
		{
			return await _context.Products
				.Include(p => p.ProductOptions)
				.Include(p => p.Category)
				.Where(p => p.ProductId == id && p.Status == 1)
				.Select(p => new ProductDetailResponseDto
				{
					ProductId = p.ProductId,
					ProductName = p.ProductName,
					Price = p.ProductOptions.FirstOrDefault().Price, // nếu chỉ lấy option đầu tiên
					Label = p.ProductOptions.FirstOrDefault().Label,
					CategoryName = p.Category.CategoryName,
					CategoryId = p.Category.CategoryId,
					Description = p.Description,
					Status = p.Status,
					ProductImage = p.ProductImage
				})
				.FirstOrDefaultAsync();
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

		public Task<ProductDetailResponseDto?> GetProductDtoByIdAsync(int id)
		{
			throw new NotImplementedException();
		}
	}
}
