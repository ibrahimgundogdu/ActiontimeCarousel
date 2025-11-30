using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class SyncProcess
{
    public long Id { get; set; }

    public string Entity { get; set; } = null!;

    /// <summary>
    /// CRUD Create Update Delete
    /// </summary>
    public short Process { get; set; }

    public long EntityId { get; set; }

    public Guid? EntityUid { get; set; }

    public DateTime DateCreate { get; set; }

    public string? FilePath { get; set; }
}
