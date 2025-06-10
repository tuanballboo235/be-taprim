using TAPrim.Application.DTOs;
using TAPrim.Application.DTOs.Products;
using TAPrim.Models;

namespace TAPrim.Application.Services
{
    public interface IProductService
    {
        Task<ApiResponseModel<Product>> CreateProductAsync(CreateProductRequest dto);
        Task<ApiResponseModel<ProductDetailResponseDto>> GetProductDetailAsync(int productId);
		Task<List<ProductDetailResponseDto>> GetProductListAsync();

	}
}
