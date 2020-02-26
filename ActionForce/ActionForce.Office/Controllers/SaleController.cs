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

            model.PriceCategoryList = Db.VPriceCategory.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.OurCompanyList = Db.OurCompany.ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult PartialAddPriceCategory()
        {
            SaleControlModel model = new SaleControlModel();

            model.OurCompanyList = Db.OurCompany.ToList();

            return PartialView("_PartialAddPriceCategory", model);
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

            model.PriceCategoryList = Db.VPriceCategory.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.OurCompanyList = Db.OurCompany.ToList();

            return PartialView("_PartialPriceCategoryList", model);
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult PartialEditPriceCategory(int? id)
        {
            SaleControlModel model = new SaleControlModel();

            model.PriceCategory = Db.VPriceCategory.FirstOrDefault(x => x.ID == id);
            model.OurCompanyList = Db.OurCompany.ToList();

            return PartialView("_PartialEditPriceCategory", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult EditPriceCategory(CUPriceCategory pricecategory)
        {
            SaleControlModel model = new SaleControlModel();

            model.Result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            if (pricecategory != null)
            {
                PriceCategory cat = Db.PriceCategory.FirstOrDefault(x => x.ID == pricecategory.ID);

                if (cat != null)
                {

                    PriceCategory self = new PriceCategory()
                    {
                        CategoryCode = cat.CategoryCode,
                        CategoryName = cat.CategoryName,
                        ID = cat.ID,
                        IsActive = cat.IsActive,
                        IsMaster = cat.IsMaster,
                        OurCompanyID = cat.OurCompanyID,
                        RecordDate = cat.RecordDate,
                        RecordEmployeeID = cat.RecordEmployeeID,
                        RecordIP = cat.RecordIP,
                        SortBy = cat.SortBy,
                        UpdateDate = cat.UpdateDate,
                        UpdateEmployeeID = cat.UpdateEmployeeID,
                        UpdateIP = cat.UpdateIP
                    };


                    cat.CategoryCode = pricecategory.CategoryCode;
                    cat.CategoryName = pricecategory.CategoryName;
                    cat.IsMaster = !string.IsNullOrEmpty(pricecategory.IsMaster) && pricecategory.IsMaster == "1" ? true : false;
                    cat.IsActive = !string.IsNullOrEmpty(pricecategory.IsActive) && pricecategory.IsActive == "1" ? true : false;
                    cat.OurCompanyID = pricecategory.OurCompanyID;
                    cat.RecordDate = DateTime.Now;
                    cat.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    cat.RecordIP = OfficeHelper.GetIPAddress();
                    cat.SortBy = pricecategory.SortBy;

                    Db.SaveChanges();

                    model.Result.IsSuccess = true;
                    model.Result.Message = $"{cat.CategoryName} Fiyat kategorisi güncellendi.";

                    var isequal = OfficeHelper.PublicInstancePropertiesEqual<PriceCategory>(self, cat, OfficeHelper.getIgnorelist());
                    OfficeHelper.AddApplicationLog("Office", "PriceCategory", "Update", cat.ID.ToString(), "Sale", "EditPriceCategory", isequal, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                }
                else
                {
                    model.Result.Message = $"{pricecategory.ID} ID'li fiyat kategorisi bulunamadı.";
                }
            }

            TempData["result"] = model.Result;

            model.PriceCategoryList = Db.VPriceCategory.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.OurCompanyList = Db.OurCompany.ToList();

            return PartialView("_PartialPriceCategoryList", model);
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult PartialRemovePriceCategory(int? id)
        {
            SaleControlModel model = new SaleControlModel();

            model.Result = new Result();

            var pricecat = Db.PriceCategory.FirstOrDefault(x => x.ID == id);
            var pricescount = Db.Price.Where(x => x.PriceCategoryID == id)?.Count();

            if (pricescount <= 0 && pricecat != null)
            {
                model.Result.IsSuccess = true;
                model.Result.Message = "Kategori başarı ile silindi";

                OfficeHelper.AddApplicationLog("Office", "PriceCategory", "Delete", pricecat.ID.ToString(), "Sale", "PartialRemovePriceCategory", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), $"{model.Authentication.ActionEmployee.EmployeeID} {model.Authentication.ActionEmployee.FullName}", OfficeHelper.GetIPAddress(), string.Empty, pricecat);

                Db.PriceCategory.Remove(pricecat);
                Db.SaveChanges();
            }
            else
            {
                if (pricescount > 0)
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = "Kategori silinemedi. Bağlı fiyatlar var";
                }
            }

            model.PriceCategoryList = Db.VPriceCategory.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            TempData["result"] = model.Result;

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