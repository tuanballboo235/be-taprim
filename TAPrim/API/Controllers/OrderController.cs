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
		public OrderController(IOrderService orderService) => _orderService = orderService;
		
		[HttpPost("get-order-by-product-account-id")]
		public async Task<IActionResult> GetOrderByProductAccountId([FromBody] OrderByProductAcountRequestDto request)
		{
			return ApiResponseHelper.HandleApiResponse(await _orderService.GetOrderByProductAccount(request.ProductAccountId));
		}

		[HttpPut("update-order/{transactionCode}")]
		public async Task<IActionResult> UpdateOrder(string transactionCode, [FromBody] UpdateOrderRequestDto request)
		{
			return ApiResponseHelper.HandleApiResponse(await _orderService.UpdateOrderAsync(transactionCode, request));

		}
	}
}
