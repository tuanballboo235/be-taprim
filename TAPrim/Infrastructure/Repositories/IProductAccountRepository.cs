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
        Task<int> GetQuantityStockProductAccountByProductOptionId(int productId);
		Task<List<ProductAccount>> GetListProductAccountByProductOptionId(int productOptionId);
		Task<int> GetTotalSellCountByProductOptionIdAsync(int productOptionId);
		Task<ProductAccountResponseDto?> GetProductAccountByPaymentTransactionCode(string transactionCode);
        Task<bool> UpdateProductAccount(ProductAccount productAccount);
	}
}
