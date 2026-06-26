namespace TAPrim.Application.DTOs.Payment
{
	public class PaymentReservationMetadata
	{
		public int Quantity { get; set; } = 1;
		public string? ClientNote { get; set; }
		public List<PaymentReservationItem> Items { get; set; } = new();
	}

	public class PaymentReservationItem
	{
		public int ProductAccountId { get; set; }
		public int Quantity { get; set; }
	}
}
