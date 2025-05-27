using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TAPrim.Application.DTOs.Products;
using TAPrim.Application;
using TAPrim.Common.Helpers;

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


	}
}
