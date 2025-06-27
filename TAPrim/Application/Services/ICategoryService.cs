using TAPrim.Application.DTOs.Common;

namespace TAPrim.Application.Services
{
	public interface ICategoryService
	{
		ApiResponseModel<object> GetCategoryTreeAsync();
	}
}
