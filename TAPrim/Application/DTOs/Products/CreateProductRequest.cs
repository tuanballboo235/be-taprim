using System.ComponentModel.DataAnnotations;

namespace TAPrim.Application.DTOs.Products
{
	public class CreateProductRequest
	{

		[Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
		[StringLength(100, MinimumLength = 3, ErrorMessage = "Tên sản phẩm phải từ 3 đến 100 ký tự")]
		public string? ProductName{ get; set; }
		[Range(0, 2, ErrorMessage = "Trạng thái không hợp lệ")]
		public int Status {  get; set; }
		public string? Description { get; set; }

        public IFormFile ProductImage { get; set; } = null!;
		public DateTime? CreatedAt { get; set; }
		[Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục hợp lệ")]
		public int CategoryId { get; set; }

	}
}
