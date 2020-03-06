using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        #region Price

        [AllowAnonymous]
        public ActionResult Price()
        {
            SaleControlModel model = new SaleControlModel();


            model.OurCompanyList = Db.OurCompany.ToList();
            model.TicketTypeList = Db.TicketType.ToList();
            model.PriceCategoryList = Db.VPriceCategory.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.TicketProductList = Db.VTicketProduct.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            if (TempData["PriceFilter"] != null)
            {
                model.FilterModel = TempData["PriceFilter"] as PriceFilterModel;

                if (model.FilterModel.ListType == 1)
                {
                    model.PriceLastList = Db.VPriceLastList.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

                    if (model.FilterModel.PriceCategoryID != null)
                    {
                        model.PriceLastList = model.PriceLastList.Where(x => x.PriceCategoryID == model.FilterModel.PriceCategoryID).ToList();
                    }

                    if (model.FilterModel.TicketTypeID != null)
                    {
                        model.PriceLastList = model.PriceLastList.Where(x => x.TicketTypeID == model.FilterModel.TicketTypeID).ToList();
                    }

                    if (model.FilterModel.ProductID != null)
                    {
                        model.PriceLastList = model.PriceLastList.Where(x => x.ProductID == model.FilterModel.ProductID).ToList();
                    }

                }
                else if (model.FilterModel.ListType == 2)
                {
                    model.PriceList = Db.VPrice.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

                    if (model.FilterModel.PriceCategoryID != null)
                    {
                        model.PriceList = model.PriceList.Where(x => x.PriceCategoryID == model.FilterModel.PriceCategoryID).ToList();
                    }

                    if (model.FilterModel.TicketTypeID != null)
                    {
                        model.PriceList = model.PriceList.Where(x => x.TicketTypeID == model.FilterModel.TicketTypeID).ToList();
                    }

                    if (model.FilterModel.ProductID != null)
                    {
                        model.PriceList = model.PriceList.Where(x => x.ProductID == model.FilterModel.ProductID).ToList();
                    }

                    if (model.FilterModel.IsActive != null)
                    {
                        model.PriceList = model.PriceList.Where(x => x.IsActive == model.FilterModel.IsActive).ToList();
                    }

                }
            }
            else
            {
                model.PriceLastList = Db.VPriceLastList.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            }

            TempData["PriceFilter"] = model.FilterModel;

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult AddPrice()
        {
            SaleControlModel model = new SaleControlModel();

            model.PriceCategoryList = Db.VPriceCategory.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult FillPriceList(int? CategoryID)//int? TicketTypeID
        {
            SaleControlModel model = new SaleControlModel();
            model.TicketTypeList = Db.TicketType.ToList();
            model.PriceCategoryList = Db.VPriceCategory.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.TicketProductList = Db.VTicketProduct.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            model.PriceLastList = Db.VPriceLastList.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            if (CategoryID > 0)
            {
                model.CurrentPriceCategory = model.PriceCategoryList.FirstOrDefault(x => x.ID == CategoryID);
                model.PriceLastList = model.PriceLastList.Where(x => x.PriceCategoryID == CategoryID && x.TicketTypeID == model.CurrentPriceCategory.TicketTypeID).ToList();
                model.PriceCategoryList = model.PriceCategoryList.Where(x => x.TicketTypeID == model.CurrentPriceCategory.TicketTypeID && x.ID == CategoryID).ToList();
                model.TicketProductList = model.TicketProductList.Where(x => x.TicketTypeID == model.CurrentPriceCategory.TicketTypeID).ToList();
            }

            return PartialView("_PartialAddPriceList", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddNewPrice(FormNewPrice[] pricelist, int? CategoryID, string DateBegin, string DateBeginHour)
        {
            SaleControlModel model = new SaleControlModel();
            model.Result = new Result();

            //double? price1 = Convert.ToDouble(ajPrice._price.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
            DateTime? date = Convert.ToDateTime(DateBegin);
            TimeSpan? time = Convert.ToDateTime(DateBeginHour).TimeOfDay;
            DateTime? startdatetime = date.Value.Add(time.Value);

            foreach (var item in pricelist)
            {
                if (!string.IsNullOrEmpty(item.IsSelected) && !string.IsNullOrEmpty(item.Price))
                {

                    var isSelected = item.IsSelected != null && item.IsSelected == "1" ? true : false;
                    var isUseSale = item.UseSale != null && item.UseSale == "1" ? true : false;
                    var isActive = item.IsActive != null && item.IsActive == "1" ? true : false;
                    double? price = Convert.ToDouble(item.Price.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                    int? productID = item.productID;
                    var product = Db.TicketProduct.FirstOrDefault(x => x.ID == productID);
                    int? categoryID = item.categoryID;

                    Price newprice = new Price();

                    newprice.Currency = model.Authentication.ActionEmployee.OurCompany.Currency;
                    newprice.ExtraMultiple = 1;
                    newprice.IsActive = isActive;
                    newprice.OurCompanyID = model.Authentication.ActionEmployee.OurCompany.CompanyID;
                    newprice.Price1 = price;
                    newprice.PriceCategoryID = categoryID;
                    newprice.ProductID = productID;
                    newprice.TicketTypeID = product.TicketTypeID;
                    newprice.Unit = product.Unit;
                    newprice.UseToSale = isUseSale;
                    newprice.RecordDate = DateTime.UtcNow;
                    newprice.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    newprice.RecordIP = OfficeHelper.GetIPAddress();
                    newprice.StartDate = startdatetime;

                    Db.Price.Add(newprice);
                    Db.SaveChanges();

                    model.Result.IsSuccess = true;
                    model.Result.Message = $"İlgili kategoriye {product.Unit} Dk. için fiyat eklendi.";

                    OfficeHelper.AddApplicationLog("Office", "Price", "Insert", newprice.ID.ToString(), "Sale", "AddNewPrice", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow, model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newprice);
                }
            }

            return RedirectToAction("PriceCategoryDetail", "Sale", new { id = CategoryID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult PriceFilter(PriceFilterModel filterModel)
        {
            filterModel.IsActive = !string.IsNullOrEmpty(filterModel.Active) && filterModel.Active == "1" ? true : false;
            TempData["PriceFilter"] = filterModel;

            return RedirectToAction("Price");
        }

        [AllowAnonymous]
        public ActionResult PriceDetail(int? id)
        {
            SaleControlModel model = new SaleControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            if (id != null)
            {
                model.Price = Db.VPrice.FirstOrDefault(x => x.ID == id);
                if (model.Price != null)
                {

                    model.PriceList = Db.VPrice.Where(x => x.OurCompanyID == model.Price.OurCompanyID && x.PriceCategoryID == model.Price.PriceCategoryID && x.TicketTypeID == model.Price.TicketTypeID && x.ProductID == model.Price.ProductID).ToList();
                }
                else
                {
                    model.Result = new Result()
                    {
                        IsSuccess = false,
                        Message = "Geçerli bir fiyat seçilmesi gerekir."
                    };
                }
            }
            else
            {
                model.Result = new Result()
                {
                    IsSuccess = false,
                    Message = "Geçerli bir fiyat seçilmesi gerekir."
                };
            }

            model.OurCompanyList = Db.OurCompany.ToList();
            model.TicketTypeList = Db.TicketType.ToList();
            model.PriceCategoryList = Db.VPriceCategory.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.TicketProductList = Db.VTicketProduct.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            if (TempData["PriceFilter"] != null)
            {
                model.FilterModel = TempData["PriceFilter"] as PriceFilterModel;
            }

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult PriceCategoryDetail(int? id)
        {
            SaleControlModel model = new SaleControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            if (id != null)
            {
                model.PriceList = Db.VPrice.Where(x => x.PriceCategoryID == id).ToList();
            }
            else
            {
                model.Result = new Result()
                {
                    IsSuccess = false,
                    Message = "Geçerli bir kategori seçilmesi gerekir."
                };
            }

            model.OurCompanyList = Db.OurCompany.ToList();
            model.TicketTypeList = Db.TicketType.ToList();
            model.PriceCategoryList = Db.VPriceCategory.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.TicketProductList = Db.VTicketProduct.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            if (TempData["PriceFilter"] != null)
            {
                model.FilterModel = TempData["PriceFilter"] as PriceFilterModel;
            }

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult PriceTypeDetail(int? id)
        {
            SaleControlModel model = new SaleControlModel();

            if (id != null)
            {
                model.PriceList = Db.VPrice.Where(x => x.TicketTypeID == id).ToList();
            }
            else
            {
                model.Result = new Result()
                {
                    IsSuccess = false,
                    Message = "Geçerli bir bilet türü seçilmesi gerekir."
                };
            }

            //model.OurCompanyList = Db.OurCompany.ToList();
            //model.TicketTypeList = Db.TicketType.ToList();
            //model.PriceCategoryList = Db.VPriceCategory.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            //model.TicketProductList = Db.VTicketProduct.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            if (TempData["PriceFilter"] != null)
            {
                model.FilterModel = TempData["PriceFilter"] as PriceFilterModel;
            }

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditPrice(FormPrice frmprice)
        {
            SaleControlModel model = new SaleControlModel();
            model.Result = new Result();

            if (frmprice != null)
            {
                var isPrice = Db.Price.FirstOrDefault(x => x.ID == frmprice.ID);

                if (isPrice != null)
                {
                    Price self = new Price()
                    {
                        Currency = isPrice.Currency,
                        ExtraMultiple = isPrice.ExtraMultiple,
                        ID = isPrice.ID,
                        IsActive = isPrice.IsActive,
                        OurCompanyID = isPrice.OurCompanyID,
                        Price1 = isPrice.Price1,
                        PriceCategoryID = isPrice.PriceCategoryID,
                        ProductID = isPrice.ProductID,
                        RecordDate = isPrice.RecordDate,
                        RecordEmployeeID = isPrice.RecordEmployeeID,
                        RecordIP = isPrice.RecordIP,
                        StartDate = isPrice.StartDate,
                        TicketTypeID = isPrice.TicketTypeID,
                        Unit = isPrice.Unit,
                        UpdateDate = isPrice.UpdateDate,
                        UpdateEmployeeID = isPrice.UpdateEmployeeID,
                        UpdateIP = isPrice.UpdateIP,
                        UseToSale = isPrice.UseToSale
                    };

                    double? price1 = Convert.ToDouble(frmprice.Price.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                    DateTime? date = Convert.ToDateTime(frmprice.DateBegin);
                    TimeSpan? time = Convert.ToDateTime(frmprice.DateBeginHour).TimeOfDay;
                    DateTime? startdatetime = date.Value.Add(time.Value);

                    isPrice.IsActive = !string.IsNullOrEmpty(frmprice.Active) && frmprice.Active == "1" ? true : false;
                    isPrice.UseToSale = !string.IsNullOrEmpty(frmprice.UseSale) && frmprice.UseSale == "1" ? true : false;
                    isPrice.StartDate = startdatetime;
                    isPrice.Price1 = price1;
                    isPrice.UpdateDate = DateTime.UtcNow;
                    isPrice.UpdateEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    isPrice.UpdateIP = OfficeHelper.GetIPAddress();

                    Db.SaveChanges();

                    model.Result = new Result() { IsSuccess = true, Message = $"{isPrice.ID} Id'li fiyat başarı ile güncellendi" };

                    var isequal = OfficeHelper.PublicInstancePropertiesEqual<Price>(self, isPrice, OfficeHelper.getIgnorelist());
                    OfficeHelper.AddApplicationLog("Office", "Price", "Update", isPrice.ID.ToString(), "Sale", "EditPrice", isequal, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow, model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);


                    model.PriceList = Db.VPrice.Where(x => x.ProductID == isPrice.ProductID).ToList();

                }
            }

            return PartialView("_PartialPriceList", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult CatPriceEdit(FormAjaxPrice ajPrice) // ajaxtan gelen edit
        {
            SaleControlModel model = new SaleControlModel();
            model.Result = new Result();

            if (ajPrice != null)
            {
                var isPrice = Db.Price.FirstOrDefault(x => x.ID == ajPrice.id);

                if (isPrice != null)
                {
                    Price self = new Price()
                    {
                        Currency = isPrice.Currency,
                        ExtraMultiple = isPrice.ExtraMultiple,
                        ID = isPrice.ID,
                        IsActive = isPrice.IsActive,
                        OurCompanyID = isPrice.OurCompanyID,
                        Price1 = isPrice.Price1,
                        PriceCategoryID = isPrice.PriceCategoryID,
                        ProductID = isPrice.ProductID,
                        RecordDate = isPrice.RecordDate,
                        RecordEmployeeID = isPrice.RecordEmployeeID,
                        RecordIP = isPrice.RecordIP,
                        StartDate = isPrice.StartDate,
                        TicketTypeID = isPrice.TicketTypeID,
                        Unit = isPrice.Unit,
                        UpdateDate = isPrice.UpdateDate,
                        UpdateEmployeeID = isPrice.UpdateEmployeeID,
                        UpdateIP = isPrice.UpdateIP,
                        UseToSale = isPrice.UseToSale
                    };

                    double? price1 = Convert.ToDouble(ajPrice._price.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                    DateTime? date = Convert.ToDateTime(ajPrice._datebegin);
                    TimeSpan? time = Convert.ToDateTime(ajPrice._datebeginhour).TimeOfDay;
                    DateTime? startdatetime = date.Value.Add(time.Value);

                    isPrice.IsActive = ajPrice._isactive != null && ajPrice._isactive == 1 ? true : false;
                    isPrice.UseToSale = ajPrice._usesale != null && ajPrice._usesale == 1 ? true : false;
                    isPrice.StartDate = startdatetime;
                    isPrice.Price1 = price1;
                    isPrice.UpdateDate = DateTime.UtcNow;
                    isPrice.UpdateEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    isPrice.UpdateIP = OfficeHelper.GetIPAddress();

                    Db.SaveChanges();

                    model.Result = new Result() { IsSuccess = true, Message = $"{isPrice.ID} Id'li fiyat başarı ile güncellendi" };

                    var isequal = OfficeHelper.PublicInstancePropertiesEqual<Price>(self, isPrice, OfficeHelper.getIgnorelist());
                    OfficeHelper.AddApplicationLog("Office", "Price", "Update", isPrice.ID.ToString(), "Sale", "PriceEdit", isequal, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow, model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);


                    model.PriceList = Db.VPrice.Where(x => x.PriceCategoryID == isPrice.PriceCategoryID).ToList();

                }
            }

            return PartialView("_PartialEditablePriceList", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult TypePriceEdit(FormAjaxPrice ajPrice) // ajaxtan gelen edit
        {
            SaleControlModel model = new SaleControlModel();
            model.Result = new Result();

            if (ajPrice != null)
            {
                var isPrice = Db.Price.FirstOrDefault(x => x.ID == ajPrice.id);

                if (isPrice != null)
                {
                    Price self = new Price()
                    {
                        Currency = isPrice.Currency,
                        ExtraMultiple = isPrice.ExtraMultiple,
                        ID = isPrice.ID,
                        IsActive = isPrice.IsActive,
                        OurCompanyID = isPrice.OurCompanyID,
                        Price1 = isPrice.Price1,
                        PriceCategoryID = isPrice.PriceCategoryID,
                        ProductID = isPrice.ProductID,
                        RecordDate = isPrice.RecordDate,
                        RecordEmployeeID = isPrice.RecordEmployeeID,
                        RecordIP = isPrice.RecordIP,
                        StartDate = isPrice.StartDate,
                        TicketTypeID = isPrice.TicketTypeID,
                        Unit = isPrice.Unit,
                        UpdateDate = isPrice.UpdateDate,
                        UpdateEmployeeID = isPrice.UpdateEmployeeID,
                        UpdateIP = isPrice.UpdateIP,
                        UseToSale = isPrice.UseToSale
                    };

                    double? price1 = Convert.ToDouble(ajPrice._price.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                    DateTime? date = Convert.ToDateTime(ajPrice._datebegin);
                    TimeSpan? time = Convert.ToDateTime(ajPrice._datebeginhour).TimeOfDay;
                    DateTime? startdatetime = date.Value.Add(time.Value);

                    isPrice.IsActive = ajPrice._isactive != null && ajPrice._isactive == 1 ? true : false;
                    isPrice.UseToSale = ajPrice._usesale != null && ajPrice._usesale == 1 ? true : false;
                    isPrice.StartDate = startdatetime;
                    isPrice.Price1 = price1;
                    isPrice.UpdateDate = DateTime.UtcNow;
                    isPrice.UpdateEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    isPrice.UpdateIP = OfficeHelper.GetIPAddress();

                    Db.SaveChanges();

                    model.Result = new Result() { IsSuccess = true, Message = $"{isPrice.ID} Id'li fiyat başarı ile güncellendi" };

                    var isequal = OfficeHelper.PublicInstancePropertiesEqual<Price>(self, isPrice, OfficeHelper.getIgnorelist());
                    OfficeHelper.AddApplicationLog("Office", "Price", "Update", isPrice.ID.ToString(), "Sale", "PriceEdit", isequal, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow, model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);


                    model.PriceList = Db.VPrice.Where(x => x.TicketTypeID == isPrice.TicketTypeID).ToList();

                }
            }

            return PartialView("_PartialEditableTypePriceList", model);
        }

        [AllowAnonymous]
        public ActionResult DeletePrice(int? id)
        {
            SaleControlModel model = new SaleControlModel();

            model.Result = new Result();
            int? catid = 0;

            if (id != null)
            {
                var price = Db.Price.FirstOrDefault(x => x.ID == id);
                catid = price.PriceCategoryID;

                model.Result.IsSuccess = true;
                model.Result.Message = $"{price.Unit} Dk. lı {price.PriceCategoryID} kategorisinden silindi.";

                OfficeHelper.AddApplicationLog("Office", "Price", "Delete", price.ID.ToString(), "Sale", "DeletePrice", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow, model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, price);

                Db.Price.Remove(price);
                Db.SaveChanges();

                TempData["Result"] = model.Result;

                return RedirectToAction("PriceCategoryDetail", "Sale", new { id = catid });
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Geçerli bir fiyat seçilmesi gerekir.";

                TempData["Result"] = model.Result;

                return RedirectToAction("PriceDetail","Sale",new { id });
            }

            
        }


        #endregion

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
            model.TicketTypeList = Db.TicketType.Where(x => x.IsActive == true).ToList();

            return PartialView("_PartialAddPriceCategory", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult AddPriceCategory(FormPriceCategory pricecategory)
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
                cat.SortBy = pricecategory.SortBy.Trim();
                cat.TicketTypeID = pricecategory.TicketTypeID;

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
            model.TicketTypeList = Db.TicketType.Where(x => x.IsActive == true).ToList();


            return PartialView("_PartialEditPriceCategory", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult EditPriceCategory(FormPriceCategory pricecategory)
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
                        UpdateIP = cat.UpdateIP,
                        TicketTypeID = cat.TicketTypeID
                    };


                    cat.CategoryCode = pricecategory.CategoryCode;
                    cat.CategoryName = pricecategory.CategoryName;
                    cat.IsMaster = !string.IsNullOrEmpty(pricecategory.IsMaster) && pricecategory.IsMaster == "1" ? true : false;
                    cat.IsActive = !string.IsNullOrEmpty(pricecategory.IsActive) && pricecategory.IsActive == "1" ? true : false;
                    cat.OurCompanyID = pricecategory.OurCompanyID;
                    cat.UpdateDate = DateTime.Now;
                    cat.UpdateEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    cat.UpdateIP = OfficeHelper.GetIPAddress();
                    cat.SortBy = pricecategory.SortBy.Trim();
                    cat.TicketTypeID = pricecategory.TicketTypeID;


                    Db.SaveChanges();

                    model.Result.IsSuccess = true;
                    model.Result.Message = $"{cat.CategoryName} Fiyat kategorisi güncellendi.";

                    var isequal = OfficeHelper.PublicInstancePropertiesEqual<PriceCategory>(self, cat, OfficeHelper.getIgnorelist());
                    OfficeHelper.AddApplicationLog("Office", "PriceCategory", "Update", cat.ID.ToString(), "Sale", "EditPriceCategory", isequal, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow, model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

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

            model.TicketProductCategoryList = Db.TicketProductCategory.Where(x => x.IsActive == true).ToList();
            model.TicketTypeList = Db.TicketType.Where(x => x.IsActive == true).ToList();
            model.OurCompanyList = Db.OurCompany.ToList();

            return PartialView("_PartialAddTicketProduct", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult AddTicketProduct(FormTicketProduct ticketproduct)
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
                prod.Description = ticketproduct.Description;

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
        public PartialViewResult EditTicketProduct(FormTicketProduct ticketproduct)
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
                        TicketTypeID = prod.TicketTypeID,
                        Description = prod.Description
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
                    prod.Description = ticketproduct.Description;

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