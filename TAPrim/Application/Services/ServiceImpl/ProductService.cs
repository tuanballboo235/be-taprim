using TAPrim.Application.DTOs;
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
                DiscountPercentDisplay = dto.DiscountPercentDisplay,
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
		public async Task<List<ProductDetailResponseDto>> GetProductListAsync()
		{
			return await _productRepo.GetAllAsync();
		}
	}
}
