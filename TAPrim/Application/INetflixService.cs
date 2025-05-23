using TAPrim.Application.DTOs;
using TAPrim.Application.DTOs.Netflix;

namespace TAPrim.Application
{
	public interface INetflixService
	{
		Task<ApiResponseModel<List<EmailResponseDto>>> GetJsonDataAsync(string email, int typeFilter);
	}
}
