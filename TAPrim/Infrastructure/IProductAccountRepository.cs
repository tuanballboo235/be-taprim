using TAPrim.Application.DTOs.ProductAccounts;
using TAPrim.Models;

namespace TAPrim.Infrastructure
{
	public interface IProductAccountRepository
	{
		Task<Product?> GetProductByIdAsync(int productId);
		Task AddProductAccountAsync(ProductAccount account);
	}
}
