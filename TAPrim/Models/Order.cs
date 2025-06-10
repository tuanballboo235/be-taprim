using System;
using System.Collections.Generic;

namespace TAPrim.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? UserId { get; set; }

    public decimal? TotalAmount { get; set; }

    public int? CouponId { get; set; }

    public int ProductId { get; set; }

    public int? ProductAccountId { get; set; }

    public int? Status { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public int RemainGetCode { get; set; }

    public DateTime? ExpiredAt { get; set; }

    public string? ContactInfo { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Product Product { get; set; } = null!;

    public virtual ProductAccount? ProductAccount { get; set; }
}
