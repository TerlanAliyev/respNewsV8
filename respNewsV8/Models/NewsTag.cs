using System;
using System.Collections.Generic;

namespace respNewsV8.Models;

public partial class NewsTag
{
    public int TagId { get; set; }

    public string? TagName { get; set; }

    public int? TagNewsId { get; set; }

    public virtual News? TagNews { get; set; }
}
