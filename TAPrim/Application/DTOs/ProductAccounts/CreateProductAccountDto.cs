namespace TAPrim.Application.DTOs.ProductAccounts
{
	public class CreateProductAccountDto
	{
		public string? AccountData { get; set; }
		public string? UsernameProductAccount { get; set; }
		public string? PasswordProductAccount { get; set; }
		public DateTime? DateChangePass { get; set; }

		public int? SellCount { get; set; }
        public DateTime? SellDateFrom { get; set; }
        public DateTime? SellDateTo { get; set; }
        public int Status { get; set; }
	}
}
