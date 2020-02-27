using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ActionForce.Entity;

namespace ActionForce.Office.Controllers
{
    public class LocationController : BaseController
    {
        // GET: Location
        [AllowAnonymous]
        public ActionResult Index()
        {
            LocationControlModel model = new LocationControlModel();

            model.LocationList = Db.VLocation.Where(x=> x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            model.StateList = model.LocationList.Select(x => x.State).Distinct().OrderBy(x => x).ToList();
            model.TypeList = model.LocationList.Select(x => x.LocationTypeName).Distinct().OrderBy(x => x).ToList();

            #region Filter
            if (TempData["LocationFilter"] != null)
            {
                model.FilterModel = TempData["LocationFilter"] as LocationFilterModel;

                if (!String.IsNullOrEmpty(model.FilterModel.LocationName))
                {
                    model.LocationList = model.LocationList.Where(x => x.LocationNameSearch.Contains(model.FilterModel.LocationName.ToUpper())).OrderBy(x => x.SortBy).ToList();
                }
                if (!String.IsNullOrEmpty(model.FilterModel.State))
                {
                    model.LocationList = model.LocationList.Where(x => x.State == model.FilterModel.State).OrderBy(x => x.SortBy).ToList();
                }
                if (!String.IsNullOrEmpty(model.FilterModel.TypeName))
                {
                    model.LocationList = model.LocationList.Where(x => x.LocationTypeName == model.FilterModel.TypeName).OrderBy(x => x.SortBy).ToList();
                }
            }
            #endregion

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult LocationFilter(LocationFilterModel getFilterModel)
        {
            TempData["LocationFilter"] = getFilterModel;

            return RedirectToAction("Index");
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult LocationSearch(LocationFilterModel getModel)
        {
            LocationControlModel model = new LocationControlModel();

            model.LocationList = Db.VLocation.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            #region Filter
            if (!String.IsNullOrEmpty(getModel.LocationName))
            {
                model.LocationList = model.LocationList.Where(x => x.LocationNameSearch.Contains(getModel.LocationName.ToUpper())).OrderBy(x => x.SortBy).ToList();
            }
            if (!String.IsNullOrEmpty(getModel.State))
            {
                model.LocationList = model.LocationList.Where(x => x.State == getModel.State).OrderBy(x => x.SortBy).ToList();
            }
            if (!String.IsNullOrEmpty(getModel.TypeName))
            {
                model.LocationList = model.LocationList.Where(x => x.LocationTypeName == getModel.TypeName).OrderBy(x => x.SortBy).ToList();
            }

            bool? isActive = getModel.IsActive == 0 ? false : getModel.IsActive == 1 ? true : (bool?)null;

            if (isActive != null)
            {
                model.LocationList = model.LocationList.Where(x => x.IsActive == isActive.Value).ToList();
            }

            #endregion

            return PartialView("_PartialLocationList", model);
        }

        [AllowAnonymous]
        public ActionResult Detail(Guid id)
        {
            LocationControlModel model = new LocationControlModel();

            if (id == Guid.Empty)
            {
                return RedirectToAction("Index");
            }

            model.LocationModel = Db.VLocation.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.LocationUID == id);

            if (model.LocationModel != null)
            {
                model.LogList = Db.ApplicationLog.Where(x => x.Modul == "Location" && x.ProcessID == model.LocationModel.LocationID.ToString()).ToList();
                model.EmployeeLocationList = Db.GetLocationEmployees(model.LocationModel.LocationID).Select(x => new LocationEmployeeModel()
                {
                    EmployeeID = x.EmployeeID,
                    EmployeeUID = x.EmployeeUID ?? Guid.Empty,
                    FullName = x.FullName,
                    Active = x.Active ?? false,
                    PositionName = x.PositionName
                }).ToList();
            }
            else
            {
                return RedirectToAction("Index", "Location");
            }

            return View(model);
        }

        [AllowAnonymous] /* Sonrasında kaldırılacak yetkiye bağlanacak. [Permision] Tablosu ve [RoleGroupPermissions]*/
        public ActionResult Edit(Guid id)
        {
            LocationControlModel model = new LocationControlModel();

            if (id == Guid.Empty)
            {
                return RedirectToAction("Index");
            }

            model.LocationModel = Db.VLocation.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.LocationUID == id);

            if (model.LocationModel != null)
            {
                model.LogList = Db.ApplicationLog.Where(x => x.Modul == "Location" && x.ProcessID == model.LocationModel.LocationID.ToString()).ToList();
                model.OurCompanyList = Db.OurCompany.ToList();
                model.PriceCategoryList = Db.PriceCategory.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
                model.MallList = Db.Mall.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
                model.BankAccountList = Db.VBankAccount.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.AccountTypeID == 2).ToList(); // TODO: AccountTypeID == 2 olmasının sebebi POS işlemlerinden dolayı
            }
            else
            {
                return RedirectToAction("Index", "Location");
            }

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetTimezoneList(int ourCompanyId)
        {
            var getOurCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == ourCompanyId);

            List<SelectListItem> list = new List<SelectListItem>();
            List<SelectListItem> priceList = new List<SelectListItem>();
            List<SelectListItem> mallList = new List<SelectListItem>();

            int start, finish = 0;

            start = getOurCompany.TimeZone.Value > getOurCompany.TimeZoneTo.Value ? getOurCompany.TimeZone.Value : getOurCompany.TimeZoneTo.Value;
            finish = getOurCompany.TimeZone.Value < getOurCompany.TimeZoneTo.Value ? getOurCompany.TimeZone.Value : getOurCompany.TimeZoneTo.Value;

            for (int i = start; i >= finish; i--)
            {
                list.Add(new SelectListItem()
                {
                    Value = i.ToString(),
                    Text = i > 0 ? "+" + i : i.ToString()
                });
            }

            priceList = Db.PriceCategory.Where(x => x.OurCompanyID == ourCompanyId).Select(y => new SelectListItem() { Value = y.ID.ToString(), Text = y.CategoryName }).ToList();
            mallList = Db.Mall.Where(x => x.OurCompanyID == ourCompanyId).Select(m => new SelectListItem() { Value = m.ID.ToString(), Text = m.FullName }).ToList();

            OurCompanyModel model = new OurCompanyModel()
            {
                Currency = getOurCompany.Currency,
                SelectList = list,
                PriceCategoryList = priceList,
                MallList = mallList
            };

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [AllowAnonymous]
        public ActionResult EditLocation(CULocation location)
        {
            Result result = new Result();

            LocationControlModel model = new LocationControlModel();
            DateTime daterecord = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value);
            var isLocation = Db.Location.FirstOrDefault(x => x.LocationID == location.LocationID && x.LocationUID == location.LocationUID);

            if (location != null && isLocation != null)
            {
                try
                {
                    //location.IsActive = !String.IsNullOrEmpty(location.IsActive) && location.IsActive == "1" ? true : false;
                    //location.IsHaveOperator = !String.IsNullOrEmpty(location.GetFormIsHaveOperator) && location.GetFormIsHaveOperator == "1" ? true : false;

                    Location self = new Location()
                    {

                    };
                }
                catch (Exception ex)
                {
                    result.Message = ex.Message;
                    result.IsSuccess = false;
                }
            }

            return RedirectToAction("Index", "Location");
        }

    }

    #region OurCompanyModel
    //SelectListItem MVC'den türediği için başka bir sınıfın içerisine alamadığımızdan dolayı OurCompanyModel sınıfı burada bulunmaktadır.
    public class OurCompanyModel
    {
        public List<SelectListItem> SelectList { get; set; }
        public List<SelectListItem> PriceCategoryList { get; set; }
        public List<SelectListItem> MallList { get; set; }
        public string Currency { get; set; }
    } 
    #endregion
}