using System;
using System.Collections.Generic;

namespace Actiontime.DataCloud.Entities;

public partial class DocumentExpenseSlip
{
    public long Id { get; set; }

    public int? OurCompanyId { get; set; }

    public int? LocationId { get; set; }

    public DateOnly? DocumentDate { get; set; }

    public string? DocumentNumber { get; set; }

    public int? CustomerId { get; set; }

    public string? CustomerAddress { get; set; }

    public int? PayMethodId { get; set; }

    public double? Amount { get; set; }

    public string? Currency { get; set; }

    public double? ExchangeRate { get; set; }

    public double? SystemAmount { get; set; }

    public string? SystemCurrency { get; set; }

    public long? ReferenceId { get; set; }

    public long? SaleId { get; set; }

    public long? SaleRowId { get; set; }

    public string? ActionTypeName { get; set; }

    public int? ActionTypeId { get; set; }

    public string? Description { get; set; }

    public long? ResultId { get; set; }

    public int? EnvironmentId { get; set; }

    public Guid? Uid { get; set; }

    public DateTime? RecordDate { get; set; }

    public int? RecordEmployeeId { get; set; }

    public string? RecordIp { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateEmployee { get; set; }

    public string? UpdateIp { get; set; }

    public bool? IsConfirmed { get; set; }

    public bool? IsActive { get; set; }
}
