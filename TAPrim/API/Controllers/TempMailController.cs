using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TAPrim.Application.DTOs.Chatgpt;
using TAPrim.Application.DTOs.Payment;
using TAPrim.Application.DTOs.Tempmail;
using TAPrim.Application.Services;
using TAPrim.Common.Helpers;

namespace TAPrim.API.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
	public class TempMailController : ControllerBase
	{
		private readonly ITempmailService _tempmailService;
		public TempMailController(ITempmailService tempmailService) {
			_tempmailService = tempmailService;
		}
		[HttpGet("get-netflix-update-family")]
		public async Task<IActionResult> GetNetflixMail()
		{
			string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            Console.WriteLine(ipAddress);
			return ApiResponseHelper.HandleApiResponse(await _tempmailService.EmailNetflixUpdateHouseFilter());
		}

		[HttpPost("get-netflix-code-sign-in")]
		public async Task<IActionResult> GetNetflixCodeSignIn([FromBody] TransactionCodeRequestDto request)
		{
			return ApiResponseHelper.HandleApiResponse(await _tempmailService.GetNetflixCodeLoginEmailFilter(request.TransactionCode));
		}

		[HttpPost("get-chatgpt-authen-code")]
		public async Task<IActionResult> GetChatgptAuthenCode([FromBody] TransactionCodeRequestDto request)
		{
			return ApiResponseHelper.HandleApiResponse(await _tempmailService.GetChatgptVerificationEmailFilter(request.TransactionCode));
		}
		[HttpPost("get-mail-content")]
		public async Task<IActionResult> GetMailContent([FromBody] MailContentRequestDto request)
		{
			Console.WriteLine($"📥 EmailId nhận được từ client: {request?.EmailId}");

			return ApiResponseHelper.HandleApiResponse(await _tempmailService.GetMailContentByEmailId(request.EmailId));
		}
	}
}
