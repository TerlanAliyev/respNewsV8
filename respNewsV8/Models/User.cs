using System;
using System.Collections.Generic;

namespace respNewsV8.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? UserName { get; set; }

    public string? UserPassword { get; set; }

    public string? UserRole { get; set; }
}
