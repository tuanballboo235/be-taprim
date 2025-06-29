using TAPrim.Application.DTOs.Common;
using TAPrim.Application.DTOs.ProductOption;
using TAPrim.Application.DTOs.Products;
using TAPrim.Models;

namespace TAPrim.Application.Services
{
    public interface IProductService
    {
        Task<ApiResponseModel<Product>> CreateProductAsync(CreateProductRequest dto);
        Task<ApiResponseModel<ProductDetailResponseDto>> GetProductDetailAsync(int productId);
		Task<ApiResponseModel<ProductDetailResponseDto>> UpdateProductAsync(int productId, UpdateProductRequest dto);
		Task<ApiResponseModel<object>> GetProductOptionDataByProductId(int productId);
		Task<ApiResponseModel<object>> GetProductByCategory();
		Task<ApiResponseModel<object>> GetProductDetailByProductId(int productId);
	}
}
