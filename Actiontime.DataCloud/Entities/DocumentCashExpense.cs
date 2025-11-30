using System;
using System.Collections.Generic;

namespace Actiontime.DataCloud.Entities;

public partial class DocumentCashExpense
{
    public long Id { get; set; }

    public string? DocumentNumber { get; set; }

    public int? ExpenseTypeId { get; set; }

    public int? ToEmployeeId { get; set; }

    public int? ToCustomerId { get; set; }

    public int? ToBankAccountId { get; set; }

    public double? Amount { get; set; }

    public string? Currency { get; set; }

    public DateTime? Date { get; set; }

    public DateTime? RecordDate { get; set; }

    public int? RecordEmployeeId { get; set; }

    public string? RecordIp { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateEmployee { get; set; }

    public string? UpdateIp { get; set; }

    public int? CashId { get; set; }

    public double? ExchangeRate { get; set; }

    public double? SystemAmount { get; set; }

    public string? SystemCurrency { get; set; }

    public int? LocationId { get; set; }

    public int? OurCompanyId { get; set; }

    public string? ReferenceTableModel { get; set; }

    public long? ReferenceId { get; set; }

    public bool? IsActive { get; set; }

    public string? ActionTypeName { get; set; }

    public int? ActionTypeId { get; set; }

    public string? Description { get; set; }

    public DateTime? SlipDate { get; set; }

    public string? SlipNumber { get; set; }

    public string? SlipDocument { get; set; }

    public long? ResultId { get; set; }

    public int? EnvironmentId { get; set; }

    public Guid? Uid { get; set; }

    public string? SlipPath { get; set; }
}
