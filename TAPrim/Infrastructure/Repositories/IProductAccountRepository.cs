using TAPrim.Application.DTOs.Common;
using TAPrim.Application.DTOs.ProductAccounts;
using TAPrim.Models;

namespace TAPrim.Infrastructure.Repositories
{
    public interface IProductAccountRepository
    {
        Task<ProductAccount?> GetProductAccountByIdAsync(int productAccountId);
		Task<Product?> GetProductByIdAsync(int productId);
        Task AddProductAccountAsync(ProductAccount account);
        Task<PagedResponseDto<ProductAccount>> GetFilteredProductAccountsAsync(ProductAccountQueryDto query);
        Task<int> GetQuantityStockProductAccountByProductId(int productId);
		Task<List<ProductAccount>> GetListProductAccountByProductId(int productId);
		Task<List<ProductAccount?>> GetProductAccountByProductId(int productId);
		Task<ProductAccountResponseDto?> GetProductAccountByPaymentTransactionCode(string transactionCode);
        Task<bool> UpdateProductAccount(ProductAccount productAccount);
	}
}
