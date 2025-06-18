using TAPrim.Models;

namespace TAPrim.Application.DTOs.ProductAccounts
{
	public class UpdateProductProductAccountRequest
	{
		public int ProductId { get; set; }

		public string? AccountData { get; set; }

		public string? UsernameProductAccount { get; set; }

		public string? PasswordProductAccount { get; set; }

		public int Status { get; set; }

		public DateTime? DateChangePass { get; set; }

		public int? SellCount { get; set; }

		public string? Note { get; set; }

		public DateTime? SellFrom { get; set; }

		public DateTime? SellTo { get; set; }
	}
}
