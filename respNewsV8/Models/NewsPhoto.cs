using System;
using System.Collections.Generic;

namespace respNewsV8.Models;

public partial class NewsPhoto
{
    public int PhotoId { get; set; }

    public string? PhotoUrl { get; set; }

    public int? PhotoNewsId { get; set; }

    public virtual News? PhotoNews { get; set; }
}
