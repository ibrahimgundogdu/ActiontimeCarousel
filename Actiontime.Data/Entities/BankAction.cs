using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class BankAction
{
    public long Id { get; set; }

    public int? BankId { get; set; }

    public int? LocationId { get; set; }

    public int? BankActionTypeId { get; set; }

    public DateTime? ActionDate { get; set; }

    public int? OrderId { get; set; }

    public int? ProcessId { get; set; }

    public double? Collection { get; set; }

    public double? Payment { get; set; }

    public double? Amount { get; set; }

    public string? Currency { get; set; }

    public int? RecordEmployeeId { get; set; }

    public DateTime? RecordDate { get; set; }

    public Guid? ProcessUid { get; set; }
}
