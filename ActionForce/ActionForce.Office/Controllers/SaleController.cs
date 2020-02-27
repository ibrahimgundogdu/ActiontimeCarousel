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
                    cat.UpdateDate = DateTime.Now;
                    cat.UpdateEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    cat.UpdateIP = OfficeHelper.GetIPAddress();
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

        #region TicketProduct

        [AllowAnonymous]
        public ActionResult TicketProduct()
        {
            SaleControlModel model = new SaleControlModel();

            model.TicketProductList = Db.VTicketProduct.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.TicketProductCategoryList = Db.TicketProductCategory.ToList();
            model.OurCompanyList = Db.OurCompany.ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult PartialAddTicketProduct()
        {
            SaleControlModel model = new SaleControlModel();

            model.TicketProductCategoryList = Db.TicketProductCategory.Where(x=> x.IsActive == true).ToList();
            model.TicketTypeList = Db.TicketType.Where(x => x.IsActive == true).ToList();
            model.OurCompanyList = Db.OurCompany.ToList();

            return PartialView("_PartialAddTicketProduct", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult AddTicketProduct(CUTicketProduct ticketproduct)
        {
            SaleControlModel model = new SaleControlModel();

            model.Result = new Result();

            if (ticketproduct != null)
            {
                TicketProduct prod = new TicketProduct();

                prod.ProductName = ticketproduct.ProductName;
                prod.CategoryID = ticketproduct.CategoryID;
                prod.Unit = ticketproduct.Unit;
                prod.IsActive = !string.IsNullOrEmpty(ticketproduct.IsActive) && ticketproduct.IsActive == "1" ? true : false;
                prod.OurCompanyID = ticketproduct.OurCompanyID;
                prod.RecordDate = DateTime.Now;
                prod.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                prod.RecordIP = OfficeHelper.GetIPAddress();
                prod.TicketTypeID = ticketproduct.TicketTypeID;

                Db.TicketProduct.Add(prod);
                Db.SaveChanges();

                model.Result.IsSuccess = true;
                model.Result.Message = $"{prod.ProductName} Bilet Ürünü Eklendi.";

                OfficeHelper.AddApplicationLog("Office", "TicketProduct", "Insert", prod.ID.ToString(), "Sale", "AddTicketProduct", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, prod);
            }

            model.TicketProductList = Db.VTicketProduct.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            return PartialView("_PartialTicketProductList", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult PartialEditTicketProduct(int? id)
        {
            SaleControlModel model = new SaleControlModel();

            model.TicketProduct = Db.VTicketProduct.FirstOrDefault(x => x.ID == id);
            model.TicketProductCategoryList = Db.TicketProductCategory.ToList();
            model.TicketTypeList = Db.TicketType.ToList();
            model.OurCompanyList = Db.OurCompany.ToList();

            return PartialView("_PartialEditTicketProduct", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult EditTicketProduct(CUTicketProduct ticketproduct)
        {
            SaleControlModel model = new SaleControlModel();

            model.Result = new Result();

            if (ticketproduct != null)
            {
                TicketProduct prod = Db.TicketProduct.FirstOrDefault(x => x.ID == ticketproduct.ID);

                if (prod != null)
                {

                    TicketProduct self = new TicketProduct()
                    {
                        ID = prod.ID,
                        IsActive = prod.IsActive,
                        OurCompanyID = prod.OurCompanyID,
                        RecordDate = prod.RecordDate,
                        RecordEmployeeID = prod.RecordEmployeeID,
                        RecordIP = prod.RecordIP,
                        UpdateDate = prod.UpdateDate,
                        UpdateEmployeeID = prod.UpdateEmployeeID,
                        UpdateIP = prod.UpdateIP,
                        CategoryID = prod.CategoryID,
                        ProductName = prod.ProductName,
                        Unit = prod.Unit,
                        TicketTypeID = prod.TicketTypeID
                    };

                    prod.CategoryID = ticketproduct.CategoryID;
                    prod.ProductName = ticketproduct.ProductName;
                    prod.IsActive = !string.IsNullOrEmpty(ticketproduct.IsActive) && ticketproduct.IsActive == "1" ? true : false;
                    prod.OurCompanyID = ticketproduct.OurCompanyID;
                    prod.UpdateDate = DateTime.Now;
                    prod.UpdateEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    prod.UpdateIP = OfficeHelper.GetIPAddress();
                    prod.Unit = ticketproduct.Unit;
                    prod.TicketTypeID = ticketproduct.TicketTypeID;

                    Db.SaveChanges();

                    model.Result.IsSuccess = true;
                    model.Result.Message = $"{prod.ProductName} Bilet Ürünü Güncellendi.";

                    var isequal = OfficeHelper.PublicInstancePropertiesEqual<TicketProduct>(self, prod, OfficeHelper.getIgnorelist());
                    OfficeHelper.AddApplicationLog("Office", "TicketProduct", "Update", prod.ID.ToString(), "Sale", "EditTicketProduct", isequal, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                }
                else
                {
                    model.Result.Message = $"{ticketproduct.ID} ID'li bilet ürünü bulunamadı.";
                }
            }

            model.TicketProductList = Db.VTicketProduct.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            return PartialView("_PartialTicketProductList", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult PartialRemoveTicketProduct(int? id)
        {
            SaleControlModel model = new SaleControlModel();

            model.Result = new Result();

            var ticketproduct = Db.TicketProduct.FirstOrDefault(x => x.ID == id);
            var pricescount = Db.Price.Where(x => x.ProductID == id)?.Count();

            if (pricescount <= 0 && ticketproduct != null)
            {
                model.Result.IsSuccess = true;
                model.Result.Message = "Bilet Ürünü başarı ile silindi";

                OfficeHelper.AddApplicationLog("Office", "TicketProduct", "Delete", ticketproduct.ID.ToString(), "Sale", "PartialRemoveTicketProduct", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), $"{model.Authentication.ActionEmployee.EmployeeID} {model.Authentication.ActionEmployee.FullName}", OfficeHelper.GetIPAddress(), string.Empty, ticketproduct);

                Db.TicketProduct.Remove(ticketproduct);
                Db.SaveChanges();
            }
            else
            {
                if (pricescount > 0)
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = "Bilet Ürünü silinemedi. Bağlı fiyatlar var";
                }
            }

            model.TicketProductList = Db.VTicketProduct.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            return PartialView("_PartialTicketProductList", model);
        }

        #endregion
    }
}