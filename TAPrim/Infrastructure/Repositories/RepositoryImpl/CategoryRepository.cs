using Microsoft.EntityFrameworkCore;
using TAPrim.Application.DTOs.Category;
using TAPrim.Models;

namespace TAPrim.Infrastructure.Repositories.RepositoryImpl
{
	public class CategoryRepository : ICategoryRepository
	{
		private readonly TaprimContext _context;

		public CategoryRepository(TaprimContext context)
		{
			_context = context;
		}
		public async Task<Category?> GetCategoryWithParentAsync(int categoryId)
		{
			var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId);
			if (category == null)
				return null;

			Category? current = category;

			while (current?.ParentId != null)
			{
				current.Parent = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == current.ParentId);
				current = current.Parent;
			}

			return category;
		}
		//đệ quy lấy ra list tree category
		public  List<CategoryTreeDto> BuildCategoryTree(List<Category> flatCategories, int? parentId = null)
		{
			return flatCategories
				.Where(c => c.ParentId == parentId)
				.Select(c => new CategoryTreeDto
				{
					CategoryId = c.CategoryId,
					CategoryName = c.CategoryName,
					CategoryType = c.CategoryType,
					Children =  BuildCategoryTree(flatCategories, c.CategoryId)
				})
				.ToList();
		}
		public List<Category> GetAllCategories()
		{
			return  _context.Categories.ToList();
		}
	}
}
