using System;
using System.Collections.Generic;

namespace Actiontime.DataCloud.Entities;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string? IdentityType { get; set; }

    public string? IdentityNumber { get; set; }

    public string? Title { get; set; }

    public string? FullName { get; set; }

    public string? FullNameSearch { get; set; }

    public string? Email { get; set; }

    public string? CountryPhoneCode { get; set; }

    public string? Mobile { get; set; }

    public string? SmsNumber { get; set; }

    public string? Mobile2 { get; set; }

    public string? Whatsapp { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? FotoFile { get; set; }

    public int? RoleId { get; set; }

    public int OurCompanyId { get; set; }

    public string? Description { get; set; }

    public bool? IsTemp { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDismissal { get; set; }

    public int? RecordEmployeeId { get; set; }

    public DateTime? RecordDate { get; set; }

    public DateTime? DateStart { get; set; }

    public DateTime? DateEnd { get; set; }

    public string? DismissDescription { get; set; }

    public Guid? EmployeeUid { get; set; }

    public int? ShiftTypeId { get; set; }

    public int? StatusId { get; set; }

    public int? RoleGroupId { get; set; }

    public string? RecordIp { get; set; }

    public int? UpdateEmployeeId { get; set; }

    public DateTime? UpdateDate { get; set; }

    public string? UpdateIp { get; set; }

    public int? DepartmentId { get; set; }

    public int? AreaCategoryId { get; set; }

    public int? SalaryCategoryId { get; set; }

    public short? SalaryPaymentTypeId { get; set; }

    public int? SequenceId { get; set; }

    public int? PositionId { get; set; }

    public string? Address { get; set; }

    public int? Country { get; set; }

    public int? State { get; set; }

    public int? City { get; set; }

    public string? PostCode { get; set; }

    public string? Iban { get; set; }

    public int? BankId { get; set; }

    public string? FoodCardNumber { get; set; }

    public string? Sgkbranch { get; set; }

    public int? LocationId { get; set; }
}
