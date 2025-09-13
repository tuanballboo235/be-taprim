using Microsoft.EntityFrameworkCore;
using System;
using TAPrim.Application.DTOs.ProductOption;
using TAPrim.Application.DTOs.Products;
using TAPrim.Models;
using TAPrim.Shared.Constants;

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
			return await _context.Products.Include(x => x.ProductOptions).FirstOrDefaultAsync(x => x.ProductId == id);
		}

		public async Task<Application.DTOs.Products.ProductDetailResponseDto?> GetProductByIdAsync(int id)
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


		public async Task<Application.DTOs.Products.ProductDetailResponseDto?> GetProductDtoByProductOptionIdAsync(int productOptionid)
		{
			return await _context.ProductOptions.Include(x => x.Product).Select(x => new ProductDetailResponseDto
			{
				ProductId = x.Product.ProductId,
				ProductName = x.Product.ProductName,
				CategoryName = x.Product.Category.CategoryName,
				CategoryId = x.Product.Category.CategoryId,
				Description = x.Product.Description,
				Status = x.Product.Status,
				ProductImage = x.Product.ProductImage
			}).FirstOrDefaultAsync(x => x.ProductId == productOptionid);
		}

		public async Task<ProductDetailResponseDto?> GetProductOptionByProductId(int productId)
		{
			return await _context.Products.Include(x => x.ProductOptions).Select(x => new ProductDetailResponseDto
			{
				ProductId = x.ProductId,
				ProductName = x.ProductName,
				ProductImage = x.ProductImage,
				CategoryName = x.Category.CategoryName,
				CategoryId = x.Category.CategoryId,
				Status = x.Status,
				ProductOptions = x.ProductOptions.OrderBy(opt => opt.Price) // Sắp xếp theo giá tăng dần
								.Select(x => new ProductOptionDto
								{
									ProductOptionId = x.ProductOptionId,
									DurationUnit = x.DurationUnit,
									DurationValue = x.DurationValue,
									Price = x.Price,
									Quantity = x.Quantity,
									Label = x.Label,
									DiscountPercent = x.DiscountPercent,
									ProductGuide = x.ProductGuide,
									StockAccount = x.ProductAccounts.Where(
										x => x.SellFrom < DateTime.Now &&
										x.SellTo > DateTime.Now &&
										x.Status == ProductAccountStatusConstant.Available && x.SellCount > 0).Count(), // lấy ra số lượng account 

									SellCount = x.ProductAccounts.Where(x => x.SellFrom < DateTime.Now &&
									x.SellTo > DateTime.Now &&
									x.Status == ProductAccountStatusConstant.Available && x.SellCount > 0).Sum(x => x.SellCount)
								}).ToList()
			}).FirstOrDefaultAsync(x => x.ProductId == productId);
		}

		public async Task<List<CategoryWithProductsDto>> GetListProductByCategoryId()
		{
			var categories = await _context.Categories
				.Include(c => c.Products)
				.ThenInclude(x => x.ProductOptions)
				.ThenInclude(x => x.ProductAccounts)// Navigation property
				.Select(c => new CategoryWithProductsDto
				{
					Title = c.CategoryName,
					Description = c.CategoryDescription,
					CategoryId = c.CategoryId,
					Products = c.Products.Select(p => new ProductDto
					{
						Id = p.ProductId,
						Name = p.ProductName,
						Image = p.ProductImage,
						MinPrice = p.ProductOptions.Min(x => x.Price),
						MaxPrice = p.ProductOptions.Max(x => x.Price),
						Status = p.Status,
						StockAccount = p.ProductOptions.Where(x => x.ProductId == p.ProductId)
						  .SelectMany(po => po.ProductAccounts).Where(p => p.Status != 0 && p.SellFrom < DateTime.Now && p.SellTo > DateTime.Now && p.SellCount > 0).Count(),
						CanSell = p.ProductOptions.Where(x => x.ProductId == p.ProductId)
						  .SelectMany(po => po.ProductAccounts).Where(p => p.Status != 0 && p.SellFrom < DateTime.Now && p.SellTo > DateTime.Now && p.SellCount > 0)
						  .Sum(pa => (int?)pa.SellCount) > 0
					}).Where(x => x.Status != 0 && x.StockAccount > 0).ToList()
				})
				.ToListAsync();

			return categories;
		}


	}
}
