namespace TAPrim.Application.DTOs.Order
{
	public class AdminProductOrderSummaryDto
	{
		public int TotalOrders { get; set; }
		public int PaidOrders { get; set; }
		public int PendingOrders { get; set; }
		public int TotalQuantity { get; set; }
		public decimal TotalRevenue { get; set; }
		public decimal AverageOrderValue { get; set; }
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
	}
}
