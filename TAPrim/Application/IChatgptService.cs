using TAPrim.Application.DTOs.Netflix;
using TAPrim.Application.DTOs;
using TAPrim.Application.DTOs.Chatgpt;

namespace TAPrim.Application
{
	public interface IChatgptService
	{
		Task<ApiResponseModel<Chatgpt2FaDto>> GetChatgptOtp(string paymentCode, string secretCode);
	}
}
