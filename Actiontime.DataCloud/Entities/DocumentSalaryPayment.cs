using System;
using System.Collections.Generic;

namespace Actiontime.DataCloud.Entities;

public partial class DocumentSalaryPayment
{
    public long Id { get; set; }

    public string? DocumentNumber { get; set; }

    public int? ToEmployeeId { get; set; }

    public double? Amount { get; set; }

    public string? Currency { get; set; }

    public DateTime? Date { get; set; }

    public DateTime? RecordDate { get; set; }

    public int? RecordEmployeeId { get; set; }

    public string? RecordIp { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateEmployee { get; set; }

    public string? UpdateIp { get; set; }

    public int? FromCashId { get; set; }

    public int? FromBankAccountId { get; set; }

    public double? ExchangeRate { get; set; }

    public double? SystemAmount { get; set; }

    public string? SystemCurrency { get; set; }

    public int? LocationId { get; set; }

    public int? OurCompanyId { get; set; }

    public long? ReferenceId { get; set; }

    public bool? IsActive { get; set; }

    public string? ActionTypeName { get; set; }

    public int? ActionTypeId { get; set; }

    public int? CategoryId { get; set; }

    public int? SalaryType { get; set; }

    public int? SalaryTypeId { get; set; }

    public string? Description { get; set; }

    public long? ResultId { get; set; }

    public int? EnvironmentId { get; set; }

    public Guid? Uid { get; set; }

    public bool? IsLumpSum { get; set; }

    public string? DocumentFile { get; set; }
}
