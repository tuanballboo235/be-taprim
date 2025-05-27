using TAPrim.Application.DTOs;
using TAPrim.Application.DTOs.Products;
using TAPrim.Models;

namespace TAPrim.Application
{
	public interface IProductService
	{
		Task<ApiResponseModel<Product>> CreateProductAsync(CreateProductRequest dto);

	}
}
