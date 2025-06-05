using TAPrim.Models;

namespace TAPrim.Infrastructure.Repositories
{
    public interface ICategoryRepository
    {
		Task<Category?> GetCategoryWithParentAsync(int categoryId);
    }
}
