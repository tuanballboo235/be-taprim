using TAPrim.Application.DTOs.Common;
using TAPrim.Application.DTOs.Products;
using TAPrim.Models;

namespace TAPrim.Application.Services
{
    public interface IProductService
    {
        Task<ApiResponseModel<Product>> CreateProductAsync(CreateProductRequest dto);
        Task<ApiResponseModel<ProductDetailResponseDto>> GetProductDetailAsync(int productId);
		Task<List<ProductDetailResponseDto>> GetProductListAsync();
		Task<ApiResponseModel<ProductDetailResponseDto>> UpdateProductAsync(int productId, UpdateProductRequest dto);
	}
}
