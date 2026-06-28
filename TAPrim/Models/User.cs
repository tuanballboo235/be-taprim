using System;
using System.Collections.Generic;

namespace TAPrim.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string? Password { get; set; }

    public bool IsEnable { get; set; }

    public DateTime? CreateAt { get; set; }

    public string? Phone { get; set; }

    public string Role { get; set; } = null!;

    public string? Email { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<TelegramAccount> TelegramAccounts { get; set; } = new List<TelegramAccount>();
}
