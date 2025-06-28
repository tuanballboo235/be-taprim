using System;
using System.Collections.Generic;

namespace TAPrim.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public int? ParentId { get; set; }

    public DateTime? CreateAt { get; set; }

    public string? CategoryType { get; set; }

    public string? CategoryDescription { get; set; }

    public virtual ICollection<Category> InverseParent { get; set; } = new List<Category>();

    public virtual Category? Parent { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
