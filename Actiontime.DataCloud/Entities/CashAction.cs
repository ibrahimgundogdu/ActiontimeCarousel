using System;
using System.Collections.Generic;

namespace Actiontime.DataCloud.Entities;

public partial class CashAction
{
    public long Id { get; set; }

    public int? CashId { get; set; }

    public int? LocationId { get; set; }

    public int? EmployeeId { get; set; }

    public int? CashActionTypeId { get; set; }

    public DateOnly? ActionDate { get; set; }

    public string? ProcessName { get; set; }

    public long? ProcessId { get; set; }

    public DateOnly? ProcessDate { get; set; }

    public string? DocumentNumber { get; set; }

    public long? SaleId { get; set; }

    public long? TicketSalePosPaymentId { get; set; }

    public string? Description { get; set; }

    public short? Direction { get; set; }

    public double? Collection { get; set; }

    public double? Payment { get; set; }

    public double? Amount { get; set; }

    public string? Currency { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public int? RecordEmployeeId { get; set; }

    public DateTime? RecordDate { get; set; }

    public int? UpdateEmployeeId { get; set; }

    public DateTime? UpdateDate { get; set; }

    public Guid? ProcessUid { get; set; }
}
