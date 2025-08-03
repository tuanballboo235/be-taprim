namespace TAPrim.Application.DTOs.ProductAccounts
{
	public class ProductAccountListResponse
	{
        public int ProductId { get; set; }
        public string ProductName { get; set; }
		public List<ProductAccountResponseDto> Items { get; set; }
	}
}
