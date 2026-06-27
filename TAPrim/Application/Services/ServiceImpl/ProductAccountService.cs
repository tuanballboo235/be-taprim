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
				var productOption = await _productAccountRepository.GetProductOptionByIdAsync(productOptionId);
				if (productOption == null)
					throw new Exception("Product option not found");

				if (dto == null || dto.Count == 0)
					throw new Exception("Product account list is empty");

				foreach (var item in dto)
				{
					var credential = NormalizeCredential(item);
					if (credential == null)
						throw new Exception("Account data must use email:password or user:password format");

					// B1: Tạo entity thực
					var productAccount = new ProductAccount
					{

						ProductOptionId = productOptionId,
						AccountData = credential.Value.AccountData,
						UsernameProductAccount = credential.Value.Username,
						PasswordProductAccount = credential.Value.Password,
						DateChangePass = item.DateChangePass,
						SellCount = item.SellCount ?? 1,
						SellFrom = item.SellDateFrom ?? DateTime.Now,
						SellTo = item.SellDateTo ?? DateTime.Now.AddDays(1),
						Status = item.Status,
						CreateAt = DateTime.Now
					};

					// B2: Lưu vào DB
					await _productAccountRepository.AddProductAccountAsync(productAccount);
				}
				

				
				// B4: Trả response
				return new ApiResponseModel<ProductAccountResponseDto>
				{
					Status = ApiResponseStatusConstant.SuccessStatus,
					Message = "Thêm tài khoản thành công"
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

		private static (string AccountData, string Username, string Password)? NormalizeCredential(CreateProductAccountDto item)
		{
			var accountData = item.AccountData?.Trim();
			var username = item.UsernameProductAccount?.Trim();
			var password = item.PasswordProductAccount?.Trim();

			if (!string.IsNullOrWhiteSpace(accountData))
			{
				var separatorIndex = accountData.IndexOf(':');
				if (separatorIndex <= 0 || separatorIndex == accountData.Length - 1)
				{
					return null;
				}

				username = accountData[..separatorIndex].Trim();
				password = accountData[(separatorIndex + 1)..].Trim();
			}
			else if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
			{
				accountData = $"{username}:{password}";
			}

			if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
			{
				return null;
			}

			if (username.Contains(' ') || username.Contains(':'))
			{
				return null;
			}

			return ($"{username}:{password}", username, password);
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
				SellFrom = pa.SellFrom,
				SellTo = pa.SellTo,
				SellCount = pa.SellCount,
				CreateAt = pa.CreateAt,
				CanSell= pa.SellFrom < DateTime.Now && pa.SellTo > DateTime.Now && pa.SellCount > 0 && pa.Status == ProductAccountStatusConstant.Available
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
		public async Task<ApiResponseModel<object>> DeleteListProductAccount(List<int> productAccountId)
		{
			try
			{
				var ids = productAccountId?
					.Where(id => id > 0)
					.Distinct()
					.ToList() ?? new List<int>();

				if (ids.Count == 0)
				{
					return new ApiResponseModel<object>
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Vui lòng chọn account cần xóa"
					};
				}

				var accounts = await _productAccountRepository.GetProductAccountsByIdsAsync(ids);
				if (accounts.Count == 0)
				{
					return new ApiResponseModel<object>
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Không tìm thấy account cần xóa"
					};
				}

				foreach (var account in accounts)
				{
					account.Status = ProductAccountStatusConstant.Deleted;
					account.SellCount = 0;
				}

				foreach (var account in accounts)
				{
					await _productAccountRepository.UpdateProductAccount(account);
				}

				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.SuccessStatus,
					Message = accounts.Count > 1
						? $"Đã xóa {accounts.Count} account"
						: "Đã xóa account",
					Data = new
					{
						DeletedIds = accounts.Select(account => account.ProductAccountId).ToList()
					}
				};
			}
			catch (Exception)
			{
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = "Không thể xóa account"
				};
			}
		}

	}
}
