using Microsoft.AspNetCore.Mvc;
using TAPrim.Application.DTOs.Common;
using TAPrim.Shared.Constants;

namespace TAPrim.Common.Helpers
{
    public static class ApiResponseHelper
	{
		public static IActionResult HandleApiResponse<T>(ApiResponseModel<T> response)
		{
			return response.Status == ApiResponseStatusConstant.SuccessStatus
				? new OkObjectResult(response)
				: new BadRequestObjectResult(response);
		}
	}
}
