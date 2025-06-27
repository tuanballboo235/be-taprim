using System;
using System.Collections.Generic;

namespace TAPrim.Models;

public partial class ProductOption
{
    public int ProductOptionId { get; set; }

    public int ProductId { get; set; }

    public int? DurationValue { get; set; }

    public string? DurationUnit { get; set; }

    public int? Quantity { get; set; }

    public string? Label { get; set; }

    public decimal? Price { get; set; }

    public int? DiscountPercent { get; set; }

    public string? ProductGuide { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ProductAccount> ProductAccounts { get; set; } = new List<ProductAccount>();
}
