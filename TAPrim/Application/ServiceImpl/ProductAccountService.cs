using TAPrim.Application.DTOs;
using TAPrim.Application.DTOs.ProductAccounts;
using TAPrim.Infrastructure;
using TAPrim.Models;
using TAPrim.Shared.Constants;

namespace TAPrim.Application.ServiceImpl
{
	public class ProductAccountService: IProductAccountService
	{
		private readonly IProductAccountRepository _productAccountRepository;

		public ProductAccountService(IProductAccountRepository productAccountRepository)
		{
			_productAccountRepository = productAccountRepository;
		}

		public async Task<ApiResponseModel<ProductAccountResponseDto>> CreateProductAccountAsync(int productId, CreateProductAccountDto dto)
		{
			var product = await _productAccountRepository.GetProductByIdAsync(productId);
			if (product == null)
				throw new Exception("Product not found");

			// B1: Tạo entity thực
			var productAccount = new ProductAccount
			{
				ProductId = productId,
				AccountData = dto.AccountData,
				UsernameProductAccount = dto.UsernameProductAccount,
				PasswordProductAccount = dto.PasswordProductAccount,
				DateChangePass = dto.DateChangePass,
				SellCount = dto.SellCount,
				Status = dto.Status
			};

			// B2: Lưu vào DB
			await _productAccountRepository.AddProductAccountAsync(productAccount);

			// B3: Map sang DTO để trả về
			var responseDto = new ProductAccountResponseDto
			{
				ProductId = productAccount.ProductId,
				AccountData = productAccount.AccountData,
				UsernameProductAccount = productAccount.UsernameProductAccount,
				Status = productAccount.Status,
				DateChangePass = productAccount.DateChangePass,
				SellCount = productAccount.SellCount,
			};

			// B4: Trả response
			return new ApiResponseModel<ProductAccountResponseDto>
			{
				Status = ApiResponseStatusConstant.SuccessStatus,
				Data = responseDto
			};
		}

	}
}
