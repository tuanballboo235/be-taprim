using TAPrim.Application.DTOs.Products;
using TAPrim.Models;

namespace TAPrim.Infrastructure.Repositories
{
    public interface IProductRepository
    {
        Task AddProductAsync(Product product);
        Task<Product?> GetProductByIdAsync(int id);
		Task<List<ProductDetailResponseDto>> GetAllAsync();
		Task<Product> UpdateProductAsync(Product updated);

	}
}
