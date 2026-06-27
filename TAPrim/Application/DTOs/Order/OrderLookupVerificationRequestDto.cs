namespace TAPrim.Application.DTOs.Order
{
	public class OrderLookupVerificationRequestDto
	{
		public string TransactionCode { get; set; } = string.Empty;
		public string VerificationCode { get; set; } = string.Empty;
	}
}
