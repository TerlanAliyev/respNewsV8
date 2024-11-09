using System;
using System.Collections.Generic;

namespace respNewsV8.Models;

public partial class NewsVideo
{
    public int VideoId { get; set; }

    public string? VideoUrl { get; set; }

    public int? VideoNewsId { get; set; }

    public virtual News? VideoNews { get; set; }
}
