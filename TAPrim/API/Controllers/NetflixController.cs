using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TAPrim.Application.DTOs.Netflix;
using TAPrim.Application.Services;
using TAPrim.Common.Helpers;

namespace TAPrim.API.Controllers
{
    [Route("api/netflix")]
	[ApiController]
	public class NetflixController : ControllerBase
	{
		private readonly INetflixService _netflixServices;

		public NetflixController(INetflixService netflixServices)
		{
			_netflixServices = netflixServices;
		}

		[HttpGet("get-email-temporary-watch-netflix")]
		public async Task<IActionResult> GetNetflixMail([FromQuery] NetflixMailRequest request) {
			return ApiResponseHelper.HandleApiResponse(await _netflixServices.GetJsonDataAsync(request.Email, request.TypeMailRequest));
		}

		[HttpGet("get-email-code-netflixs")]
		public async Task<IActionResult> GetNetflixCode([FromQuery] NetflixMailRequest request)
		{
			return ApiResponseHelper.HandleApiResponse(await _netflixServices.GetJsonDataAsync(request.Email, request.TypeMailRequest));
		}
	}
}
