using TAPrim.Application.DTOs.Category;
using TAPrim.Models;

namespace TAPrim.Infrastructure.Repositories
{
    public interface ICategoryRepository
    {
		Task<Category?> GetCategoryWithParentAsync(int categoryId);
		List<CategoryTreeDto> BuildCategoryTree(List<Category> flatCategories, int? parentId = null);
		List<Category> GetAllCategories();
	}
}
