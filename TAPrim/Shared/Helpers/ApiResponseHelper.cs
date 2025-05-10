using BasketballAcademyManagementSystemAPI.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using TAPrim.Shared.Constants;

namespace BasketballAcademyManagementSystemAPI.Common.Helpers
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
