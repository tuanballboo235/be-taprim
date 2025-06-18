using TAPrim.Application.DTOs.Common;
using TAPrim.Application.DTOs.Netflix;

namespace TAPrim.Application.Services
{
    public interface INetflixService
    {
        Task<ApiResponseModel<List<EmailResponseDto>>> GetJsonDataAsync(string email, int typeFilter);
    }
}
