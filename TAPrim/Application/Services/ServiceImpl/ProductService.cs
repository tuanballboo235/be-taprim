using Azure;
using TAPrim.Application.DTOs.Common;
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
        public ProductService(IProductRepository productRepo, IFileService fileService,IProductAccountRepository productAccountRepository)
        {
            _productRepo = productRepo;
            _fileService = fileService;
            _productAccountRepository = productAccountRepository;
        }

        public async Task<ApiResponseModel<Product>> CreateProductAsync(CreateProductRequest dto)
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

			var productOption = new ProductOption
			{
				ProductId = product.ProductId,
				DurationValue = dto.DiscountPercentDisplay,
				Price = dto.Price,
				Status = dto.Status,
				CategoryId = dto.CategoryId,
				AttentionNote = dto.AttentionNote,
				Description = dto.Description,
				ProductCode = dto.ProductCode,
				ProductImage = imagePath
			};
			await _productRepo.AddProductAsync(product);


			return new ApiResponseModel<Product>()
            {
                Data = product,
                Status = ApiResponseStatusConstant.SuccessStatus,
                Message = "Tạo mới sản phẩm thành công"
            };
        }

		public async Task<ApiResponseModel<ProductDetailResponseDto>> UpdateProductAsync(int productId ,UpdateProductRequest dto)
		{
			try
			{
				var product = await _productRepo.GetProductByIdAsync(productId);
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
				product.DiscountPercentDisplay = dto.DiscountPercentDisplay;
				product.Price = dto.Price ?? product.Price;
				product.Status = dto.Status ?? product.Status;
				product.CategoryId = dto.CategoryId ?? product.CategoryId;
				product.AttentionNote = dto.AttentionNote?? product.AttentionNote;
				product.Description = dto.Description ?? product.Description;
				product.ProductCode = dto.ProductCode ?? product.ProductCode;
				product.ProductImage = imagePath;  // nếu như ko truyền ảnh thì sẽ giữ nguyên ảnh gốc, còn truyền ảnh thì cũng đã cập nhật vào biến imgPath và gắn r

				var updated = await _productRepo.UpdateProductAsync(product);

				var response = new ProductDetailResponseDto
				{
					ProductId = product.ProductId,
					ProductName = product.ProductName,
					Price = product.Price,
					DiscountPercentDisplay = product.DiscountPercentDisplay,
					AttentionNote = product.AttentionNote,
					Description = product.Description,
					ProductImage = product.ProductImage,
					ProductCode = product.ProductCode,
					CategoryId = product.CategoryId,
					CategoryName = product.Category?.CategoryName ?? "Unknown",
					AccountStockQuantity = 0 // hoặc bỏ hoàn toàn field này nếu không cần
				};
				//xóa file ảnh cũ ngay khi update thành công
				await _fileService.DeleteImage(oldImgPath);
				return new ApiResponseModel<ProductDetailResponseDto>
				{
					Status = ApiResponseStatusConstant.SuccessStatus,
					Message = "Cập nhật thành công",
					Data = response
				};
			} catch (Exception e)
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
			var product = await _productRepo.GetProductByIdAsync(productId);
            var StockAccounts = await _productAccountRepository.GetQuantityStockProductAccountByProductId(productId);
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
				Price = product.Price,
				DiscountPercentDisplay = product.DiscountPercentDisplay,
				AttentionNote = product.AttentionNote,
				Description = product.Description?.ToString(), // nếu là int bạn cần xử lý riêng
				ProductImage = product.ProductImage,
				ProductCode = product.ProductCode,
				CategoryId = product.CategoryId,
				CategoryName = product.Category.CategoryName,
                AccountStockQuantity = StockAccounts
			};

			return new ApiResponseModel<ProductDetailResponseDto>
			{
				Status = ApiResponseStatusConstant.SuccessStatus,
				Data = dto
			};
		}

		private async Task<int> CalculateProductAccountQuantity(int productId)
		{
			int AccountStockQuantity = 0;	
			int SellCountTotal = 0;
			
			var productAccount = await _productAccountRepository.GetProductAccountByProductId(productId);

			foreach (var account in productAccount) { 
			
				SellCountTotal += account.SellCount.Value;
			}
			return SellCountTotal;
		}
		public async Task<List<ProductDetailResponseDto>> GetProductListAsync()
		{
			var products = await _productRepo.GetAllAsync();
			foreach(var product in products)
			{
				product.AccountStockQuantity = await CalculateProductAccountQuantity(product.ProductId);
			}
			return products;
		}
	}
}
