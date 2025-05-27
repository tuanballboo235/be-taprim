using TAPrim.Models;

namespace TAPrim.Infrastructure
{
	public interface IProductRepository
	{
		Task AddProductAsync(Product product);

	}
}
