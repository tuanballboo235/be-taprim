using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TAPrim.Application.DTOs.Common;
using TAPrim.Application.DTOs.Payment;
using TAPrim.Application.DTOs.Products;
using TAPrim.Application.Services;
using TAPrim.Application.Services.ServiceImpl;
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

		//Xác nhận thanh toán thành công từ sepay và set product account cho đơn hàng
		[HttpPost("sepay-webhook")]
		public async Task<IActionResult> ReceiveAsync([FromBody] SePayWebhookDto data)
		{
			return ApiResponseHelper.HandleApiResponse(await _paymentService.SetProductAccountForPaymentByTransactionCode(data));
		}

		[HttpPost("generate-vietqr")]
		public async Task<IActionResult> GenerateQrAndCreatePayment(CreatePaymentRequest request)
		{
			return ApiResponseHelper.HandleApiResponse(await _paymentService.GenerateQrAsync(request));
		}

		[HttpGet("filter-payments")]
		public async Task<IActionResult> GetFilteredPayments([FromQuery] PaymentFilterDto filter)
		{
			return ApiResponseHelper.HandleApiResponse(await _paymentService.GetPaymentsAsync(filter));

		}

		[HttpGet("test-send-mails")]
		public async Task<IActionResult> TestSendMail()
		{
			return ApiResponseHelper.HandleApiResponse(await _paymentService.TestEmail());
		}

		[HttpPost("clear-order-and-payment-temp-by-paymentId")]
		public async Task<IActionResult> ClearOrderAndPaymentTempByPaymentId([FromBody] int paymentId)
		{
			return ApiResponseHelper.HandleApiResponse(await _paymentService.ClearOrderAndPaymentTempByPaymentId(paymentId));

		}

	}
}