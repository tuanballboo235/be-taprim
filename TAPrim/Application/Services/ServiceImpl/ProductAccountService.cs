using TAPrim.Application.DTOs;
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


		public async Task<ApiResponseModel<ProductAccountResponseDto>> CreateProductAccountAsync(int productId, CreateProductAccountDto dto)
		{
			try
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
					SellFrom = dto.SellDateFrom,
					SellTo = dto.SellDateTo,
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
				ProductId = pa.ProductId,
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
						ProductId = productAccount.ProductId,
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
