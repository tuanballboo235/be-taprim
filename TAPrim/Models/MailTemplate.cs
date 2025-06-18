using System;
using System.Collections.Generic;

namespace TAPrim.Models;

public partial class MailTemplate
{
    public string MailTemplateId { get; set; } = null!;

    public string TemplateTitle { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
