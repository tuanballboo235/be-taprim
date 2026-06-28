using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TAPrim.Application.DTOs.ProductAccounts;
using TAPrim.Application.Services;
using TAPrim.Common.Helpers;
using TAPrim.Shared.Constants;

namespace TAPrim.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ProductAccountController : ControllerBase
	{
		private readonly IProductAccountService _productAccountService;

		public ProductAccountController(IProductAccountService productAccountService)
		{
			_productAccountService = productAccountService;
		}

		[HttpPost("add-product-account/{productOptionId}")]
		[Authorize(Roles = AuthRoleConstants.AdminRoles)]
		public async Task<IActionResult> CreateProductAccount(int productOptionId, [FromBody] List<CreateProductAccountDto> request)
		{
			return ApiResponseHelper.HandleApiResponse(await _productAccountService.AddProductAccountAsync(productOptionId, request));
		}

		[HttpGet("get-product-account")]
		[Authorize(Roles = AuthRoleConstants.AdminRoles)]
		public async Task<IActionResult> GetProductAccounts([FromQuery] ProductAccountQueryDto filter)
		{
			return ApiResponseHelper.HandleApiResponse(await _productAccountService.GetProductAccountsAsync(filter));
		}

		[HttpPost("get-product-account-by-transaction-code")]
		public async Task<IActionResult> GetProductAccountByTransactionCode([FromBody] GetProductAccountByTransactionCodeRequestDto request)
		{
			return ApiResponseHelper.HandleApiResponse(await _productAccountService.GetProductAccountsByTransactionCodeAsync(request.TransactionCode));
		}

		[HttpPut("update-product-account/{productAccountId}")]
		[Authorize(Roles = AuthRoleConstants.AdminRoles)]
		public async Task<IActionResult> UpdateProductAccount(int productAccountId, [FromBody] UpdateProductProductAccountRequest request)
		{
			return ApiResponseHelper.HandleApiResponse(await _productAccountService.UpdateProductAccount(productAccountId, request));
		}
	}
}
