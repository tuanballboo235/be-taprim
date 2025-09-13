using TAPrim.Application.DTOs.Common;
using TAPrim.Application.DTOs.ProductAccounts;
using TAPrim.Application.DTOs.ProductOption;
using TAPrim.Application.DTOs.Products;
using TAPrim.Models;

namespace TAPrim.Application.Services
{
    public interface IProductService
    {

		//Product Option
		Task<ApiResponseModel<object>> GetProductOptionDataByProductId(int productId);
		Task<ApiResponseModel<object>> UpdateProductOptionById(int id , UpdateProductProductAccountRequest request);

		//Product 
		Task<ApiResponseModel<Product>> CreateProductAsync(CreateProductRequest dto);
        Task<ApiResponseModel<ProductDetailResponseDto>> GetProductDetailAsync(int productId);
		Task<ApiResponseModel<ProductDetailResponseDto>> UpdateProductAsync(int productId, UpdateProductRequest dto);
		Task<ApiResponseModel<object>> GetProductByCategory();
		Task<ApiResponseModel<object>> GetProductDetailByProductId(int productId);
	}
}
