using System;
using System.Collections.Generic;

namespace respNewsV8.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public string? CategoryCoverUrl { get; set; }

    public virtual ICollection<News> News { get; set; } = new List<News>();
}
