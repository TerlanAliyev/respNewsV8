using System;
using System.Collections.Generic;

namespace respNewsV8.Models;

public partial class Owner
{
    public int OwnerId { get; set; }

    public string? OwnerName { get; set; }

    public int? OwnerTotal { get; set; }

    public virtual ICollection<News> News { get; set; } = new List<News>();
}
