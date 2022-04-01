using ActionForce.Entity;
using ClosedXML.Excel;
using LinqToExcel;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class RevenueController : BaseController
    {

        public ActionResult Index(int? WeekYear, int? WeekNumber, int? LocationID, DateTime? date, int? isactive)
        {
            RevenueControlModel model = new RevenueControlModel();

            if (model.Authentication.ActionEmployee.RoleGroup.RoleLevel < 5)
            {
                return RedirectToAction("Index", "Home");
            }

            var currentdate = DateTime.Now.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var selecteddate = DateTime.Now.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            if (date != null)
            {
                selecteddate = date.Value.Date;
            }

            DateList cdatelist = Db.DateList.FirstOrDefault(x => x.DateKey == currentdate);
            DateList sdatelist = Db.DateList.FirstOrDefault(x => x.DateKey == selecteddate);
            model.CurrentDate = cdatelist;
            model.SelectedDate = sdatelist;

            model.WeekNumber = WeekNumber ?? sdatelist.WeekNumber.Value;
            model.WeekYear = WeekYear ?? sdatelist.WeekYear.Value;

            var weekdatekeys = Db.DateList.Where(x => x.WeekYear == model.WeekYear && x.WeekNumber == model.WeekNumber).ToList();
            model.WeekList = weekdatekeys;
            model.FirstWeekDay = weekdatekeys.OrderBy(x => x.DateKey).FirstOrDefault();
            model.LastWeekDay = weekdatekeys.OrderByDescending(x => x.DateKey).FirstOrDefault();

            var prevdate = model.FirstWeekDay.DateKey.AddDays(-1).Date;
            var nextdate = model.LastWeekDay.DateKey.AddDays(1).Date;

            model.PrevWeek = Db.DateList.FirstOrDefault(x => x.DateKey == prevdate);
            model.NextWeek = Db.DateList.FirstOrDefault(x => x.DateKey == nextdate);

            //model.Locations = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderBy(x=> x.SortBy).ToList();

            model.Revenues = Db.VRevenue.Where(x => x.WeekYear == model.WeekYear && x.WeekNumber == model.WeekNumber && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            bool? active = isactive == 1 ? true : isactive == 0 ? false : (bool?)null;

            if (active != null)
            {
                model.Revenues = model.Revenues.Where(x => x.IsActive == active).ToList();
            }

            List<long> revids = model.Revenues.Select(x => x.ID).Distinct().ToList();

            model.RevenueLines = Db.VRevenueLines.Where(x => x.WeekYear == model.WeekYear && x.WeekNumber == model.WeekNumber && revids.Contains(x.RevenueID.Value)).ToList();



            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Compute(int? WeekYear, int? WeekNumber, int? LocationID, int? WeekNumberBegin)
        {
            RevenueComputeFilterModel model = new RevenueComputeFilterModel();

            model.WeekYear = WeekYear;
            model.WeekNumber = WeekNumber;
            model.LocationID = LocationID;
            model.WeekNumberBegin = WeekNumberBegin;

            if (WeekYear > 0 && WeekNumber > 0)
            {
                if (LocationID > 0)
                {
                    var res = Db.ComputeLocationWeekRevenue(model.WeekNumber, model.WeekYear, LocationID);
                }
                else
                {
                    var locations = Db.Location.Where(x => x.LocationTypeID != 5 && x.LocationTypeID != 6).ToList();

                    foreach (var location in locations)
                    {
                        var res = Db.ComputeLocationWeekRevenue(model.WeekNumber, model.WeekYear, location.LocationID);
                    }
                }
            }


            if (WeekYear > 0 && WeekNumberBegin > 0)
            {
                List<int> locations = Db.Location.Where(x => x.LocationTypeID != 5 && x.LocationTypeID != 6).Select(x => x.LocationID).Distinct().ToList();
                List<int> weeks = Db.DateList.Where(x => x.WeekYear == WeekYear && x.WeekNumber >= WeekNumberBegin).Select(x => x.WeekNumber.Value).Distinct().OrderBy(x => x).ToList();

                foreach (var week in weeks)
                {
                    foreach (var location in locations)
                    {
                        var res = Db.ComputeLocationWeekRevenue(week, model.WeekYear, location);
                    }
                }
            }

            if (LocationID > 0 && WeekYear == null && WeekNumber == null) // lokasyonun tüm yılları hesaplanır
            {
                for (int i = 2017; i < DateTime.Now.Year + 1; i++)
                {
                    List<int> weeks = Db.DateList.Where(x => x.WeekYear == i).Select(x => x.WeekNumber.Value).Distinct().OrderBy(x => x).ToList();

                    foreach (var week in weeks)
                    {
                        var res = Db.ComputeLocationWeekRevenue(week, i, LocationID);
                    }
                }
            }

            if (LocationID > 0 && WeekYear > 0 && WeekNumber == null)
            {
                List<int> weeks = Db.DateList.Where(x => x.WeekYear == WeekYear).Select(x => x.WeekNumber.Value).Distinct().OrderBy(x => x).ToList();

                foreach (var week in weeks)
                {
                    var res = Db.ComputeLocationWeekRevenue(week, WeekYear, LocationID);
                }
            }

            return View(model);
        }

        public PartialViewResult RecalculateRevenue(int WeekYear, int WeekNumber, int LocationID)
        {
            RevenueDetailModel model = new RevenueDetailModel();

            var res = Db.ComputeLocationWeekRevenue(WeekNumber, WeekYear, LocationID);

            model.Revenue = Db.VRevenue.FirstOrDefault(x => x.WeekYear == WeekYear && x.WeekNumber == WeekNumber && x.LocationID == LocationID);
            model.RevenueLines = Db.VRevenueLines.Where(x => x.WeekYear == WeekYear && x.WeekNumber == WeekNumber && x.LocationID == LocationID).ToList();

            return PartialView("_PartialRevenueDetail", model);
        }

        public ActionResult RecalculateRevenueAll(int? WeekYear, int? WeekNumber)
        {
            RevenueControlModel model = new RevenueControlModel();

            if (WeekYear > 0 && WeekNumber > 0)
            {
                model.Locations = Db.Location.Where(x => x.LocationTypeID != 5 && x.LocationTypeID != 6).ToList();

                foreach (var location in model.Locations)
                {
                    var res = Db.ComputeLocationWeekRevenue(WeekNumber, WeekYear, location.LocationID);
                }
            }

            return RedirectToAction("Index", "Revenue", new { WeekYear, WeekNumber });
        }

        public ActionResult Parameters(int? isactive)
        {
            RevenueControlModel model = new RevenueControlModel();

            model.Locations = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            List<int> locationids = model.Locations.Select(x => x.LocationID).ToList();

            model.Counters = new CounterModel() { CountActive = model.Locations.Where(x => x.IsActive == true)?.Count() ?? 0, CountPassive = model.Locations.Where(x => x.IsActive == false)?.Count() ?? 0, CountAll = model.Locations?.Count() ?? 0 };

            bool? active = isactive == 1 ? true : isactive == 0 ? false : (bool?)null;

            if (active != null)
            {
                model.Locations = model.Locations.Where(x => x.IsActive == active).ToList();
            }

            model.LocationParameters = Db.LocationParam.Where(x => locationids.Contains(x.LocationID)).ToList();
            model.RevenueParameters = Db.RevenueParameter.Where(x => locationids.Contains(x.LocationID)).ToList();
            model.PeriodParameters = Db.LocationPeriods.Where(x => locationids.Contains(x.LocationID.Value)).ToList();
            model.ParameterTypes = Db.ActionType.Where(x => x.IsActive == true && x.IsParam == true);
            model.LocationParamCalculate = Db.LocationParamCalculate.ToList();

            return View(model);
        }

        [HttpPost]
        public PartialViewResult AddLocationParameter(int locationid, string startdate, string total, int typeid, string rate, string calctype)
        {
            LocationParameterDetailModel model = new LocationParameterDetailModel();

            DateTime? startDate = !string.IsNullOrEmpty(startdate) ? Convert.ToDateTime(startdate) : DateTime.Now.Date;
            var location = Db.Location.FirstOrDefault(x => x.LocationID == locationid);
            var paramtype = Db.ActionType.FirstOrDefault(x => x.TypeID == typeid);
            var totalAmount = !string.IsNullOrEmpty(total) ? Convert.ToDouble(total.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture) : (double?)null;
            var rateAmount = !string.IsNullOrEmpty(rate) ? Convert.ToDouble(rate.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture) : (double?)null;

            if (location != null)
            {
                LocationParam param = new LocationParam();
                param.Calculate = !string.IsNullOrEmpty(calctype) ? Convert.ToInt32(calctype) : (int?)null;
                param.DateStart = startDate;
                param.LocationID = locationid;
                param.Money = location.Currency;
                param.Rate = rateAmount;
                param.RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
                param.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                param.RecordIP = OfficeHelper.GetIPAddress();
                param.Total = totalAmount;
                param.TypeID = typeid;
                param.TypeName = paramtype.TypeName;

                Db.LocationParam.Add(param);
                Db.SaveChanges();

                OfficeHelper.AddApplicationLog("Office", "LocationParameter", "Insert", param.ID.ToString(), "Revenue", "AddLocationParameter", null, true, $"Lokasyon Parametresi Eklendi", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, param);

            }

            model.Location = Db.Location.FirstOrDefault(x => x.LocationID == locationid);
            model.LocationParameters = Db.LocationParam.Where(x => x.LocationID == locationid).ToList();
            model.LocationParamCalculate = Db.LocationParamCalculate.ToList();

            return PartialView("_PartialLocationParameterDetail", model);
        }

        [HttpPost]
        public PartialViewResult EditLocationParameter(int id, string startdate, string total, string rate, string calctype)
        {
            LocationParameterDetailModel model = new LocationParameterDetailModel();

            var locationParameter = Db.LocationParam.FirstOrDefault(x => x.ID == id);
            DateTime? startDate = !string.IsNullOrEmpty(startdate) ? Convert.ToDateTime(startdate) : DateTime.Now.Date;
            var location = Db.Location.FirstOrDefault(x => x.LocationID == locationParameter.LocationID);
            var totalAmount = !string.IsNullOrEmpty(total) ? Convert.ToDouble(total.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture) : (double?)null;
            var rateAmount = !string.IsNullOrEmpty(rate) ? Convert.ToDouble(rate.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture) : (double?)null;

            if (locationParameter != null && location != null)
            {

                LocationParam self = new LocationParam()
                {
                    Calculate = locationParameter.Calculate,
                    DateFinish = locationParameter.DateFinish,
                    DateStart = locationParameter.DateStart,
                    ID = locationParameter.ID,
                    LocationID = locationParameter.LocationID,
                    Money = locationParameter.Money,
                    Rate = locationParameter.Rate,
                    RecordDate = locationParameter.RecordDate,
                    RecordEmployeeID = locationParameter.RecordEmployeeID,
                    RecordIP = locationParameter.RecordIP,
                    Total = locationParameter.Total,
                    TypeID = locationParameter.TypeID,
                    TypeName = locationParameter.TypeName,
                    UpdateDate = locationParameter.UpdateDate,
                    UpdateEmployeeID = locationParameter.UpdateEmployeeID,
                    UpdateIP = locationParameter.UpdateIP
                };

                locationParameter.Calculate = !string.IsNullOrEmpty(calctype) ? Convert.ToInt32(calctype) : (int?)null;
                locationParameter.DateStart = startDate;
                locationParameter.Rate = rateAmount;
                locationParameter.Total = totalAmount;
                locationParameter.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
                locationParameter.UpdateEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                locationParameter.UpdateIP = OfficeHelper.GetIPAddress();

                Db.SaveChanges();

                var isequal = OfficeHelper.PublicInstancePropertiesEqual<LocationParam>(self, locationParameter, OfficeHelper.getIgnorelist());
                OfficeHelper.AddApplicationLog("Office", "LocationParameter", "Update", locationParameter.ID.ToString(), "Revenue", "EditLocationParameter", isequal, true, $"{locationParameter.ID} ID li Lokasyon Parametresi Güncellendi", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
            }

            model.Location = Db.Location.FirstOrDefault(x => x.LocationID == location.LocationID);
            model.LocationParameters = Db.LocationParam.Where(x => x.LocationID == location.LocationID).ToList();
            model.LocationParamCalculate = Db.LocationParamCalculate.ToList();

            return PartialView("_PartialLocationParameterDetail", model);
        }

        [HttpPost]
        public PartialViewResult DeleteLocationParameter(int id)
        {
            LocationParameterDetailModel model = new LocationParameterDetailModel();

            var locationParameter = Db.LocationParam.FirstOrDefault(x => x.ID == id);
            var location = Db.Location.FirstOrDefault(x => x.LocationID == locationParameter.LocationID);

            if (locationParameter != null && location != null)
            {
                OfficeHelper.AddApplicationLog("Office", "LocationParameter", "Delete", id.ToString(), "Revenue", "DeleteLocationParameter", null, true, $"Lokasyon Parametresi Silindi", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, locationParameter);

                Db.LocationParam.Remove(locationParameter);
                Db.SaveChanges();
            }

            model.Location = Db.Location.FirstOrDefault(x => x.LocationID == location.LocationID);
            model.LocationParameters = Db.LocationParam.Where(x => x.LocationID == location.LocationID).ToList();
            model.LocationParamCalculate = Db.LocationParamCalculate.ToList();

            return PartialView("_PartialLocationParameterDetail", model);
        }

        [HttpPost]
        public PartialViewResult AddRevenueParameter(int locationid, string startdate, string cash, string credit, string rent)
        {
            RevenueParameterDetailModel model = new RevenueParameterDetailModel();

            DateTime startDate = !string.IsNullOrEmpty(startdate) ? Convert.ToDateTime(startdate) : DateTime.Now.Date;
            var location = Db.Location.FirstOrDefault(x => x.LocationID == locationid);
            bool iscash = !string.IsNullOrEmpty(cash) && cash == "1" ? true : false;
            bool iscredit = !string.IsNullOrEmpty(credit) && credit == "1" ? true : false;
            bool isrent = !string.IsNullOrEmpty(rent) && rent == "1" ? true : false;

            if (location != null)
            {
                RevenueParameter param = new RevenueParameter();
                param.DateBegin = startDate;
                param.LocationID = locationid;
                param.RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
                param.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                param.RecordIP = OfficeHelper.GetIPAddress();
                param.IsCash = iscash;
                param.IsCredit = iscredit;
                param.IsRent = isrent;

                Db.RevenueParameter.Add(param);
                Db.SaveChanges();

                OfficeHelper.AddApplicationLog("Office", "RevenueParameter", "Insert", param.ID.ToString(), "Revenue", "AddRevenueParameter", null, true, $"Hasılat Parametresi Eklendi", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, param);

            }

            model.Location = Db.Location.FirstOrDefault(x => x.LocationID == locationid);
            model.RevenueParameters = Db.RevenueParameter.Where(x => x.LocationID == locationid).ToList();

            return PartialView("_PartialRevenueParameterDetail", model);
        }

        [HttpPost]
        public PartialViewResult EditRevenueParameter(int id, string startdate, string cash, string credit, string rent)
        {
            RevenueParameterDetailModel model = new RevenueParameterDetailModel();

            var revenueParameter = Db.RevenueParameter.FirstOrDefault(x => x.ID == id);

            DateTime startDate = !string.IsNullOrEmpty(startdate) ? Convert.ToDateTime(startdate) : DateTime.Now.Date;
            var location = Db.Location.FirstOrDefault(x => x.LocationID == revenueParameter.LocationID);
            bool iscash = !string.IsNullOrEmpty(cash) && cash == "1" ? true : false;
            bool iscredit = !string.IsNullOrEmpty(credit) && credit == "1" ? true : false;
            bool isrent = !string.IsNullOrEmpty(rent) && rent == "1" ? true : false;

            if (revenueParameter != null && location != null)
            {

                RevenueParameter self = new RevenueParameter()
                {
                    DateBegin = revenueParameter.DateBegin,
                    ID = revenueParameter.ID,
                    LocationID = revenueParameter.LocationID,
                    IsRent = revenueParameter.IsRent,
                    IsCash = revenueParameter.IsCash,
                    IsCredit = revenueParameter.IsCredit,
                    RecordDate = revenueParameter.RecordDate,
                    RecordEmployeeID = revenueParameter.RecordEmployeeID,
                    RecordIP = revenueParameter.RecordIP,
                    UpdateDate = revenueParameter.UpdateDate,
                    UpdateEmployeeID = revenueParameter.UpdateEmployeeID,
                    UpdateIP = revenueParameter.UpdateIP
                };


                revenueParameter.DateBegin = startDate;
                revenueParameter.IsCash = iscash;
                revenueParameter.IsCredit = iscredit;
                revenueParameter.IsRent = isrent;
                revenueParameter.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
                revenueParameter.UpdateEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                revenueParameter.UpdateIP = OfficeHelper.GetIPAddress();


                Db.SaveChanges();

                var isequal = OfficeHelper.PublicInstancePropertiesEqual<RevenueParameter>(self, revenueParameter, OfficeHelper.getIgnorelist());
                OfficeHelper.AddApplicationLog("Office", "RevenueParameter", "Update", revenueParameter.ID.ToString(), "Revenue", "EditRevenueParameter", isequal, true, $"{revenueParameter.ID} ID li Hasılat Parametresi Güncellendi", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
            }

            model.Location = Db.Location.FirstOrDefault(x => x.LocationID == location.LocationID);
            model.RevenueParameters = Db.RevenueParameter.Where(x => x.LocationID == location.LocationID).ToList();

            return PartialView("_PartialRevenueParameterDetail", model);
        }

        [HttpPost]
        public PartialViewResult DeleteRevenueParameter(int id)
        {
            RevenueParameterDetailModel model = new RevenueParameterDetailModel();

            var revenueParameter = Db.RevenueParameter.FirstOrDefault(x => x.ID == id);
            var location = Db.Location.FirstOrDefault(x => x.LocationID == revenueParameter.LocationID);

            if (revenueParameter != null && location != null)
            {
                OfficeHelper.AddApplicationLog("Office", "RevenueParameter", "Delete", id.ToString(), "Revenue", "DeleteRevenueParameter", null, true, $"Hasılat Parametresi Silindi", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, revenueParameter);

                Db.RevenueParameter.Remove(revenueParameter);
                Db.SaveChanges();
            }

            model.Location = Db.Location.FirstOrDefault(x => x.LocationID == location.LocationID);
            model.RevenueParameters = Db.RevenueParameter.Where(x => x.LocationID == location.LocationID).ToList();

            return PartialView("_PartialRevenueParameterDetail", model);
        }

        [HttpPost]
        public PartialViewResult AddPeriodParameter(int locationid, string startdate, string enddate, string description)
        {
            PeriodParameterDetailModel model = new PeriodParameterDetailModel();
            var location = Db.Location.FirstOrDefault(x => x.LocationID == locationid);

            if (!string.IsNullOrEmpty(startdate))
            {
                DateTime startDate = Convert.ToDateTime(startdate);
                DateTime? endDate = !string.IsNullOrEmpty(enddate) ? Convert.ToDateTime(enddate) : (DateTime?)null;

                if (location != null)
                {

                    LocationPeriods param = new LocationPeriods();
                    param.ContractStartDate = startDate;
                    param.ContractFinishDate = endDate;
                    param.LocationID = locationid;
                    param.RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
                    param.RecordedEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    param.RecordIP = OfficeHelper.GetIPAddress();
                    param.Description = description;
                    param.OurCompanyID = location.OurCompanyID;

                    Db.LocationPeriods.Add(param);
                    Db.SaveChanges();

                    OfficeHelper.AddApplicationLog("Office", "LocationPeriods", "Insert", param.ID.ToString(), "Revenue", "AddPeriodParameter", null, true, $"Lokasyon Periyot Parametresi Eklendi", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, param);

                }
            }

            model.Location = location;
            model.PeriodParameters = Db.LocationPeriods.Where(x => x.LocationID == locationid).ToList();

            return PartialView("_PartialPeriodParameterDetail", model);
        }

        [HttpPost]
        public PartialViewResult EditPeriodParameter(int id, string startdate, string enddate, string description)
        {
            PeriodParameterDetailModel model = new PeriodParameterDetailModel();

            var periodParameter = Db.LocationPeriods.FirstOrDefault(x => x.ID == id);

            if (!string.IsNullOrEmpty(startdate))
            {

                DateTime startDate = Convert.ToDateTime(startdate);
                DateTime? endDate = !string.IsNullOrEmpty(enddate) ? Convert.ToDateTime(enddate) : (DateTime?)null;

                var location = Db.Location.FirstOrDefault(x => x.LocationID == periodParameter.LocationID);

                if (periodParameter != null && location != null)
                {

                    LocationPeriods self = new LocationPeriods()
                    {
                        ID = periodParameter.ID,
                        FinalFinishDate = periodParameter.FinalFinishDate,
                        Description = periodParameter.Description,
                        ContractStartDate = periodParameter.ContractStartDate,
                        ContractFinishDate = periodParameter.ContractFinishDate,
                        LocationID = periodParameter.LocationID,
                        OurCompanyID = periodParameter.OurCompanyID,
                        RecordDate = periodParameter.RecordDate,
                        RecordedEmployeeID = periodParameter.RecordedEmployeeID,
                        UpdateEmployeeID = periodParameter.UpdateEmployeeID,
                        RecordIP = periodParameter.RecordIP,
                        UpdateDate = periodParameter.UpdateDate,
                        UpdateIP = periodParameter.UpdateIP
                    };

                    periodParameter.ContractStartDate = startDate;
                    periodParameter.ContractFinishDate = endDate;
                    periodParameter.Description = description;
                    periodParameter.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
                    periodParameter.UpdateEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    periodParameter.UpdateIP = OfficeHelper.GetIPAddress();


                    Db.SaveChanges();

                    var isequal = OfficeHelper.PublicInstancePropertiesEqual<LocationPeriods>(self, periodParameter, OfficeHelper.getIgnorelist());
                    OfficeHelper.AddApplicationLog("Office", "LocationPeriods", "Update", periodParameter.ID.ToString(), "Revenue", "EditPeriodParameter", isequal, true, $"{periodParameter.ID} ID li Lokasyon Periyodu Güncellendi", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                }

                model.Location = location;
            }

            model.PeriodParameters = Db.LocationPeriods.Where(x => x.LocationID == model.Location.LocationID).ToList();

            return PartialView("_PartialPeriodParameterDetail", model);
        }

        [HttpPost]
        public PartialViewResult DeletePeriodParameter(int id)
        {
            PeriodParameterDetailModel model = new PeriodParameterDetailModel();

            var periodParameter = Db.LocationPeriods.FirstOrDefault(x => x.ID == id);
            var location = Db.Location.FirstOrDefault(x => x.LocationID == periodParameter.LocationID);

            if (periodParameter != null && location != null)
            {
                OfficeHelper.AddApplicationLog("Office", "LocationPeriods", "Delete", id.ToString(), "Revenue", "DeletePeriodParameter", null, true, $"Lokasyon Periyodu Silindi", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, periodParameter);

                Db.LocationPeriods.Remove(periodParameter);
                Db.SaveChanges();
            }

            model.Location = location;
            model.PeriodParameters = Db.LocationPeriods.Where(x => x.LocationID == model.Location.LocationID).ToList();

            return PartialView("_PartialPeriodParameterDetail", model);
        }

        //ImportParameter
        [AllowAnonymous]
        public FileResult GetParameterTemplate()
        {
            RevenueControlModel model = new RevenueControlModel();

            var locationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).ToList();
            var locationParam = Db.LocationParam.Where(x => x.TypeID == 5).OrderByDescending(x => x.ID).Take(10).ToList();

            if (locationParam != null && locationParam.Count > 0)
            {
                string targetpath = Server.MapPath("~/Document/Salary/");
                string FileName = $"LocationParameter.xlsx";

                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        // Kiralar Kısmı

                        var worksheet = workbook.Worksheets.Add("Kiralar");

                        worksheet.Cell("A1").Value = "LocationID";
                        worksheet.Cell("B1").Value = "Kodu";
                        worksheet.Cell("C1").Value = "Adi";
                        worksheet.Cell("D1").Value = "Turu";
                        worksheet.Cell("E1").Value = "Baslangic";
                        worksheet.Cell("F1").Value = "Bitis";
                        worksheet.Cell("G1").Value = "Tutar";
                        worksheet.Cell("H1").Value = "Birim";


                        //worksheet.Cell("A2").FormulaA1 = "=MID(A1, 7, 5)";

                        int rownum = 2;

                        foreach (var item in locationList)
                        {

                            worksheet.Cell("A" + rownum).Value = item.LocationID;
                            worksheet.Cell("B" + rownum).Value = item.SortBy;
                            worksheet.Cell("C" + rownum).Value = item.LocationFullName;
                            worksheet.Cell("D" + rownum).Value = item.Description;
                            worksheet.Cell("E" + rownum).Value = DateTime.Now.Date;
                            worksheet.Cell("F" + rownum).Value = null;
                            worksheet.Cell("G" + rownum).Value = 0.0;
                            worksheet.Cell("H" + rownum).Value = "TRL";

                            rownum++;
                        }

                        string pathToExcelFile = targetpath + FileName;
                        workbook.SaveAs(pathToExcelFile);

                        return File(pathToExcelFile, "application/vnd.ms-excel", FileName);
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }


        }

        [AllowAnonymous]
        public ActionResult ImportParameter()
        {
            RevenueControlModel model = new RevenueControlModel();
            model.Result = new Result();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result;
            }

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ImportParameterFile(FormRentImport form)
        {
            RevenueControlModel model = new RevenueControlModel();
            model.Result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty,
                InfoKeyList = new List<InfoKey>()
            };

            if (form == null)
            {
                return RedirectToAction("Parameters");
            }

            var datalist = new List<ExcelParameterRent>();
            List<LocationParam> paramList = new List<LocationParam>();

            if (form.ParameterFile != null && form.ParameterFile.ContentLength > 0)
            {

                if (form.ParameterFile.ContentType == "application/vnd.ms-excel" || form.ParameterFile.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    string filename = Guid.NewGuid().ToString() + Path.GetExtension(form.ParameterFile.FileName);
                    string targetpath = Server.MapPath("~/Document/Salary/");
                    string pathToExcelFile = targetpath + filename;


                    form.ParameterFile.SaveAs(Path.Combine(targetpath, filename));



                    var connectionString = "";
                    if (filename.EndsWith(".xls"))
                    {
                        connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", pathToExcelFile);
                    }
                    else if (filename.EndsWith(".xlsx"))
                    {
                        connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", pathToExcelFile);
                    }


                    //var adapter = new OleDbDataAdapter("SELECT * FROM [SalaryPeriod$]", connectionString);
                    //var ds = new DataSet();
                    //adapter.Fill(ds, "ExcelTable");
                    //DataTable dtable = ds.Tables["ExcelTable"];


                    string sheetName = "Kiralar";
                    var excelFile = new ExcelQueryFactory(pathToExcelFile);
                    var salaryList = from a in excelFile.Worksheet<ExcelParameterRent>(sheetName) select a;
                    datalist = salaryList.ToList();

                    if (datalist.Count > 0)
                    {

                        foreach (var item in datalist)
                        {
                            var location = Db.Location.FirstOrDefault(x => x.LocationID == item.LocationID);

                            if (item.Tutar > 0)
                            {
                                var lastParam = Db.LocationParam.Where(x => x.LocationID == item.LocationID && x.TypeID == 5 && x.DateStart <= item.Baslangic).OrderByDescending(x => x.DateStart).FirstOrDefault();
                                if (lastParam != null)
                                {
                                    lastParam.DateFinish = item.Baslangic;
                                    Db.SaveChanges();
                                }

                                try
                                {

                                    if (location != null)
                                    {

                                        LocationParam newparam = new LocationParam();

                                        newparam.LocationID = item.LocationID;
                                        newparam.DateStart = item.Baslangic;
                                        newparam.DateFinish = item.Bitis;
                                        newparam.TypeID = 5;
                                        newparam.TypeName = "Rent";
                                        newparam.Total = item.Tutar;
                                        newparam.Rate = null;
                                        newparam.Money = item.Birim;
                                        newparam.Calculate = null;
                                        newparam.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                                        newparam.RecordDate = DateTime.UtcNow.AddHours(3);
                                        newparam.RecordIP = OfficeHelper.GetIPAddress();
                                        newparam.UpdateDate = null;
                                        newparam.UpdateEmployeeID = null;
                                        newparam.UpdateIP = null;

                                        paramList.Add(newparam);

                                        model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = true, Name = $"{location.LocationFullName}", Message = $"Kira Bilgisi Eklendi" });
                                    }
                                }
                                catch (DbEntityValidationException ex)
                                {
                                    foreach (var entityValidationErrors in ex.EntityValidationErrors)
                                    {
                                        foreach (var validationError in entityValidationErrors.ValidationErrors)
                                        {
                                            model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = false, Name = validationError.PropertyName, Message = validationError.ErrorMessage });
                                        }
                                    }
                                }
                            }
                            else
                            {
                                model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = false, Name = $"{location.LocationFullName}", Message = $"Kira tutarı 0 dan büyük olmalı" });
                            }
                        }

                        Db.SaveChanges();

                        Db.LocationParam.AddRange(paramList);
                        Db.SaveChanges();

                        //deleting excel file from folder
                        if ((System.IO.File.Exists(pathToExcelFile)))
                        {
                            System.IO.File.Delete(pathToExcelFile);
                        }

                    }
                    else
                    {
                        model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = false, Name = "Data Hatası", Message = "Excel Dosyasında veri yok." });
                    }
                }
                else
                {
                    model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = false, Name = "Format Hatası", Message = "Sadece Excel Dosyası Geçerlidir." });
                }
            }
            else
            {
                model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = false, Name = "Dosya Hatası", Message = "Excel Dosyası Seçin." });
            }

            model.Result.IsSuccess = false;
            model.Result.Message = $"Maaş Periyodu bilgisi bulunamadı";

            TempData["result"] = model.Result;
            OfficeHelper.AddApplicationLog("Office", "Parameter", "Import", "", "Revenue", "ImportParameter", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, datalist);

            return RedirectToAction("ImportParameter");

        }

        //Benefits
        [AllowAnonymous]
        public ActionResult Benefits(string PeriodCode, int[] LocationID)
        {
            RevenueControlModel model = new RevenueControlModel();

            if (model.Authentication.ActionEmployee.RoleGroup.RoleLevel < 5)
            {
                return RedirectToAction("Index", "Home");
            }


            if (string.IsNullOrEmpty(PeriodCode))
            {
                PeriodCode = DateTime.Now.ToString("yyyy-MM");
            }

            model.SelectedPeriod = PeriodCode;

            List<int> locationIds = Db.ExpenseChartGroupItems.Where(x => x.ChartGroupID == 1 && x.PeriodCode == PeriodCode).Select(x => x.LocationID).ToList();

            model.OfficeLocations = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && (x.LocationTypeID == 5 || x.LocationTypeID == 6)).ToList();
            model.Locations = Db.Location.Where(x => locationIds.Contains(x.LocationID)).ToList();
            model.PartnerActions = Db.VPartnerActions.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.PeriodCode == model.SelectedPeriod).ToList();
            model.ExpensePeriods = Db.ExpensePeriod.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            var expenseDocumentCharts = Db.VExpenseDocumentChart.Where(x => x.ExpensePeriodCode == PeriodCode && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            if (LocationID != null)
            {
                var discartIds = model.OfficeLocations.Where(x => !LocationID.Contains(x.LocationID) && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).Select(x => x.LocationID).ToList();

                var discartedCharts = expenseDocumentCharts.Where(x => discartIds.Contains(x.SourceExpenseCenterID.Value) && x.ExpenseGroupID == 2 && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

                model.ExpenseDocumentCharts = expenseDocumentCharts.Except<VExpenseDocumentChart>(discartedCharts).ToList();
                model.OfficeLocationIds = LocationID.ToList();
            }
            else
            {
                model.ExpenseDocumentCharts = expenseDocumentCharts;
            }

            model.ExpenseSalePartnerless = Db.VExpenseSalePartnerless.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.PeriodCode == model.SelectedPeriod).ToList();

            return View(model);
        }

    }
}