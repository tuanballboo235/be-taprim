using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TAPrim.Application.DTOs.Chatgpt;
using TAPrim.Application.Services;
using TAPrim.Common.Helpers;

namespace TAPrim.API.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
	public class ChatgptController : ControllerBase
	{
		private readonly IChatgptService _chatgService;
		public ChatgptController(IChatgptService chatgService) { 
			_chatgService = chatgService;
		}
		[HttpPost("get-2fa-chatgpt")]
		public async Task<IActionResult> GetNetflixMail([FromBody] Chatgpt2FaRequest request)
		{
			return ApiResponseHelper.HandleApiResponse(await _chatgService.GetChatgptOtp(request.PaymentCode, request.HashCode));
		}
	}
}
