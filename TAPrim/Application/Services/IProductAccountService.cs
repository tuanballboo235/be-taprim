using TAPrim.Application.DTOs;
using TAPrim.Application.DTOs.ProductAccounts;
using TAPrim.Models;

namespace TAPrim.Application.Services
{
    public interface IProductAccountService
    {
        Task<ApiResponseModel<ProductAccountResponseDto>> CreateProductAccountAsync(int productId, CreateProductAccountDto dto);
        Task<ApiResponseModel<PagedResponseDto<ProductAccountResponseDto>>> GetProductAccountsAsync(ProductAccountQueryDto query);
    }
}
