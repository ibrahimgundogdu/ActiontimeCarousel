using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class LocationManager
    {

        #region Exam
        #region AddExam
        //public Result<DocumentEmployeePermit> AddEmployeePermit(EmployeePermit permit, AuthenticationModel authentication)
        //{
        //    Result<DocumentEmployeePermit> result = new Result<DocumentEmployeePermit>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    if (permit != null && authentication != null)
        //    {
        //        using (ActionTimeEntities Db = new ActionTimeEntities())
        //        {
        //            try
        //            {
        //                // aynı tarih ile çakışan başka izni var mı kontrol edilir.
        //                var isotherpermit = Db.DocumentEmployeePermit.FirstOrDefault(x => x.ID != permit.ID && x.UID != permit.UID && x.Date == permit.Date && x.EmployeeID == permit.EmployeeID && x.IsActive == true && (x.DateBegin >= permit.DateBegin || x.DateEnd <= permit.DateEnd));

        //                if (isotherpermit == null)
        //                {


        //                    var location = Db.Location.FirstOrDefault(x => x.LocationID == permit.LocationID);
        //                    var permittype = Db.PermitType.FirstOrDefault(x => x.ID == permit.PermitTypeID);
        //                    string prefix = location.OurCompanyID == 1 ? "PRM" : "IZN";
        //                    var locationstats = Db.LocationStats.FirstOrDefault(x => x.LocationID == permit.LocationID && x.StatsID == 2 && x.OptionID == 3);


        //                    var empunits = Db.EmployeeSalary.Where(x => x.EmployeeID == permit.EmployeeID && x.DateStart <= permit.Date && x.Hourly > 0).OrderByDescending(x => x.DateStart).FirstOrDefault();

        //                    double unithour = empunits?.Hourly ?? 0;


        //                    if (location.OurCompanyID == 1 && locationstats != null)
        //                    {
        //                        unithour = unithour + 1;
        //                    }

        //                    DocumentEmployeePermit employeePermit = new DocumentEmployeePermit();

        //                    employeePermit.ActionTypeID = permit.ActinTypeID;
        //                    employeePermit.ActionTypeName = permit.ActionTypeName;
        //                    employeePermit.Currency = authentication.ActionEmployee.OurCompany.Currency;
        //                    employeePermit.Date = permit.Date;
        //                    employeePermit.DateBegin = permit.DateBegin;
        //                    employeePermit.DateEnd = permit.DateEnd;
        //                    employeePermit.Description = permit.Description;
        //                    employeePermit.DocumentNumber = OfficeHelper.GetDocumentNumber(location.OurCompanyID, prefix);
        //                    employeePermit.EmployeeID = permit.EmployeeID;
        //                    employeePermit.EnvironmentID = permit.EnvironmentID;
        //                    employeePermit.IsActive = permit.IsActive;
        //                    employeePermit.IsPaidTo = permittype.IsPaidTo;
        //                    employeePermit.LocationID = permit.LocationID;
        //                    double minuteDuration = (int)(permit.DateEnd - permit.DateBegin).TotalMinutes; // tabloda computed alan
        //                    employeePermit.OurCompanyID = location.OurCompanyID;
        //                    employeePermit.PermitTypeID = permit.PermitTypeID;
        //                    employeePermit.QuantityHour = (double)(minuteDuration / (double)60);
        //                    employeePermit.RecordDate = location.LocalDateTime;
        //                    employeePermit.RecordEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                    employeePermit.RecordIP = OfficeHelper.GetIPAddress();
        //                    employeePermit.ReferenceID = permit.ReferanceID ?? null;
        //                    employeePermit.ResultID = permit.ResultID ?? null;
        //                    employeePermit.ReturnWorkDate = permit.ReturnWorkDate;
        //                    employeePermit.StatusID = permit.StatusID;
        //                    employeePermit.UID = permit.UID;
        //                    employeePermit.UnitPrice = unithour;

        //                    if (permit.PermitTypeID == 2)
        //                    {
        //                        employeePermit.TotalAmount = employeePermit.IsPaidTo == false ? (employeePermit.UnitPrice * employeePermit.QuantityHour) : 0;
        //                    }
        //                    else
        //                    {
        //                        employeePermit.TotalAmount = 0;
        //                    }

        //                    Db.DocumentEmployeePermit.Add(employeePermit);
        //                    Db.SaveChanges();

        //                    // eğer status 1 ise ve izin saatlik ücretsiz kesintili ise carisine kesinti işlenir.
        //                    if (employeePermit.StatusID == 1 && employeePermit.IsPaidTo == false && employeePermit.TotalAmount > 0 && permit.PermitTypeID == 2)
        //                    {
        //                        OfficeHelper.AddEmployeeAction(employeePermit.EmployeeID, employeePermit.LocationID, employeePermit.ActionTypeID, employeePermit.ActionTypeName, employeePermit.ID, employeePermit.Date, employeePermit.Description, 1, 0, employeePermit.TotalAmount, employeePermit.Currency, null, null, 3, employeePermit.RecordEmployeeID, employeePermit.RecordDate, employeePermit.UID.Value, employeePermit.DocumentNumber, 16);
        //                    }

        //                    result.IsSuccess = true;
        //                    result.Message = "Çalışan izini başarı ile eklendi";

        //                    // log atılır
        //                    OfficeHelper.AddApplicationLog("Office", "Permit", "Insert", employeePermit.ID.ToString(), "Salary", "Permit", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, employeePermit);
        //                }
        //                else
        //                {
        //                    result.Message = $"Çalışanın izini başka bir mevcut izini ile çakıştı. Lütfen kontrol ediniz.";
        //                    OfficeHelper.AddApplicationLog("Office", "Permit", "Insert", "-1", "Salary", "Permit", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(permit.TimeZone), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, permit);

        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                result.Message = $"Çalışan izini eklenemedi : {ex.Message}";
        //                OfficeHelper.AddApplicationLog("Office", "Permit", "Insert", "-1", "Salary", "Permit", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(permit.TimeZone), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, permit);
        //            }

        //        }
        //    }

        //    return result;
        //}

        #endregion
        #region EditExam
        //public Result<DocumentEmployeePermit> EditEmployeePermit(EmployeePermit permit, AuthenticationModel authentication)
        //{
        //    Result<DocumentEmployeePermit> result = new Result<DocumentEmployeePermit>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty,
        //        Data = null
        //    };

        //    using (ActionTimeEntities Db = new ActionTimeEntities())
        //    {
        //        var isPermit = Db.DocumentEmployeePermit.FirstOrDefault(x => x.ID == permit.ID && x.UID == permit.UID);


        //        if (permit != null && authentication != null && isPermit != null)
        //        {
        //            try
        //            {

        //                DocumentEmployeePermit self = new DocumentEmployeePermit()
        //                {
        //                    ActionTypeID = isPermit.ActionTypeID,
        //                    ActionTypeName = isPermit.ActionTypeName,
        //                    Currency = isPermit.Currency,
        //                    Date = isPermit.Date,
        //                    DateBegin = isPermit.DateBegin,
        //                    DateEnd = isPermit.DateEnd,
        //                    StatusID = isPermit.StatusID,
        //                    Description = isPermit.Description,
        //                    DocumentNumber = isPermit.DocumentNumber,
        //                    EmployeeID = isPermit.EmployeeID,
        //                    EnvironmentID = isPermit.EnvironmentID,
        //                    ID = isPermit.ID,
        //                    IsActive = isPermit.IsActive,
        //                    IsPaidTo = isPermit.IsPaidTo,
        //                    LocationID = isPermit.LocationID,
        //                    MinuteDuration = isPermit.MinuteDuration,
        //                    OurCompanyID = isPermit.OurCompanyID,
        //                    PermitTypeID = isPermit.PermitTypeID,
        //                    QuantityHour = isPermit.QuantityHour,
        //                    RecordDate = isPermit.RecordDate,
        //                    RecordEmployeeID = isPermit.RecordEmployeeID,
        //                    RecordIP = isPermit.RecordIP,
        //                    ReferenceID = isPermit.ReferenceID,
        //                    ResultID = isPermit.ResultID,
        //                    ReturnWorkDate = isPermit.ReturnWorkDate,
        //                    TotalAmount = isPermit.TotalAmount,
        //                    UID = isPermit.UID,
        //                    UnitPrice = isPermit.UnitPrice,
        //                    UpdateDate = isPermit.UpdateDate,
        //                    UpdateEmployeeID = isPermit.UpdateEmployeeID,
        //                    UpdateIP = isPermit.UpdateIP
        //                };

        //                var location = Db.Location.FirstOrDefault(x => x.LocationID == permit.LocationID);
        //                var permittype = Db.PermitType.FirstOrDefault(x => x.ID == permit.PermitTypeID);
        //                var locationstats = Db.LocationStats.FirstOrDefault(x => x.LocationID == permit.LocationID && x.StatsID == 2 && x.OptionID == 3);

        //                var empunits = Db.EmployeeSalary.Where(x => x.EmployeeID == permit.EmployeeID && x.DateStart <= permit.Date && x.Hourly > 0).OrderByDescending(x => x.DateStart).FirstOrDefault();

        //                double unithour = empunits?.Hourly ?? 0;

        //                if (location.OurCompanyID == 1 && locationstats != null)
        //                {
        //                    unithour = unithour + 1;
        //                }


        //                //var isotherpermit = Db.DocumentEmployeePermit.FirstOrDefault(x => x.Date == isPermit.Date && x.UID != isPermit.UID && x.ID != isPermit.ID && (x.DateBegin >= isPermit.DateBegin || x.DateEnd <= isPermit.DateEnd));
        //                var isotherpermit = Db.DocumentEmployeePermit.FirstOrDefault(x => x.ID != permit.ID && x.UID != permit.UID && x.Date == permit.Date && x.EmployeeID == permit.EmployeeID && x.IsActive == true && (x.DateBegin >= permit.DateBegin || x.DateEnd <= permit.DateEnd));

        //                if (isotherpermit == null)
        //                {

        //                    double minuteDuration = OfficeHelper.CalculatePermitDuration(permit.DateBegin, permit.DateEnd, permit.EmployeeID, permit.LocationID);// (int)(permit.DateEnd - permit.DateBegin).TotalMinutes;

        //                    isPermit.LocationID = permit.LocationID;
        //                    isPermit.EmployeeID = permit.EmployeeID;
        //                    isPermit.Date = permit.Date;
        //                    isPermit.DateBegin = permit.DateBegin;
        //                    isPermit.DateEnd = permit.DateEnd;
        //                    isPermit.Description = permit.Description;
        //                    isPermit.StatusID = permit.StatusID;
        //                    isPermit.PermitTypeID = permit.PermitTypeID;
        //                    isPermit.IsPaidTo = permittype.IsPaidTo;
        //                    isPermit.IsActive = permit.IsActive;
        //                    isPermit.ReturnWorkDate = permit.ReturnWorkDate;
        //                    isPermit.QuantityHour = (double)(minuteDuration / (double)60);
        //                    isPermit.UpdateDate = location.LocalDateTime;
        //                    isPermit.UpdateEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                    isPermit.UpdateIP = OfficeHelper.GetIPAddress();
        //                    isPermit.UnitPrice = unithour;

        //                    if (permit.PermitTypeID == 2)
        //                    {
        //                        isPermit.TotalAmount = isPermit.IsPaidTo == false ? (isPermit.UnitPrice * isPermit.QuantityHour) : 0;
        //                    }
        //                    else
        //                    {
        //                        isPermit.TotalAmount = 0;
        //                    }

        //                    Db.SaveChanges();

        //                    // önce cari hesap kaydı varsa silinir.
        //                    var iscari = Db.EmployeeCashActions.FirstOrDefault(x => x.ProcessID == isPermit.ID && x.ProcessUID == isPermit.UID);
        //                    if (iscari != null)
        //                    {
        //                        Db.EmployeeCashActions.Remove(iscari);
        //                        Db.SaveChanges();
        //                    }

        //                    // eğer status 1 ise ve izin saatlik ve ücretsiz kesintili ise carisine kesinti işlenir.
        //                    if (isPermit.StatusID == 1 && isPermit.IsPaidTo == false && isPermit.TotalAmount > 0 && permit.PermitTypeID == 2)
        //                    {
        //                        OfficeHelper.AddEmployeeAction(isPermit.EmployeeID, isPermit.LocationID, isPermit.ActionTypeID, isPermit.ActionTypeName, isPermit.ID, isPermit.Date, isPermit.Description, 1, 0, isPermit.TotalAmount, isPermit.Currency, null, null, 3, isPermit.UpdateEmployeeID, isPermit.UpdateDate, isPermit.UID.Value, isPermit.DocumentNumber, 16);
        //                    }

        //                    if (isPermit.StatusID == 2)
        //                    {
        //                        isPermit.IsActive = false;
        //                        Db.SaveChanges();
        //                    }


        //                    result.IsSuccess = true;
        //                    result.Message = $"{isPermit.DocumentNumber} nolu Çalışan İzni başarı ile güncellendi";

        //                    // log atılır
        //                    var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentEmployeePermit>(self, isPermit, OfficeHelper.getIgnorelist());
        //                    OfficeHelper.AddApplicationLog("Office", "Permit", "Update", isPermit.ID.ToString(), "Salary", "Permit", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
        //                }
        //                else
        //                {
        //                    result.Message = $"Çalışan izini mevcut başka izini ile çakıştı.";
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                result.Message = $"Çalışan izini güncellenemedi : {ex.Message}";
        //                OfficeHelper.AddApplicationLog("Office", "Permit", "Update", isPermit.ID.ToString(), "Salary", "Permit", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(permit.TimeZone), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, permit);
        //            }


        //        }
        //    }
        //    return result;
        //}
        #endregion
        #endregion



    }
}