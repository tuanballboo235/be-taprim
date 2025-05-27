namespace TAPrim.Application.DTOs.ProductAccounts
{
	public class ProductAccountResponseDto
	{
		public int ProductId { get; set; }
		public string? AccountData { get; set; }
		public string? UsernameProductAccount { get; set; }
		public string? PasswordProductAccount { get; set; }
		public DateTime? DateChangePass { get; set; }
		public int? SellCount { get; set; }
		public int Status { get; set; }
	}
}
