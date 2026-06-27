using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TAPrim.Application.DTOs.Order;
using TAPrim.Application.DTOs.Payment;
using TAPrim.Application.Services;
using TAPrim.Common.Helpers;
using TAPrim.Shared.Constants;

namespace TAPrim.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class OrderController : ControllerBase
	{
		private readonly IOrderService _orderService;

		public OrderController(IOrderService orderService)
		{
			_orderService = orderService;
		}

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

		[HttpPost("get-order-detail-by-transaction-code")]
		public async Task<IActionResult> GetOrderDetailsByTransactionCode([FromBody] OrderLookupVerificationRequestDto request)
		{
			return ApiResponseHelper.HandleApiResponse(await _orderService.GetOrderDetailsByTransactionCode(request.TransactionCode, request.VerificationCode));
		}

		[HttpPost("request-order-lookup-code")]
		public async Task<IActionResult> RequestOrderLookupCode([FromBody] TransactionCodeRequestDto request)
		{
			return ApiResponseHelper.HandleApiResponse(await _orderService.SendOrderLookupVerificationCode(request.TransactionCode));
		}

		[HttpGet("admin/product-orders")]
		[Authorize(Roles = AuthRoleConstants.AdminRoles)]
		public async Task<IActionResult> GetAdminProductOrders([FromQuery] AdminProductOrderFilterDto filter)
		{
			return ApiResponseHelper.HandleApiResponse(await _orderService.GetAdminProductOrdersAsync(filter));
		}
	}
}
