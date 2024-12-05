using System;
using System.Collections.Generic;

namespace respNewsV8.Models;

public partial class Messagess
{
    public int MessageId { get; set; }

    public string? MessageTitle { get; set; }

    public string? MeesageContext { get; set; }

    public string? MessageMail { get; set; }

    public DateTime? MessageDate { get; set; }

    public bool? MessageIsRead { get; set; }
}
