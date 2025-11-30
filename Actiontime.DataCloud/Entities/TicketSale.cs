using System;
using System.Collections.Generic;

namespace Actiontime.DataCloud.Entities;

public partial class TicketSale
{
    public long Id { get; set; }

    public int? StatusId { get; set; }

    public string? OrderNumber { get; set; }

    public int? SaleTypeId { get; set; }

    public DateTime? Date { get; set; }

    public int? OurCompanyId { get; set; }

    public int? LocationId { get; set; }

    public int? EmployeeId { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerData { get; set; }

    public string? CustomerPhone { get; set; }

    public string? IdentityCard { get; set; }

    public string? Description { get; set; }

    /// <summary>
    /// 1 location on app, 2 office, 3 diğer
    /// </summary>
    public int? SaleChannelD { get; set; }

    public int? PriceCategoryId { get; set; }

    public int? PaymethodId { get; set; }

    public double? Amount { get; set; }

    public string? Currency { get; set; }

    public string? CardNumber { get; set; }

    public int? CardReaderId { get; set; }

    /// <summary>
    /// iptal veya iade durumlarında durum kodu seçmek için
    /// </summary>
    public int? ReasonId { get; set; }

    public int? PosStatusId { get; set; }

    public int? EnvironmentId { get; set; }

    public Guid Uid { get; set; }

    public int? LocalOrderId { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public int? RecordEmployeeId { get; set; }

    public DateTime? RecordDate { get; set; }

    public string? RecordIp { get; set; }

    public int? UpdateEmployeeId { get; set; }

    public DateTime? UpdateDate { get; set; }

    public string? UpdateIp { get; set; }

    public bool? IsSendPosTerminal { get; set; }

    public string? PosRegistryNumber { get; set; }

    public bool? IsFinancialization { get; set; }

    public bool? IsActive { get; set; }
}
