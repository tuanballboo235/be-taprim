using TAPrim.Application.DTOs.ProductOption;
using TAPrim.Application.DTOs.Products;
using TAPrim.Models;

namespace TAPrim.Infrastructure.Repositories
{
    public interface IProductRepository
    {

		//Product Option
		Task<ProductOption?> GetProductOptionByIdAsync(int id);
		//Product
		Task AddProductAsync(Product product);
		Task AddProductOptionAsync(ProductOption productOption);
		Task<bool> UpdateProductAsync(Product updated);
		Task<bool> UpdateProductOptionAsync(ProductOption updated);
		Task<Application.DTOs.Products.ProductDetailResponseDto?> GetProductDtoByProductOptionIdAsync(int productOptionId);
		Task<Product?> GetProductById(int id);
		Task<ProductDetailResponseDto?> GetProductOptionByProductId(int productId);
		Task<List<CategoryWithProductsDto>> GetListProductByCategoryId();
	}
}
