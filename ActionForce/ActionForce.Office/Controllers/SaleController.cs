using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class SaleController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            SaleControlModel model = new SaleControlModel();

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Price()
        {
            SaleControlModel model = new SaleControlModel();

            return View(model);
        }

        #region PriceCategory

        [AllowAnonymous]
        public ActionResult PriceCategory()
        {
            SaleControlModel model = new SaleControlModel();

            model.PriceCategoryList = Db.VPriceCategory.ToList();
            model.OurCompanyList = Db.OurCompany.ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult AddPriceCategory(CUPriceCategory pricecategory)
        {
            SaleControlModel model = new SaleControlModel();
            model.Result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            if (pricecategory != null)
            {
                PriceCategory cat = new PriceCategory();

                cat.CategoryCode = pricecategory.CategoryCode;
                cat.CategoryName = pricecategory.CategoryName;
                cat.IsMaster = !string.IsNullOrEmpty(pricecategory.IsMaster) && pricecategory.IsMaster == "1" ? true : false;
                cat.IsActive = !string.IsNullOrEmpty(pricecategory.IsActive) && pricecategory.IsActive == "1" ? true : false;
                cat.OurCompanyID = pricecategory.OurCompanyID;
                cat.RecordDate = DateTime.Now;
                cat.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                cat.RecordIP = OfficeHelper.GetIPAddress();
                cat.SortBy = pricecategory.SortBy;

                Db.PriceCategory.Add(cat);
                Db.SaveChanges();

                model.Result.IsSuccess = true;
                model.Result.Message = $"{cat.CategoryName} Fiyat kategorisi eklendi.";

                OfficeHelper.AddApplicationLog("Office", "PriceCategory", "Insert", cat.ID.ToString(), "Sale", "AddPriceCategory", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, cat);
            }

            model.PriceCategoryList = Db.VPriceCategory.ToList();
            model.OurCompanyList = Db.OurCompany.ToList();

            return PartialView("_PartialPriceCategoryList", model);
        }

        #endregion


        [AllowAnonymous]
        public ActionResult TicketProduct()
        {
            SaleControlModel model = new SaleControlModel();

            return View(model);
        }
    }
}