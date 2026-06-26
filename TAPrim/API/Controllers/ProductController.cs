using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TAPrim.Application.DTOs.ProductOption;
using TAPrim.Application.DTOs.Products;
using TAPrim.Application.Services;
using TAPrim.Common.Helpers;
using TAPrim.Shared.Constants;

namespace TAPrim.API.Controllers
{
	[Route("api/product")]
	[ApiController]
	public class ProductController : ControllerBase
	{
		private readonly IProductService _productService;

		public ProductController(IProductService productService)
		{
			_productService = productService;
		}

		[HttpPost("create-product")]
		[Authorize(Roles = AuthRoleConstants.AdminRoles)]
		public async Task<IActionResult> CreateProduct([FromForm] CreateProductRequest request)
		{
			return ApiResponseHelper.HandleApiResponse(await _productService.CreateProductAsync(request));
		}

		[HttpPost("get-product-details/{productId}")]
		public async Task<IActionResult> GetProductDetails(int productId)
		{
			return ApiResponseHelper.HandleApiResponse(await _productService.GetProductDetailAsync(productId));
		}

		[HttpPut("update-product/{productId}")]
		[Authorize(Roles = AuthRoleConstants.AdminRoles)]
		public async Task<IActionResult> UpdateProduct(int productId, [FromForm] UpdateProductRequest request)
		{
			return ApiResponseHelper.HandleApiResponse(await _productService.UpdateProductAsync(productId, request));
		}

		[HttpPut("update-productoption-by-id/{id}")]
		[Authorize(Roles = AuthRoleConstants.AdminRoles)]
		public async Task<IActionResult> UpdateProductoptionById(int id, UpdateProductOptionRequest request)
		{
			var products = await _productService.UpdateProductOptionById(id, request);
			return Ok(products);
		}

		[HttpGet("list-product-option-by-productId/{productId}")]
		public async Task<IActionResult> GetProductOptionByProductId(int productId)
		{
			var products = await _productService.GetProductDetailByProductId(productId);
			return Ok(products);
		}

		[HttpGet("list-product-by-category")]
		public async Task<IActionResult> GetListProductByCategory([FromQuery] string keyword = null)
		{
			var products = await _productService.GetProductByCategory(keyword);
			return Ok(products);
		}
	}
}
