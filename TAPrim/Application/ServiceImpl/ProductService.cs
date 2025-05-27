using TAPrim.Application.DTOs;
using TAPrim.Application.DTOs.Products;
using TAPrim.Infrastructure;
using TAPrim.Models;
using TAPrim.Shared.Constants;

namespace TAPrim.Application.ServiceImpl
{
	public class ProductService : IProductService
	{
		private readonly IProductRepository _productRepo;
		private readonly IFileService _fileService;

		public ProductService(IProductRepository productRepo, IFileService fileService)
		{
			_productRepo = productRepo;
			_fileService = fileService;
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


	}
}
