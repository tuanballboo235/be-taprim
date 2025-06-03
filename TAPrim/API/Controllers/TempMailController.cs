using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TAPrim.Application.DTOs.Chatgpt;
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
		[HttpPost("get-2fa-chatgpt")]
		public async Task<IActionResult> GetNetflixMail([FromBody] Chatgpt2FaRequest request)
		{
			return ApiResponseHelper.HandleApiResponse(await _tempmailService.FilterEmailNetflixUpdateHouse(request.PaymentCode));
		}
	}
}
