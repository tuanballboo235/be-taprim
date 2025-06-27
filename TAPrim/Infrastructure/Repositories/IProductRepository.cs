using TAPrim.Application.DTOs.ProductOption;
using TAPrim.Application.DTOs.Products;
using TAPrim.Models;

namespace TAPrim.Infrastructure.Repositories
{
    public interface IProductRepository
    {
        Task AddProductAsync(Product product);
		Task AddProductOptionAsync(ProductOption productOption);

		Task<bool> UpdateProductAsync(Product updated);
		Task<bool> UpdateProductOptionAsync(ProductOption updated);

		Task<ProductDetailResponseDto?> GetProductDtoByIdAsync(int id);
		Task<Product?> GetProductById(int id);
		Task<List<ProductDetailResponseDto>> GetAllAsync();
		Task<List<ProductOptionResponseDto>> GetProductOptionByProductId(int productId);

	}
}
