using System;
using System.Collections.Generic;

namespace respNewsV8.Models;

public partial class Statisticss
{
    public int StatisticId { get; set; }

    public string VisitorIp { get; set; } = null!;

    public string? VisitorCountry { get; set; }

    public string? VisitorCity { get; set; }

    public DateTime VisitDate { get; set; }

    public int? VisitCount { get; set; }

    public bool? IsMobile { get; set; }

    public bool? IsDesktop { get; set; }

    public bool? IsEngLanguage { get; set; }

    public bool? IsAzLanguage { get; set; }

    public bool? IsRuLanguage { get; set; }
}
