using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class LocationController : BaseController
    {
        // GET: Location
        [AllowAnonymous]
        public ActionResult Index()
        {
            LocationControlModel model = new LocationControlModel();

            model.LocationList = Db.GetLocationAll(model.Authentication.ActionEmployee.OurCompanyID).Select(x => new LocationModel()
            {
                Currency = x.Currency,
                Description = x.Description,
                IP = x.IP,
                IsActive = x.IsActive,
                IsHaveOperator = x.IsHaveOperator,
                Latitude = x.Latitude,
                LocationCode = x.LocationCode,
                LocationID = x.LocationID,
                LocationName = x.LocationName,
                LocationNameSearch = x.LocationNameSearch,
                LocationUID = x.LocationUID,
                Longitude = x.Longitude,
                MapURL = x.MapURL,
                SortBy = x.SortBy,
                State = x.State,
                TypeName = x.TypeName,
                Timezone = x.Timezone
            }).ToList();
            model.StateList = model.LocationList.Select(x => x.State).Distinct().OrderBy(x => x).ToList();
            model.TypeList = model.LocationList.Select(x => x.TypeName).Distinct().OrderBy(x => x).ToList();

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
                    model.LocationList = model.LocationList.Where(x => x.TypeName == model.FilterModel.TypeName).OrderBy(x => x.SortBy).ToList();
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

            model.LocationList = Db.GetLocationAll(model.Authentication.ActionEmployee.OurCompanyID).Select(x => new LocationModel()
            {
                Currency = x.Currency,
                Description = x.Description,
                IP = x.IP,
                IsActive = x.IsActive,
                IsHaveOperator = x.IsHaveOperator,
                Latitude = x.Latitude,
                LocationCode = x.LocationCode,
                LocationID = x.LocationID,
                LocationName = x.LocationName,
                LocationNameSearch = x.LocationNameSearch,
                LocationUID = x.LocationUID,
                Longitude = x.Longitude,
                MapURL = x.MapURL,
                SortBy = x.SortBy,
                State = x.State,
                TypeName = x.TypeName,
                Timezone = x.Timezone
            }).ToList();

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
                model.LocationList = model.LocationList.Where(x => x.TypeName == getModel.TypeName).OrderBy(x => x.SortBy).ToList();
            }

            bool? isActive = getModel.IsActive == 0 ? false : getModel.IsActive == 1 ? true : (bool?)null;

            if (isActive != null)
            {
                model.LocationList = model.LocationList.Where(x => x.IsActive == isActive.Value).ToList();
            }

            #endregion

            return PartialView("_PartialLocationList", model);
        }
    }
}