using System;
using System.Collections.Generic;

namespace respNewsV8.Models;

public partial class İnfographic
{
    public int InfId { get; set; }

    public string? InfName { get; set; }

    public string? InfPhoto { get; set; }

    public DateTime? InfPostDate { get; set; }
}
