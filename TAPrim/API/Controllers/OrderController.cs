using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TAPrim.Application.Services;

namespace TAPrim.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class OrderController : ControllerBase
	{
		private readonly IOrderService _orderService;
		OrderController(IOrderService orderService) => _orderService = orderService;
		
		[HttpGet("get-order-by-product-account-id")]
		public IActionResult GetOrderByProductAccountId()
		{
			return Ok();
		}
	}
}
