using System;
using System.Collections.Generic;

namespace TAPrim.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public int Status { get; set; }

    public int CategoryId { get; set; }

    public string? Description { get; set; }

    public string ProductImage { get; set; } = null!;

    public DateTime? CreateAt { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ProductAccount> ProductAccounts { get; set; } = new List<ProductAccount>();

    public virtual ICollection<ProductOption> ProductOptions { get; set; } = new List<ProductOption>();
}
