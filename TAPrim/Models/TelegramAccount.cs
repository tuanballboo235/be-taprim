using System;
using System.Collections.Generic;

namespace TAPrim.Models;

public partial class TelegramAccount
{
    public int TelegramAccountId { get; set; }

    public int TelegramUserId { get; set; }

    public int ChatId { get; set; }

    public int UserId { get; set; }

    public string? Username { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateTime CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual User User { get; set; } = null!;
}
