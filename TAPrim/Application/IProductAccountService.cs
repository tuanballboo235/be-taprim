using TAPrim.Application.DTOs;
using TAPrim.Application.DTOs.ProductAccounts;
using TAPrim.Models;

namespace TAPrim.Application
{
	public interface IProductAccountService
	{
		Task<ApiResponseModel<ProductAccountResponseDto>> CreateProductAccountAsync(int productId, CreateProductAccountDto dto);

	}
}
