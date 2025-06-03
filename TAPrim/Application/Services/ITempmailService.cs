using TAPrim.Application.DTOs.Tempmail;
using TAPrim.Application.DTOs;

namespace TAPrim.Application.Services
{
	public interface ITempmailService
	{
		Task<ApiResponseModel<List<TempmailEmailItemDto>>> FilterEmailNetflixUpdateHouse(string transactionCode);
	}
}
