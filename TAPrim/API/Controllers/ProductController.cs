using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TAPrim.Application.DTOs.Products;
using TAPrim.Common.Helpers;
using TAPrim.Application.Services;
using TAPrim.Application.DTOs;
using TAPrim.Models;

namespace TAPrim.API.Controllers
{
    [Route("api/product")]
	[ApiController]
	public class ProductController: ControllerBase
	{
		private readonly IProductService _productService;

		public ProductController(IProductService productService)
		{
			_productService = productService;
		}

		[HttpPost("create-product")]
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
		public async Task<IActionResult> UpdateProduct(int productId, [FromForm] UpdateProductRequest request)
		{
			return ApiResponseHelper.HandleApiResponse(await _productService.UpdateProductAsync(productId,request));
		}

		[HttpGet("list-products")]
		public async Task<IActionResult> GetProductList()
		{
			var products = await _productService.GetProductListAsync();
			return Ok(products);
		}

		[HttpGet("list-product-option-by-productId/{productId}")]
		public async Task<IActionResult> GetProductOptionByProductId(int productId)
		{
			var products = await _productService.GetProductOptionDataByProductId(productId);
			return Ok(products);
		}

		//public async Task<IActionResult> GetProductOptionByProductIdList()
		//{
		//	var products = await _productService.GetProductListAsync();
		//	return Ok(products);
		//}
	}
}
