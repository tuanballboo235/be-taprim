using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TAPrim.Application;
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
		public async Task<IActionResult> GetNetflixMail(string email) {
			return ApiResponseHelper.HandleApiResponse(await _netflixServices.GetJsonDataAsync(email));
		}
	}
}
