using TAPrim.Application.DTOs.Common;
using TAPrim.Application.DTOs.ProductAccounts;
using TAPrim.Infrastructure.Repositories;
using TAPrim.Models;
using TAPrim.Shared.Constants;

namespace TAPrim.Application.Services.ServiceImpl
{
    public class ProductAccountService : IProductAccountService
	{
		private readonly IProductAccountRepository _productAccountRepository;

		public ProductAccountService(IProductAccountRepository productAccountRepository)
		{
			_productAccountRepository = productAccountRepository;
		}


		public async Task<ApiResponseModel<ProductAccountResponseDto>> AddProductAccountAsync(int productOptionId, List<CreateProductAccountDto> dto)
		{
			try
			{
				var product = await _productAccountRepository.GetProductByIdAsync(productOptionId);
				if (product == null)
					throw new Exception("Product not found");

				foreach (var item in dto)
				{
					// B1: Tạo entity thực
					var productAccount = new ProductAccount
					{

						ProductOptionId = productOptionId,
						AccountData = item.AccountData,
						UsernameProductAccount = item.UsernameProductAccount,
						PasswordProductAccount = item.PasswordProductAccount,
						DateChangePass = item.DateChangePass,
						SellCount = item.SellCount,
						SellFrom = item.SellDateFrom,
						SellTo = item.SellDateTo,
						Status = item.Status
					};

					// B2: Lưu vào DB
					await _productAccountRepository.AddProductAccountAsync(productAccount);
				}
				

				
				// B4: Trả response
				return new ApiResponseModel<ProductAccountResponseDto>
				{
					Status = ApiResponseStatusConstant.SuccessStatus,

				};
			}catch (Exception ex)
			{
				return new ApiResponseModel<ProductAccountResponseDto>
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = ex.Message
				};
			}	
		}

		public async Task<ApiResponseModel<PagedResponseDto<ProductAccountResponseDto>>> GetProductAccountsAsync(ProductAccountQueryDto query)
		{
			var result = await _productAccountRepository.GetFilteredProductAccountsAsync(query);

			var mapped = result.Items.Select(pa => new ProductAccountResponseDto
			{
				ProductAccountId = pa.ProductAccountId,
				ProductOptionId = pa.ProductOptionId,
				AccountData = pa.AccountData,
				UsernameProductAccount = pa.UsernameProductAccount,
				PasswordProductAccount = pa.PasswordProductAccount,
				Status = pa.Status,
				DateChangePass = pa.DateChangePass,
				SellCount = pa.SellCount
			}).ToList();

			return new ApiResponseModel<PagedResponseDto<ProductAccountResponseDto>>
			{
				Status = ApiResponseStatusConstant.SuccessStatus,

				Data = new PagedResponseDto<ProductAccountResponseDto>
				{

					Items = mapped,
					TotalRecords = result.TotalRecords,
					TotalPages = result.TotalPages,
					CurrentPage = result.CurrentPage,
					PageSize = result.PageSize
				}
			};
		}

		public async Task<ApiResponseModel<object>> GetProductAccountsByTransactionCodeAsync(string transactionCode)
		{
			try
			{
				return new ApiResponseModel<object>()
				{
					Status = ApiResponseStatusConstant.SuccessStatus,
					Data = await _productAccountRepository.GetProductAccountByPaymentTransactionCode(transactionCode),
					Message = "Lấy thông tin tài khoản thành công"
				};
			}
			catch (Exception ex)
			{
				return new ApiResponseModel<object>()
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = "Đã có lỗi xảy ra"
				};
			}
		}
		public async Task<ApiResponseModel<object>> UpdateProductAccount(int productAccountId, UpdateProductProductAccountRequest request)
		{
			try
			{
				var productAccount = await _productAccountRepository.GetProductAccountByIdAsync(productAccountId);
				if (productAccount == null)
				{
					return new ApiResponseModel<object>()
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Không tìm thấy tài khoản sản phẩm"
					};
				}
				productAccount.AccountData = request.AccountData ?? productAccount.AccountData;
				productAccount.Status = request.Status ?? productAccount.Status;
				productAccount.DateChangePass = request.DateChangePass ?? productAccount.DateChangePass;
				productAccount.UsernameProductAccount = request.UsernameProductAccount ?? productAccount.UsernameProductAccount;
				productAccount.PasswordProductAccount = request.PasswordProductAccount ?? productAccount.PasswordProductAccount ;
				productAccount.SellFrom = request.SellFrom ?? productAccount.SellFrom;
				productAccount.SellTo = request.SellTo ?? productAccount.SellTo;
				productAccount.SellCount = request.SellCount ?? productAccount.SellCount;

				await _productAccountRepository.UpdateProductAccount(productAccount);

				return new ApiResponseModel<object>()
				{
					Status = ApiResponseStatusConstant.SuccessStatus,
					Message = "Lấy thông tin tài khoản thành công",
					Data = new ProductAccountResponseDto
					{
						ProductAccountId = productAccount.ProductAccountId,
						ProductOptionId = productAccount.ProductOptionId,
						AccountData = productAccount.AccountData,
						UsernameProductAccount = productAccount.UsernameProductAccount,
						PasswordProductAccount = productAccount.PasswordProductAccount,
						Status = productAccount.Status,
						DateChangePass = productAccount.DateChangePass,
						SellCount = productAccount.SellCount,
					}
				};
			}
			catch (Exception ex) {
				return new ApiResponseModel<object>()
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = "Đã có lỗi xảy ra"
				};
			}
		}
	}
}
