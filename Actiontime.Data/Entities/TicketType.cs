using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class TicketType
{
    public int Id { get; set; }

    public string? TicketTypeName { get; set; }

    public bool? IsActive { get; set; }
}
