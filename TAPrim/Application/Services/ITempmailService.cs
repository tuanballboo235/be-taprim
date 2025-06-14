using TAPrim.Application.DTOs.Tempmail;
using TAPrim.Application.DTOs;

namespace TAPrim.Application.Services
{
	public interface ITempmailService
	{
		Task<ApiResponseModel<List<TempmailEmailItemDto>>> EmailNetflixUpdateHouseFilter(string ipaddress);
		Task<ApiResponseModel<List<TempmailEmailItemDto>>> GetNetflixCodeLoginEmailFilter(string transactionCode);
		Task<ApiResponseModel<List<TempmailEmailItemDto>>> GetChatgptVerificationEmailFilter(string transactionCode);
		Task<ApiResponseModel<TempMailMessage>> GetMailContentByEmailId(string emailId);
	}
}
