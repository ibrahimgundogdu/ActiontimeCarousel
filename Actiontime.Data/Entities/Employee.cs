using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class Employee
{
    public int Id { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? CountryPhoneCode { get; set; }

    public string? Mobile { get; set; }

    public string? SmsNumber { get; set; }

    public string? Mobile2 { get; set; }

    public string? Whatsapp { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? FotoFile { get; set; }

    public short? RoleId { get; set; }

    public short OurCompanyId { get; set; }

    public Guid? EmployeeUid { get; set; }

    public int? RoleGroupId { get; set; }
}
