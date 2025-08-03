using System;
using System.Collections.Generic;

namespace TAPrim.Models;

public partial class ProductAccount
{
    public int ProductAccountId { get; set; }

    public int ProductOptionId { get; set; }

    public string? AccountData { get; set; }

    public string? UsernameProductAccount { get; set; }

    public string? PasswordProductAccount { get; set; }

    public int Status { get; set; }

    public DateTime? DateChangePass { get; set; }

    public int? SellCount { get; set; }

    public string? Note { get; set; }

    public DateTime? SellFrom { get; set; }

    public DateTime? SellTo { get; set; }

    public DateTime CreateAt { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ProductOption ProductOption { get; set; } = null!;
}
