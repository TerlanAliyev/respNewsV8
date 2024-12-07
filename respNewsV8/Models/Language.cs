using System;
using System.Collections.Generic;

namespace respNewsV8.Models;

public partial class Language
{
    public int LanguageId { get; set; }

    public string? LanguageName { get; set; }

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<News> News { get; set; } = new List<News>();
}
