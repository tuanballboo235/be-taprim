using Azure;
using TAPrim.Application.DTOs.Common;
using TAPrim.Application.DTOs.ProductAccounts;
using TAPrim.Application.DTOs.ProductOption;
using TAPrim.Application.DTOs.Products;
using TAPrim.Infrastructure.Repositories;
using TAPrim.Infrastructure.Repositories.RepositoryImpl;
using TAPrim.Models;
using TAPrim.Shared.Constants;

namespace TAPrim.Application.Services.ServiceImpl
{
	public class ProductService : IProductService
	{
		private readonly IProductRepository _productRepo;
		private readonly IFileService _fileService;
		private readonly IProductAccountRepository _productAccountRepository;
		public ProductService(IProductRepository productRepo, IFileService fileService, IProductAccountRepository productAccountRepository)
		{
			_productRepo = productRepo;
			_fileService = fileService;
			_productAccountRepository = productAccountRepository;
		}

		public async Task<ApiResponseModel<Product>> CreateProductAsync(CreateProductRequest dto)
		{
			try
			{
				string imagePath = "default.jpg";

				if (dto.ProductImage != null)
				{
					imagePath = await _fileService.SaveImageAsync(dto.ProductImage);
				}

				var product = new Product
				{
					ProductName = dto.ProductName,
					Status = dto.Status,
					CategoryId = dto.CategoryId,
					Description = dto.Description,
					ProductImage = imagePath,
					CreateAt = DateTime.Now,
				};

				await _productRepo.AddProductAsync(product);

			

				return new ApiResponseModel<Product>()
				{
					Data = product,
					Status = ApiResponseStatusConstant.SuccessStatus,
					Message = "Tạo mới sản phẩm thành công"
				};
			}
			catch (Exception ex)
			{
				return new ApiResponseModel<Product>()
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = ex.Message
				};

			}

		}

		public async Task<ApiResponseModel<ProductDetailResponseDto>> UpdateProductAsync(int productId, UpdateProductRequest dto)
		{
			try
			{
				var product = await _productRepo.GetProductById(productId);
				if (product == null)
				{
					return new ApiResponseModel<ProductDetailResponseDto>
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Không tìm thấy sản phẩm"
					};
				}
				//lưu lại đường dẫn cũ của file để xóa 
				string oldImgPath = product.ProductImage;

				string imagePath = product.ProductImage;
				if (dto.ProductImage != null)
				{
					imagePath = await _fileService.SaveImageAsync(dto.ProductImage);
				}

				// Gán lại thông tin
				product.ProductName = dto.ProductName ?? product.ProductName;

				product.Status = dto.Status ?? product.Status;
				product.CategoryId = dto.CategoryId ?? product.CategoryId;

				product.Description = dto.Description ?? product.Description;

				product.ProductImage = imagePath;  // nếu như ko truyền ảnh thì sẽ giữ nguyên ảnh gốc, còn truyền ảnh thì cũng đã cập nhật vào biến imgPath và gắn r

				var updated = await _productRepo.UpdateProductAsync(product);

				//xóa file ảnh cũ ngay khi update thành công
				await _fileService.DeleteImage(oldImgPath);
				return new ApiResponseModel<ProductDetailResponseDto>
				{
					Status = ApiResponseStatusConstant.SuccessStatus,
					Message = "Cập nhật thành công",
				};
			}
			catch (Exception e)
			{
				return new ApiResponseModel<ProductDetailResponseDto>
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = "Đã xảy ra lỗi",
				};
			}
		}

		public async Task<ApiResponseModel<ProductDetailResponseDto>> GetProductDetailAsync(int productId)
		{
			var product = await _productRepo.GetProductDtoByProductOptionIdAsync(productId);
			var StockAccounts = await _productAccountRepository.GetQuantityStockProductAccountByProductOptionId(productId);
			if (product == null)
			{
				return new ApiResponseModel<ProductDetailResponseDto>
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = "Product not found"
				};
			}

			var dto = new ProductDetailResponseDto
			{
				ProductId = product.ProductId,
				ProductName = product.ProductName,

				Description = product.Description?.ToString(), // nếu là int bạn cần xử lý riêng
				ProductImage = product.ProductImage,
				CategoryId = product.CategoryId,
				CategoryName = product.CategoryName,
			};

			return new ApiResponseModel<ProductDetailResponseDto>
			{
				Status = ApiResponseStatusConstant.SuccessStatus,
				Data = dto
			};
		}
	

		public async Task<ApiResponseModel<object>> GetProductOptionDataByProductId(int productId)
		{
			try
			{
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.SuccessStatus,
					Data = await _productRepo.GetProductOptionByProductId(productId)
				};

			}
			catch (Exception ex)
			{
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.FailedStatus,
				};
			}
		}


		public async Task<ApiResponseModel<object>> GetProductByCategory()
		{
			try
			{
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.SuccessStatus,
					Data = await _productRepo.GetListProductByCategoryId()
				};

			}
			catch (Exception ex)
			{
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.FailedStatus,
				};
			}
		}

		public async Task<ApiResponseModel<object>> GetProductDetailByProductId(int productId)
		{
			try
			{
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.SuccessStatus,
					Data = await _productRepo.GetProductOptionByProductId(productId)
				};

			}
			catch (Exception ex)
			{
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.FailedStatus,
				};
			}
		}

		public async Task<ApiResponseModel<object>> UpdateProductOptionById(int id, UpdateProductOptionRequest request)
		{
			try
			{
				var productOption = await _productRepo.GetProductOptionByIdAsync(id);
				if (productOption == null)
				{
					return new ApiResponseModel<object>
					{
						Status = ApiResponseStatusConstant.FailedStatus,
						Message = "Không tìm thấy sản phẩm"
					};
				}
				//lưu lại đường dẫn cũ của file để xóa 
				string oldImgPath = productOption.ProductOptionImage;

				string imagePath = productOption.ProductOptionImage;
				if (request.ProductOptionImage != null)
				{
					imagePath = await _fileService.SaveImageAsync(request.ProductOptionImage);
				}

				// Gán lại thông tin

				productOption.DurationUnit = request.DurationUnit ?? productOption.DurationUnit;
				productOption.DurationValue = request.DurationValue ?? productOption.DurationValue;

				productOption.DiscountPercent = request.DiscountPercent ?? productOption.DiscountPercent;
				productOption.Quantity = request.Quantity ?? productOption.Quantity;
				productOption.Label = request.Label ?? productOption.Label;
				productOption.Price = request.Price ?? productOption.Price;
				productOption.ProductGuide = request.ProductGuide ?? productOption.ProductGuide;
				productOption.ProductGuide = request.ProductGuide ?? productOption.ProductGuide;


				productOption.ProductOptionImage = imagePath;  // nếu như ko truyền ảnh thì sẽ giữ nguyên ảnh gốc, còn truyền ảnh thì cũng đã cập nhật vào biến imgPath và gắn r

				var updated = await _productRepo.UpdateProductOptionAsync(productOption);

				//xóa file ảnh cũ ngay khi update thành công
				await _fileService.DeleteImage(oldImgPath);
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.SuccessStatus,
					Message = "Cập nhật thành công",
				};
			}
			catch (Exception e)
			{
				return new ApiResponseModel<object>
				{
					Status = ApiResponseStatusConstant.FailedStatus,
					Message = "Đã xảy ra lỗi",
				};
			}
		}
	}
}
