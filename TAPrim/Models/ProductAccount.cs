using System;
using System.Collections.Generic;

namespace TAPrim.Models;

public partial class ProductAccount
{
    public int ProductAccountId { get; set; }

    public int ProductId { get; set; }

    public string? AccountData { get; set; }

    public string? UsernameProductAccount { get; set; }

    public string? PasswordProductAccount { get; set; }

    public int Status { get; set; }

    public DateTime? DateChangePass { get; set; }

    public int? SellCount { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Product Product { get; set; } = null!;
}
