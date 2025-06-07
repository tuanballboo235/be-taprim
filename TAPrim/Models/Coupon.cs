using System;
using System.Collections.Generic;

namespace TAPrim.Models;

public partial class Coupon
{
    public int CouponId { get; set; }

    public string? CouponCode { get; set; }

    public int? DiscountPercent { get; set; }

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidUntil { get; set; }

    public bool? IsActive { get; set; }

    public int? CreateAt { get; set; }

    public int RemainTurn { get; set; }
}
