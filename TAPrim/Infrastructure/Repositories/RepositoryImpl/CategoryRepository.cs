using Microsoft.EntityFrameworkCore;
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
			var category = await _context.Categories
				.Include(c => c.Parent)
				.FirstOrDefaultAsync(c => c.CategoryId == categoryId);
			while (category.ParentId != null) { 
			
			return await _context.Categories
				.Include(c => c.Parent)
				.FirstOrDefaultAsync(c => c.CategoryId == category.ParentId);
			}
			return null;	
		}

	}
}
