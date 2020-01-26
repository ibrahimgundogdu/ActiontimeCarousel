using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        public ActionResult Compute(int? WeekYear, int? WeekNumber, int? LocationID)
        {
            RevenueControlModel model = new RevenueControlModel();

            model.WeekYear = WeekYear ?? 0;
            model.WeekNumber = WeekNumber ?? 0;

            if (WeekYear > 0 && WeekNumber > 0)
            {
                if (LocationID > 0)
                {
                    var res = Db.ComputeLocationWeekRevenue(model.WeekNumber, model.WeekYear, LocationID);
                }
                else
                {
                    model.Locations = Db.Location.Where(x => x.LocationTypeID != 5 && x.LocationTypeID != 6).ToList();

                    foreach (var location in model.Locations)
                    {
                        var res = Db.ComputeLocationWeekRevenue(model.WeekNumber, model.WeekYear, location.LocationID);
                    }
                }
            }

            model.Revenues = Db.VRevenue.Where(x => x.WeekYear == model.WeekYear && x.WeekNumber == model.WeekNumber).ToList();

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
                model.Locations = Db.Location.Where(x=> x.LocationTypeID != 5 && x.LocationTypeID != 6).ToList();

                foreach (var location in model.Locations)
                {
                    var res = Db.ComputeLocationWeekRevenue(WeekNumber, WeekYear, location.LocationID);
                }
            }

            return RedirectToAction("Index", "Revenue", new { WeekYear, WeekNumber });
        }

        public ActionResult Parameters()
        {
            RevenueControlModel model = new RevenueControlModel();

            model.Locations = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.LocationParameters = Db.LocationParam.ToList();
            model.RevenueParameters = Db.RevenueParameter.ToList();
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


    }
}