using System;
using System.Collections.Generic;

namespace TAPrim.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? CouponId { get; set; }

    public int ProductId { get; set; }

    public int? ProductAccountId { get; set; }

    public int? Status { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public int RemainGetCode { get; set; }

    public DateTime? ExpiredAt { get; set; }

    public string? ContactInfo { get; set; }

    public int PaymentId { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? ClientNote { get; set; }

    public virtual Coupon? Coupon { get; set; }

    public virtual Payment Payment { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual ProductAccount? ProductAccount { get; set; }
}
