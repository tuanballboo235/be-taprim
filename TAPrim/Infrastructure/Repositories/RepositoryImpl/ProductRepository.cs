using Microsoft.EntityFrameworkCore;
using System;
using TAPrim.Application.DTOs.ProductOption;
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

			return await _context.Products
								 .Include(p => p.Category).Select(x=>new ProductDetailResponseDto
								 {
									 ProductId=x.ProductId,
									 ProductName=x.ProductName ?? "N/A",
									 Status = x.Status,
									 CategoryId=x.CategoryId,
									 CategoryName=x.Category.CategoryName ?? "N/A",
									 Description = x.Description,
									 ProductImage=x.ProductImage

								 }).Where(x=>x.Status==1)
								 .ToListAsync();
		}

		public Task<ProductDetailResponseDto?> GetProductDtoByIdAsync(int id)
		{
			throw new NotImplementedException();
		}

		public async Task<List<ProductOptionResponseDto>> GetProductOptionByProductId(int productId)
		{
			return await _context.ProductOptions.Select(x=> new ProductOptionResponseDto
			{
				ProductOptionId=x.ProductOptionId,
				ProductId=x.ProductId,
				DurationUnit=x.DurationUnit,
				DurationValue=x.DurationValue,
				Price=x.Price,
				Quantity=x.Quantity,
				Label=x.Label,
				DiscountPercent=x.DiscountPercent,
				ProductGuide = x.ProductGuide
			}).Where(x=>x.ProductId==productId).ToListAsync();
		}

		public async Task<List<CategoryWithProductsDto>> GetListProductByCategoryId()
		{
			var categories = await _context.Categories
				.Include(c => c.Products)
				.ThenInclude(x=>x.ProductOptions)
				.ThenInclude(x=>x.ProductAccounts)// Navigation property
				.Select(c => new CategoryWithProductsDto
				{
					Title = c.CategoryName,
					Description = c.CategoryDescription,
					Products = c.Products.Select(p => new ProductDto
					{
						Id = p.ProductId,
						Name = p.ProductName,
						Image = p.ProductImage,
						MinPrice = p.ProductOptions.Min(x => x.Price),
						MaxPrice = p.ProductOptions.Max(x => x.Price),

						InStock = p.ProductOptions.Where(x=>x.ProductId == p.ProductId)
						  .SelectMany(po => po.ProductAccounts)
						  .Sum(pa => (int?)pa.SellCount) > 0
					}).ToList()
				})
				.ToListAsync();

			return categories;
		}


	}
}
