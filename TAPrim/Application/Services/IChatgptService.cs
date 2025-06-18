using TAPrim.Application.DTOs.Netflix;
using TAPrim.Application.DTOs.Chatgpt;
using TAPrim.Application.DTOs.Common;

namespace TAPrim.Application.Services
{
    public interface IChatgptService
    {
        Task<ApiResponseModel<Chatgpt2FaDto>> GetChatgptOtp(string paymentCode, string secretCode);
    }
}
