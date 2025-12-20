using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class CashDocument
{
    public int Id { get; set; }

    public int? LocationId { get; set; }

    public int? CashActionTypeId { get; set; }

    public int? PayMethodId { get; set; }

    public double? Amount { get; set; }

    public string? Currency { get; set; }

    public DateOnly? DocumentDate { get; set; }

    public string? Description { get; set; }

    public string? PhotoFile { get; set; }

    public DateTime? RecordDate { get; set; }

    public int? RecordEmployeeId { get; set; }

    public Guid? Uid { get; set; }
}
