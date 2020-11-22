using ActionForce.Entity;
using ActionForce.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Location.Controllers
{
    public class HomeController : BaseController
    {
        private readonly DocumentManager documentManager;
        public HomeController()
        {
            LayoutControlModel model = new LayoutControlModel();

            documentManager = new DocumentManager(
               new ProcessEmployee()
               {
                   ID = model.Authentication.CurrentEmployee.EmployeeID,
                   FullName = model.Authentication.CurrentEmployee.FullName
               },
               LocationHelper.GetIPAddress(),
               new ProcessCompany()
               {
                   ID = model.Authentication.CurrentOurCompany.ID,
                   Name = model.Authentication.CurrentOurCompany.Name,
                   Currency = model.Authentication.CurrentOurCompany.Currency,
                   TimeZone = model.Authentication.CurrentOurCompany.TimeZone
               }
           );
        }

        // GET: Home
        [AllowAnonymous]
        public ActionResult Index()
        {
            HomeControlModel model = new HomeControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            model.PriceList = Db.GetLocationCurrentPrices(model.Authentication.CurrentLocation.ID).ToList();

            LocationServiceManager manager = new LocationServiceManager(Db, model.Authentication.CurrentLocation);
            model.DocumentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));
            var currentDate = DateTime.UtcNow.AddHours(model.Location.TimeZone);


            //model.Summary = manager.GetLocationSummary(DateTime.Now.Date, model.Authentication.CurrentEmployee);
            model.LocationBalance = manager.GetLocationSaleBalanceToday(currentDate);
            model.TicketList = manager.GetLocationTicketsToday(currentDate);

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Detail(Guid? id)
        {
            HomeControlModel model = new HomeControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            model.PriceList = Db.GetLocationCurrentPrices(model.Authentication.CurrentLocation.ID).ToList();
            LocationServiceManager manager = new LocationServiceManager(Db, model.Authentication.CurrentLocation);

            if (id != null)
            {
                model.SaleRow = Db.TicketSaleRows.FirstOrDefault(x => x.UID == id);

                if (model.SaleRow != null)
                {
                    model.ParentSaleRow = null;

                    if (model.SaleRow.ParentID > 0)
                    {
                        model.ParentSaleRow = Db.TicketSaleRows.FirstOrDefault(x => x.ID == model.SaleRow.ParentID);
                    }

                    model.ChildSaleRow = null;

                    if (Db.TicketSaleRows.Any(x => x.ParentID == model.SaleRow.ID))
                    {
                        model.ChildSaleRow = Db.TicketSaleRows.FirstOrDefault(x => x.ParentID == model.SaleRow.ID);
                    }

                    model.Sale = Db.TicketSale.FirstOrDefault(x => x.ID == model.SaleRow.SaleID);
                    model.VPrice = Db.VPrice.FirstOrDefault(x => x.ID == model.SaleRow.PriceID);

                    model.Status = Db.TicketStatus.Where(x => new List<int> { 2, 4, 5 }.Contains(x.ID)).ToList();
                    model.PayMethods = Db.PayMethod.Where(x => x.ID > 0).ToList();
                    model.PriceList = Db.GetLocationCurrentPrices(model.Authentication.CurrentLocation.ID).ToList();

                    if (model.VPrice.TicketTypeID == 2)
                    {
                        model.AnimalCostumes = Db.GetLocationAnimalCostums(model.Location.ID).Select(x => new AnimalCostume()
                        {
                            CostumeID = x.ID,
                            CostumeName = x.CostumeName
                        }).ToList();
                    }

                    if (model.VPrice.TicketTypeID == 7)
                    {
                        model.MallMotoColor = Db.GetLocationMallMotoColors(model.Location.ID).Select(x => new MallMotoColor()
                        {
                            ColorID = x.ID,
                            ColorName = x.ColorName
                        }).ToList();
                    }

                    model.EmployeeRecorded = Db.Employee.FirstOrDefault(x => x.EmployeeID == model.SaleRow.RecordEmployeeID)?.FullName;
                    model.EmployeeUpdated = Db.Employee.FirstOrDefault(x => x.EmployeeID == model.SaleRow.UpdateEmployeeID)?.FullName;
                    model.SaleChannelName = Db.SaleChannel.FirstOrDefault(x => x.ID == model.Sale.SaleChannelD)?.ChannelName;
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return RedirectToAction("Index");
            }


            return View(model);
        }

        [AllowAnonymous]
        public PartialViewResult GetTicketModal(int PriceID, int PaymethodID)
        {
            StandartTicket model = new StandartTicket();

            model.Price = Db.VPrice.FirstOrDefault(x => x.ID == PriceID);
            model.PayMethodID = PaymethodID;

            if (model.Price.TicketTypeID == 2)
            {
                model.AnimalCostumes = Db.GetLocationAnimalCostums(model.Location.ID).Select(x => new AnimalCostume()
                {
                    CostumeID = x.ID,
                    CostumeName = x.CostumeName
                }).ToList();
            }

            if (model.Price.TicketTypeID == 7)
            {
                model.MallMotoColor = Db.GetLocationMallMotoColors(model.Location.ID).Select(x => new MallMotoColor()
                {
                    ColorID = x.ID,
                    ColorName = x.ColorName
                }).ToList();
            }

            return PartialView("_PartialTicket", model);
        }

        [AllowAnonymous]
        public string SetTicketSale(int PriceID, int PaymethodID, int? ColorID, int? CostumeID)
        {
            string message = string.Empty;

            StandartTicket model = new StandartTicket();
            var documentdate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));
            var date = DateTime.UtcNow.AddHours(model.Location.TimeZone);
            var processdate = DateTime.UtcNow.AddHours(model.Location.TimeZone);

            model.Price = Db.VPrice.FirstOrDefault(x => x.ID == PriceID);
            model.PayMethodID = PaymethodID;
            var OrderNumber = string.Format("S{0}{1}{2}{3}", date.Year.ToString().Substring(2, 2), date.Month < 10 ? "0" + date.Month.ToString() : date.Month.ToString(), date.Day < 10 ? "0" + date.Day.ToString() : date.Day.ToString(), date.Ticks.ToString());
            string ip = LocationHelper.GetIPAddress();

            var rowID = Db.AddLocationTicketSale(0, model.Location.ID, date, PriceID, PaymethodID, 2, model.Authentication.CurrentEmployee.EmployeeID, ColorID > 0 ? ColorID : null, CostumeID > 0 ? CostumeID : null, OrderNumber, ip, 2, null).FirstOrDefault();

            if (rowID > 0)
            {
                model.Result.IsSuccess = true;
                model.Result.Message = "Satış Eklendi";

                var saleRow = new TicketSaleRows();

                using (ActionTimeEntities _db = new ActionTimeEntities())
                {
                    saleRow = _db.TicketSaleRows.FirstOrDefault(x => x.ID == rowID);
                }

                LocationHelper.AddApplicationLog("Location", "TicketSaleRows", "Insert", rowID.ToString(), "Home", "SetTicketSale", null, true, message, string.Empty, processdate, $"{model.Authentication.CurrentEmployee.EmployeeID} - {model.Authentication.CurrentEmployee.FullName}", LocationHelper.GetIPAddress(), string.Empty, saleRow);

                System.Threading.Tasks.Task lblusatask = System.Threading.Tasks.Task.Factory.StartNew(() => documentManager.CheckLocationTicketSale(rowID.Value, date));

            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Satış Eklenemedi";
            }

            TempData["Result"] = model.Result;

            return message;
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateSaleRow(FormSaleRow formSaleRow)
        {
            string message = string.Empty;

            StandartTicket model = new StandartTicket();

            if (formSaleRow != null && formSaleRow.UID != null)
            {
                var saleRow = Db.TicketSaleRows.FirstOrDefault(x => x.UID == formSaleRow.UID);

                if (saleRow != null)
                {
                    var self = new TicketSaleRows()
                    {
                        TicketTripID = saleRow.TicketTripID,
                        AnimalCostumeTypeID = saleRow.AnimalCostumeTypeID,
                        Currency = saleRow.Currency,
                        CustomerData = saleRow.CustomerData,
                        UID = saleRow.UID,
                        CustomerName = saleRow.CustomerName,
                        Date = saleRow.Date,
                        Description = saleRow.Description,
                        DeviceID = saleRow.DeviceID,
                        Discount = saleRow.Discount,
                        EmployeeID = saleRow.EmployeeID,
                        ExtraUnit = saleRow.ExtraUnit,
                        ID = saleRow.ID,
                        IsExchangable = saleRow.IsExchangable,
                        IsPromotion = saleRow.IsPromotion,
                        IsSale = saleRow.IsSale,
                        Latitude = saleRow.Latitude,
                        LocationID = saleRow.LocationID,
                        Longitude = saleRow.Longitude,
                        MallMotoColorID = saleRow.MallMotoColorID,
                        PaymethodID = saleRow.PaymethodID,
                        PrePaid = saleRow.PrePaid,
                        Price = saleRow.Price,
                        PriceCategoryID = saleRow.PriceCategoryID,
                        PriceID = saleRow.PriceID,
                        PromotionID = saleRow.PromotionID,
                        Quantity = saleRow.Quantity,
                        RecordDate = saleRow.RecordDate,
                        RecordEmployeeID = saleRow.RecordEmployeeID,
                        SaleID = saleRow.SaleID,
                        StatusID = saleRow.StatusID,
                        TicketNumber = saleRow.TicketNumber,
                        TicketTypeID = saleRow.TicketTypeID,
                        Total = saleRow.Total,
                        Unit = saleRow.Unit,
                        UpdateDate = saleRow.UpdateDate,
                        UpdateEmployeeID = saleRow.UpdateEmployeeID,
                        UseImmediately = saleRow.UseImmediately,
                        ExtraPrice = saleRow.ExtraPrice,
                        ParentID = saleRow.ParentID
                        
                    };

                    DateTime ticketdate = saleRow.Date.Date.Add(formSaleRow.RecordTime);
                    int? colorid = formSaleRow.ColorID ?? null;
                    int? costumeid = formSaleRow.CostumeID ?? null;

                    var rowid = Db.UpdateLocationTicketSale(saleRow.LocationID, saleRow.ID, ticketdate, formSaleRow.PriceID, formSaleRow.ExtraUnit, formSaleRow.PayMethodID, formSaleRow.StatusID, model.Authentication.CurrentEmployee.EmployeeID, colorid, costumeid, formSaleRow.Description).FirstOrDefault();

                    if (rowid != null && rowid > 0)
                    {
                        using (ActionTimeEntities _db = new ActionTimeEntities())
                        {
                            saleRow = _db.TicketSaleRows.FirstOrDefault(x => x.ID == saleRow.ID);
                        }

                        model.Result.IsSuccess = true;
                        model.Result.Message = "Güncelleme Başarılı";
                        DateTime dateProcess = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone);

                        var isequal = LocationHelper.PublicInstancePropertiesEqual<TicketSaleRows>(self, saleRow, LocationHelper.getIgnorelist());
                        LocationHelper.AddApplicationLog("Location", "TicketSaleRows", "Update", saleRow.ID.ToString(), "Home", "UpdateSaleRow", isequal, true, message, string.Empty, ticketdate, $"{model.Authentication.CurrentEmployee.EmployeeID} - {model.Authentication.CurrentEmployee.FullName}", LocationHelper.GetIPAddress(), string.Empty, null);

                        System.Threading.Tasks.Task lblusatask = System.Threading.Tasks.Task.Factory.StartNew(() => documentManager.CheckLocationTicketSale(rowid.Value, dateProcess));

                    }
                    else
                    {
                        model.Result.Message = "Güncelleme Başarısız";
                    }
                }
                else
                {
                    model.Result.Message = "Satış Bulunamadı";
                }
            }
            else
            {
                model.Result.Message = "Form Verisi Alınamadı";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("Detail", new { id = formSaleRow.UID });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelSaleRow(FormSaleRow formSaleRow)
        {
            string message = string.Empty;

            StandartTicket model = new StandartTicket();

            if (formSaleRow != null && formSaleRow.UID != null)
            {
                var saleRow = Db.TicketSaleRows.FirstOrDefault(x => x.UID == formSaleRow.UID);

                if (saleRow != null)
                {
                    var self = new TicketSaleRows()
                    {
                        TicketTripID = saleRow.TicketTripID,
                        AnimalCostumeTypeID = saleRow.AnimalCostumeTypeID,
                        Currency = saleRow.Currency,
                        CustomerData = saleRow.CustomerData,
                        UID = saleRow.UID,
                        CustomerName = saleRow.CustomerName,
                        Date = saleRow.Date,
                        Description = saleRow.Description,
                        DeviceID = saleRow.DeviceID,
                        Discount = saleRow.Discount,
                        EmployeeID = saleRow.EmployeeID,
                        ExtraUnit = saleRow.ExtraUnit,
                        ID = saleRow.ID,
                        IsExchangable = saleRow.IsExchangable,
                        IsPromotion = saleRow.IsPromotion,
                        IsSale = saleRow.IsSale,
                        Latitude = saleRow.Latitude,
                        LocationID = saleRow.LocationID,
                        Longitude = saleRow.Longitude,
                        MallMotoColorID = saleRow.MallMotoColorID,
                        PaymethodID = saleRow.PaymethodID,
                        PrePaid = saleRow.PrePaid,
                        Price = saleRow.Price,
                        PriceCategoryID = saleRow.PriceCategoryID,
                        PriceID = saleRow.PriceID,
                        PromotionID = saleRow.PromotionID,
                        Quantity = saleRow.Quantity,
                        RecordDate = saleRow.RecordDate,
                        RecordEmployeeID = saleRow.RecordEmployeeID,
                        SaleID = saleRow.SaleID,
                        StatusID = saleRow.StatusID,
                        TicketNumber = saleRow.TicketNumber,
                        TicketTypeID = saleRow.TicketTypeID,
                        Total = saleRow.Total,
                        Unit = saleRow.Unit,
                        UpdateDate = saleRow.UpdateDate,
                        UpdateEmployeeID = saleRow.UpdateEmployeeID,
                        UseImmediately = saleRow.UseImmediately,
                        ExtraPrice = saleRow.ExtraPrice,
                        ParentID = saleRow.ParentID
                    };


                    var rowid = Db.UpdateLocationTicketSale(saleRow.LocationID, saleRow.ID, saleRow.Date, saleRow.PriceID, saleRow.ExtraUnit, saleRow.PaymethodID, 4, model.Authentication.CurrentEmployee.EmployeeID, saleRow.MallMotoColorID, saleRow.AnimalCostumeTypeID, formSaleRow.Description).FirstOrDefault();

                    if (rowid != null && rowid > 0)
                    {
                        using (ActionTimeEntities _db = new ActionTimeEntities())
                        {
                            saleRow = _db.TicketSaleRows.FirstOrDefault(x => x.ID == saleRow.ID);
                        }

                        model.Result.IsSuccess = true;
                        model.Result.Message = "Satış İptal Başarılı";
                        DateTime dateProcess = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone);

                        var isequal = LocationHelper.PublicInstancePropertiesEqual<TicketSaleRows>(self, saleRow, LocationHelper.getIgnorelist());
                        LocationHelper.AddApplicationLog("Location", "TicketSaleRows", "Update", saleRow.ID.ToString(), "Home", "UpdateSaleRow", isequal, true, message, string.Empty, saleRow.Date, $"{model.Authentication.CurrentEmployee.EmployeeID} - {model.Authentication.CurrentEmployee.FullName}", LocationHelper.GetIPAddress(), string.Empty, null);

                        System.Threading.Tasks.Task lblusatask = System.Threading.Tasks.Task.Factory.StartNew(() => documentManager.CheckLocationTicketSale(rowid.Value, dateProcess));

                    }
                    else
                    {
                        model.Result.Message = "Satış İptal Başarısız";
                    }
                }
                else
                {
                    model.Result.Message = "Satış Bulunamadı";
                }
            }
            else
            {
                model.Result.Message = "Form Verisi Alınamadı";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("Detail", new { id = formSaleRow.UID });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RefundSaleRow(FormSaleRow formSaleRow)
        {
            string message = string.Empty;

            StandartTicket model = new StandartTicket();

            if (formSaleRow != null && formSaleRow.UID != null)
            {
                var saleRow = Db.TicketSaleRows.FirstOrDefault(x => x.UID == formSaleRow.UID);

                if (saleRow != null)
                {
                    var processdate = DateTime.UtcNow.AddHours(model.Location.TimeZone);
                    model.PayMethodID = formSaleRow.PayMethodID;
                    string ip = LocationHelper.GetIPAddress();

                    var rowid = Db.AddLocationTicketSale(saleRow.ID, model.Location.ID, processdate, saleRow.PriceID, model.PayMethodID, 2, model.Authentication.CurrentEmployee.EmployeeID, saleRow.MallMotoColorID, saleRow.AnimalCostumeTypeID, "", ip, 5, formSaleRow.Description).FirstOrDefault();

                    if (rowid != null && rowid > 0)
                    {
                        model.Result.IsSuccess = true;
                        model.Result.Message = "Satış İadesi Eklendi";

                        var refsaleRow = new TicketSaleRows();

                        using (ActionTimeEntities _db = new ActionTimeEntities())
                        {
                            refsaleRow = _db.TicketSaleRows.FirstOrDefault(x => x.ID == rowid);
                        }

                        LocationHelper.AddApplicationLog("Location", "TicketSaleRows", "Insert", rowid.ToString(), "Home", "SetTicketSale", null, true, message, string.Empty, processdate, $"{model.Authentication.CurrentEmployee.EmployeeID} - {model.Authentication.CurrentEmployee.FullName}", LocationHelper.GetIPAddress(), string.Empty, refsaleRow);

                        System.Threading.Tasks.Task lblusatask = System.Threading.Tasks.Task.Factory.StartNew(() => documentManager.CheckLocationTicketSale(rowid.Value, refsaleRow.Date));

                    }
                    else
                    {
                        model.Result.Message = "İade Başarısız";
                    }
                }
                else
                {
                    model.Result.Message = "Satış Bulunamadı";
                }
            }
            else
            {
                model.Result.Message = "Form Verisi Alınamadı";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("Detail", new { id = formSaleRow.UID });
        }

    }
}