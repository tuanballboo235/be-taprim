using System;
using System.Collections.Generic;

namespace TAPrim.Models;

public partial class TempMailEmailStore
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string EmailId { get; set; } = null!;
}
