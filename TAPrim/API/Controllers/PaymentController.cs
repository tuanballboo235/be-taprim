using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TAPrim.Application.DTOs;
using TAPrim.Application.Services;
using TAPrim.Common.Helpers;
using TAPrim.Infrastructure.Repositories;
using TAPrim.Shared.Constants;

namespace TAPrim.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PaymentController : ControllerBase
	{
		private readonly IPaymentService _paymentService;
		public PaymentController(IPaymentService paymentService)
		{
			_paymentService = paymentService;	
		}
		[HttpPost]
		public async Task<IActionResult> ReceiveAsync([FromBody] SePayWebhookDto data)
		{
			return Ok();
		}


	}
}
