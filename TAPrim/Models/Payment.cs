using System;
using System.Collections.Generic;

namespace TAPrim.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int OrderId { get; set; }

    public string TransactionCode { get; set; } = null!;

    public int? PaymentMethod { get; set; }

    public DateTime? PaidDateAt { get; set; }

    public DateTime CreateAt { get; set; }

    public int? UserId { get; set; }

    public decimal Amount { get; set; }

    public string? Note { get; set; }

    public int? Status { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual User? User { get; set; }
}
