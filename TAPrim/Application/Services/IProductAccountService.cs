using TAPrim.Application.DTOs.Common;
using TAPrim.Application.DTOs.ProductAccounts;
using TAPrim.Models;

namespace TAPrim.Application.Services
{
    public interface IProductAccountService
    {
        Task<ApiResponseModel<ProductAccountResponseDto>> AddProductAccountAsync(int productOptionId, List<CreateProductAccountDto> dto);
        Task<ApiResponseModel<PagedResponseDto<ProductAccountResponseDto>>> GetProductAccountsAsync(ProductAccountQueryDto query);
        Task<ApiResponseModel<object>> GetProductAccountsByTransactionCodeAsync(string transactionCode);
        Task<ApiResponseModel<object>> UpdateProductAccount(int productAccountId, UpdateProductProductAccountRequest request);
	}
}
