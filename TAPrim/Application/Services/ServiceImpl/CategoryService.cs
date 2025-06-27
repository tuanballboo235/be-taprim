using TAPrim.Application.DTOs.Common;
using TAPrim.Infrastructure.Repositories;
using TAPrim.Shared.Constants;

namespace TAPrim.Application.Services.ServiceImpl
{
	public class CategoryService:ICategoryService
	{
		private readonly ICategoryRepository _categoryRepository;
		public CategoryService(ICategoryRepository categoryRepository)
		{
			_categoryRepository = categoryRepository;
		}

		public  ApiResponseModel<object> GetCategoryTreeAsync() {

			var category =  _categoryRepository.GetAllCategories();
			return new ApiResponseModel<object>()
			{
				Status = ApiResponseStatusConstant.SuccessStatus,
				Data = _categoryRepository.BuildCategoryTree(category)
			};
		}

	}
}
