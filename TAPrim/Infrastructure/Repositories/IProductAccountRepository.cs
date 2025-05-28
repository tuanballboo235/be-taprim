using TAPrim.Application.DTOs;
using TAPrim.Application.DTOs.ProductAccounts;
using TAPrim.Models;

namespace TAPrim.Infrastructure.Repositories
{
    public interface IProductAccountRepository
    {
        Task<Product?> GetProductByIdAsync(int productId);
        Task AddProductAccountAsync(ProductAccount account);
        Task<PagedResponseDto<ProductAccount>> GetFilteredProductAccountsAsync(ProductAccountQueryDto query);
    }
}
