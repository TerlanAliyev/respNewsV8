using System;
using System.Collections.Generic;

namespace respNewsV8.Models;

public partial class Subscriber
{
    public int SubId { get; set; }

    public string? SubEmail { get; set; }

    public DateTime? SubDate { get; set; }
}
