using TAPrim.Models;

namespace TAPrim.Infrastructure.Repositories
{
    public interface IProductRepository
    {
        Task AddProductAsync(Product product);
        Task<Product?> GetProductByIdAsync(int id);

	}
}
