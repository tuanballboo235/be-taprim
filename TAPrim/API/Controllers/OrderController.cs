using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using TAPrim.Application.DTOs.Order;
using TAPrim.Application.Services;
using TAPrim.Common.Helpers;

namespace TAPrim.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class OrderController : ControllerBase
	{
		private readonly IOrderService _orderService;
		OrderController(IOrderService orderService) => _orderService = orderService;
		
		[HttpGet("get-order-by-product-account-id")]
		public async Task<IActionResult> GetOrderByProductAccountId([FromBody] OrderByProductAcountRequestDto request)
		{
			return ApiResponseHelper.HandleApiResponse(await _orderService.GetOrderByProductAccount(request.ProductAccountId));
		}

		[HttpPost("update-order)")]
		public async Task<IActionResult> UpdateOrder([FromBody] OrderByProductAcountRequestDto request)
		{
			int a = 0;
			return Ok();
		}
	}
}
