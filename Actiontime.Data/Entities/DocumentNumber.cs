using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class DocumentNumber
{
    public short Id { get; set; }

    public string? Name { get; set; }

    public string? Prefix { get; set; }

    public long? Counter { get; set; }

    public DateTime? Date { get; set; }
}
