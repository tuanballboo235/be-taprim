using TAPrim.Application.DTOs.Products;
using TAPrim.Models;

namespace TAPrim.Infrastructure.Repositories
{
    public interface IProductRepository
    {
        Task AddProductAsync(Product product);
		Task<ProductDetailResponseDto?> GetProductByIdAsync(int id);
		Task<List<ProductDetailResponseDto>> GetAllAsync();
		Task<bool> UpdateProductAsync(Product updated);

	}
}
