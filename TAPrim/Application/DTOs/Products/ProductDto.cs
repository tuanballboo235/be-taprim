namespace TAPrim.Application.DTOs.Products
{
	public class ProductDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Image { get; set; }
		public decimal? MinPrice { get; set; }
		public decimal? MaxPrice { get; set; }
        public int Status { get; set; }
        public int StockAccount{ get; set; }
		public bool CanSell { get; set; }
	}
}
