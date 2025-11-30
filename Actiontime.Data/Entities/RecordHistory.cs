using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class RecordHistory
{
    public long Id { get; set; }

    public string TableName { get; set; } = null!;

    public Guid RowId { get; set; }

    public Guid RowUid { get; set; }

    public string? Crud { get; set; }

    public DateTime? CrudDate { get; set; }

    public string? CrudIp { get; set; }

    public int? CrudUserId { get; set; }

    public string? CrudUserFullName { get; set; }
}
